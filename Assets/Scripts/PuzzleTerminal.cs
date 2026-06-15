using UnityEngine;
using UnityEngine.UI;

// Objetivo "solve" + educacional: terminal com pergunta de múltipla escolha sobre o
// conteúdo das pistas. Acertou -> para as paredes e ativa o teleporte. Errou -> nada
// (pode tentar de novo; as paredes continuam fechando).
// Como usar: crie um Cube (o terminal), adicione este script e um BoxCollider trigger maior.
// Preencha pergunta/respostas no Inspector. Ligue os campos de UI (ver README) e o Teleporter da sala.
[RequireComponent(typeof(Collider))]
public class PuzzleTerminal : MonoBehaviour
{
    [Header("Conteúdo educacional")]
    [TextArea] public string question = "Qual é a resposta correta?";
    public string[] answers = new string[3] { "A", "B", "C" };
    public int correctIndex = 0;

    [Header("Ligações da sala")]
    public EnclosingRoom room;
    public Teleporter exitTeleporter;

    [Header("UI (painel com 1 Text de pergunta e 3 Buttons)")]
    public GameObject panel;
    public Text questionText;
    public Button[] answerButtons;

    bool playerNear;
    bool solved;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        if (panel != null) panel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = true;
        if (!solved) GameManager.Instance.SetPrompt("[E] Responder o terminal");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = false;
        GameManager.Instance.SetPrompt("");
        ClosePanel();
    }

    void Update()
    {
        if (playerNear && !solved && Input.GetKeyDown(KeyCode.E))
            OpenPanel();
    }

    void OpenPanel()
    {
        if (panel == null) return;
        panel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        questionText.text = question;
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int idx = i;
            answerButtons[i].GetComponentInChildren<Text>().text = answers[i];
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => Answer(idx));
        }
    }

    void Answer(int idx)
    {
        if (idx == correctIndex)
        {
            solved = true;
            if (room != null) room.StopClosing();
            if (exitTeleporter != null) exitTeleporter.Activate();
            GameManager.Instance.SetPrompt("");
            GameManager.Instance.ShowMessage("Correto! As paredes pararam. Entre no teleporte VERDE para a proxima cena.");
        }
        else
        {
            GameManager.Instance.ShowMessage("Errado... tente lembrar. As paredes continuam fechando!");
        }
        ClosePanel();
    }

    void ClosePanel()
    {
        if (panel != null) panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
