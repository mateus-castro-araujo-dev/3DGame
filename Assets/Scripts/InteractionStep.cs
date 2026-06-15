using UnityEngine;

// Uma ação de resgate no mundo: chegue perto, aperte E. Se não for o passo atual
// do protocolo, avisa qual é o certo (errar a ordem é parte do desafio educacional).
// Ações longas (ex: RCP) exigem vários toques de E (pressesRequired).
// Um marcador "!" amarelo flutua sobre o objeto; vira "OK" verde quando concluído.
[RequireComponent(typeof(Collider))]
public class InteractionStep : MonoBehaviour
{
    public RescueSequence sequence;
    public string actionName;              // ex: "Desligar o fogao"
    [TextArea] public string successMessage; // explicação educacional ao concluir
    public int pressesRequired = 1;
    public TextMesh marker;

    bool done;
    bool near;
    int presses;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        near = true;
        if (!done) GameManager.Instance.SetPrompt("[E] " + actionName);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        near = false;
        GameManager.Instance.SetPrompt("");
    }

    void Update()
    {
        if (GameManager.IsBriefing) return;
        if (!near || done || !Input.GetKeyDown(KeyCode.E)) return;

        if (!sequence.IsCurrent(this))
        {
            GameManager.Instance.FailAndRestart("A VÍTIMA MORREU!\n\nVocê errou a ordem do protocolo.\n(Tentou: " + actionName + ")\n\nNum resgate real, cada passo fora de ordem\ncusta uma vida. Memorize melhor desta vez!");
            return;
        }

        presses++;
        if (presses < pressesRequired)
        {
            GameManager.Instance.SetPrompt("[E] " + actionName + "  (" + presses + "/" + pressesRequired + ")");
            return;
        }

        done = true;
        GameManager.Instance.SetPrompt("");
        GameManager.Instance.ShowMessage(successMessage);
        if (marker != null)
        {
            marker.text = "OK";
            marker.color = new Color(0.3f, 1f, 0.4f);
        }
        sequence.CompleteStep(this);
    }
}
