using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// Gerencia o fluxo do jogo: briefing com contagem regressiva (jogador congelado),
// respawn, mensagens na tela e vitória.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool IsBriefing { get; private set; }

    public Transform player;
    public Transform initialSpawn;
    public Text messageText;
    public Text objectiveText;    // canto: "CENA 1/3"
    public Text promptText;       // "[E] ..." quando perto de uma ação
    public GameObject briefingPanel;
    public Text briefingText;
    public Text countdownText;
    public Text timerText;        // cronômetro grande durante a sala
    public GameObject memoryPanel; // overlay escuro do Modo Memória [Q]
    public Text memoryText;        // protocolo relembrado na tela
    public GameObject winPanel;
    public Image flashImage; // flash azul ao teletransportar

    [Tooltip("Tempo (segundos) para concluir cada sala antes da vítima morrer")]
    public float roomTimeLimit = 15f;
    float timeLeft;
    bool timerRunning;
    string currentProtocol = ""; // texto do protocolo da sala atual (mostrado no [Q])

    [Tooltip("Distância entre os centros das salas no eixo X (igual ao ROOM_SPACING do SceneBuilder)")]
    public float roomSpacing = 80f;

    Transform currentSpawn;
    int roomNumber = 1;

    void Awake()
    {
        Instance = this;
        IsBriefing = false;
    }

    void Start()
    {
        currentSpawn = initialSpawn;
        if (winPanel != null) winPanel.SetActive(false);
        if (briefingPanel != null) briefingPanel.SetActive(false);
        SetPrompt(""); // o "CENA X/3" aparece após o briefing
        IndexRoomLabels(); // só os textos da sala atual ficam visíveis
    }

    // ---- Visibilidade dos textos por sala ----
    // Os textos 3D (TextMesh: título, eco do briefing, marcadores) usam o shader
    // "GUI/Text Shader", que tem Fog desligado. Por isso a névoa esconde a geometria
    // das outras salas, mas NÃO os textos. Solução: mostrar só os textos da sala atual.
    struct RoomLabel
    {
        public MeshRenderer renderer;
        public int room;
        public bool isEcho; // pista azul, só aparece no Modo Memória [Q]
    }

    readonly List<RoomLabel> roomLabels = new List<RoomLabel>();
    int shownRoom = -1;
    bool shownMemoryMode;

    void IndexRoomLabels()
    {
        roomLabels.Clear();
        foreach (TextMesh tm in FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
        {
            MeshRenderer mr = tm.GetComponent<MeshRenderer>();
            if (mr == null) continue;
            roomLabels.Add(new RoomLabel
            {
                renderer = mr,
                room = RoomIndexAt(tm.transform.position.x),
                isEcho = tm.CompareTag("MemoryEcho")
            });
        }
        shownRoom = -1; // força aplicar na primeira atualização
        UpdateLabelVisibility();
    }

    int RoomIndexAt(float worldX)
    {
        return Mathf.Max(0, Mathf.RoundToInt(worldX / roomSpacing));
    }

    void UpdateLabelVisibility()
    {
        if (player == null || roomLabels.Count == 0) return;

        int currentRoom = RoomIndexAt(player.position.x);
        bool memoryMode = ModeSwitcher.Instance != null && ModeSwitcher.Instance.InMemoryMode;
        if (currentRoom == shownRoom && memoryMode == shownMemoryMode) return;

        foreach (RoomLabel label in roomLabels)
        {
            if (label.renderer == null) continue;
            label.renderer.enabled = label.room == currentRoom && (!label.isEcho || memoryMode);
        }
        shownRoom = currentRoom;
        shownMemoryMode = memoryMode;
    }

    void Update()
    {
        UpdateLabelVisibility();
        TickTimer();
        UpdateMemoryPanel();
    }

    // Modo Memória [Q]: mostra o protocolo na TELA (painel escuro), não mais na parede.
    void UpdateMemoryPanel()
    {
        if (memoryPanel == null) return;
        bool show = ModeSwitcher.Instance != null && ModeSwitcher.Instance.InMemoryMode && !IsBriefing && !failing;
        if (memoryPanel.activeSelf != show)
        {
            if (show && memoryText != null) memoryText.text = currentProtocol;
            memoryPanel.SetActive(show);
        }
    }

    // ---- Cronômetro de 15 s por sala ----
    // Começa quando o briefing termina; se zerar, a vítima morre e o treinamento
    // reinicia. É parado quando o resgate da sala é concluído.
    public void StartTimer()
    {
        timeLeft = roomTimeLimit;
        timerRunning = true;
        UpdateTimerUI();
    }

    public void StopTimer()
    {
        timerRunning = false;
        if (timerText != null) timerText.text = "";
    }

    void TickTimer()
    {
        if (!timerRunning || IsBriefing || failing) return;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            timerRunning = false;
            UpdateTimerUI();
            FailAndRestart("A VÍTIMA MORREU!\n\nO tempo de 15 segundos acabou.\n\nNum resgate real, cada segundo conta.\nSeja mais rápido na próxima vez!");
            return;
        }
        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;
        timerText.text = Mathf.CeilToInt(timeLeft) + "s";
        timerText.color = timeLeft <= 5f ? new Color(1f, 0.3f, 0.25f) : Color.white;
    }

    void SetObjective()
    {
        if (objectiveText != null)
            objectiveText.text = "CENA " + roomNumber + "/3";
    }

    public void SetPrompt(string msg)
    {
        if (promptText != null) promptText.text = msg;
    }

    Coroutine briefingRoutine;

    // Congela o jogador, mostra o protocolo com contagem regressiva, depois libera.
    // Para apenas o briefing anterior (não o flash de teleporte, que toca por cima).
    public void StartBriefing(string text, float seconds)
    {
        currentProtocol = text; // guarda o protocolo desta sala para o Modo Memória [Q]
        if (briefingRoutine != null) StopCoroutine(briefingRoutine);
        briefingRoutine = StartCoroutine(BriefingRoutine(text, seconds));
    }

    IEnumerator BriefingRoutine(string text, float seconds)
    {
        IsBriefing = true;
        SetFrozen(true);
        SetPrompt("");
        if (messageText != null) messageText.text = "";
        if (objectiveText != null) objectiveText.text = ""; // some durante a contagem
        if (briefingPanel != null) briefingPanel.SetActive(true);
        if (briefingText != null) briefingText.text = text;

        float t = seconds;
        while (t > 0f)
        {
            if (countdownText != null)
                countdownText.text = "Memorize!  " + Mathf.CeilToInt(t);
            // (contagem regressiva em segundos)
            t -= Time.deltaTime;
            yield return null;
        }

        if (briefingPanel != null) briefingPanel.SetActive(false);
        SetFrozen(false);
        IsBriefing = false;
        SetObjective(); // o "CENA X/3" só aparece depois da contagem
        StartTimer();   // dispara os 15 s: se zerar, a vítima morre
        ShowMessage("Siga o protocolo na ordem ('!') antes que o tempo acabe!  [Q] relembra o protocolo.");
    }

    void SetFrozen(bool frozen)
    {
        if (player == null) return;
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = !frozen;
    }

    public void NextRoom(Transform spawn)
    {
        currentSpawn = spawn;
        roomNumber++;
        SetObjective();
    }

    public void RestartRoom()
    {
        FailAndRestart("A VÍTIMA MORREU!\n\nO tempo acabou: as paredes se fecharam\nantes de você completar o resgate.\n\nSeja mais rápido desta vez!");
    }

    bool failing;

    // Errou a ordem do protocolo (ou foi esmagado): mostra o motivo e
    // reinicia o treinamento desde a CENA 1.
    public void FailAndRestart(string reason)
    {
        if (failing) return;
        failing = true;
        StopAllCoroutines();
        StartCoroutine(FailRoutine(reason));
    }

    IEnumerator FailRoutine(string reason)
    {
        IsBriefing = true; // congela paredes e interações
        StopTimer();
        SetFrozen(true);
        SetPrompt("");
        if (messageText != null) messageText.text = "";
        if (flashImage != null) flashImage.gameObject.SetActive(false);
        if (briefingPanel != null) briefingPanel.SetActive(true);
        if (briefingText != null)
        {
            briefingText.text = reason;
            briefingText.color = new Color(1f, 0.45f, 0.4f);
        }
        if (countdownText != null) countdownText.text = "Voltando ao início do treinamento...";

        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Win()
    {
        StopTimer();
        if (winPanel != null) winPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    // Flash de teletransporte (overlay azul que desvanece)
    public void Flash()
    {
        if (flashImage != null) StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        flashImage.gameObject.SetActive(true);
        float t = 0f;
        const float dur = 0.6f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime; // funciona até com o jogo pausado (vitória)
            flashImage.color = new Color(0.6f, 0.85f, 1f, Mathf.Lerp(0.85f, 0f, t / dur));
            yield return null;
        }
        flashImage.gameObject.SetActive(false);
    }

    public void ShowMessage(string msg)
    {
        if (messageText == null) return;
        messageText.text = msg;
        CancelInvoke(nameof(ClearMessage));
        Invoke(nameof(ClearMessage), 4f);
    }

    void ClearMessage()
    {
        if (messageText != null) messageText.text = "";
    }
}
