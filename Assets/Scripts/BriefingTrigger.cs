using UnityEngine;

// Ao entrar na sala, dispara o briefing do protocolo: jogador congelado,
// texto na tela com contagem regressiva. Dispara apenas uma vez.
[RequireComponent(typeof(Collider))]
public class BriefingTrigger : MonoBehaviour
{
    [TextArea] public string briefingText;
    public float seconds = 8f;

    bool consumed;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (consumed || !other.CompareTag("Player")) return;
        consumed = true;
        GameManager.Instance.StartBriefing(briefingText, seconds);
    }
}
