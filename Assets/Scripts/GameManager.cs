using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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
    public GameObject winPanel;
    public Image flashImage; // flash azul ao teletransportar

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
        ShowMessage("Siga o protocolo na ordem ('!').  [Q] relembra o protocolo (mas acelera as paredes).");
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
