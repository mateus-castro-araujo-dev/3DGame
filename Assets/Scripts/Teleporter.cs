using UnityEngine;

// Mecânica "teleporte": começa desativado (cinza). Quando o puzzle da sala é resolvido,
// Activate() o deixa verde e funcional. Ao entrar nele, o jogador é teleportado para
// o ponto de destino (próxima sala) — ou vence o jogo se for o último.
// Como usar: crie um Cylinder achatado no chão, adicione este script, collider trigger.
// Crie um objeto vazio "SpawnSala2" na próxima sala e arraste para "destination".
[RequireComponent(typeof(Collider))]
public class Teleporter : MonoBehaviour
{
    public Transform destination;
    [Tooltip("Marque se este é o teleporte final (vitória)")]
    public bool isFinal;

    bool active;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        SetColor(new Color(0.45f, 0.45f, 0.5f)); // cinza: desativado
    }

    public void Activate()
    {
        active = true;
        SetColor(new Color(0.2f, 1f, 0.4f)); // verde: pode entrar
        Light l = GetComponentInChildren<Light>();
        if (l != null) { l.color = new Color(0.3f, 1f, 0.4f); l.intensity = 8f; }
    }

    // pinta o disco e o feixe (renderers filhos)
    void SetColor(Color c)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.material.color = c;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!active || !other.CompareTag("Player")) return;

        if (isFinal)
        {
            GameManager.Instance.Flash();
            GameManager.Instance.Win();
            return;
        }

        // Teleporta: CharacterController precisa ser desligado para mudar a posição
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        other.transform.position = destination.position;
        other.transform.rotation = destination.rotation;
        if (cc != null) cc.enabled = true;

        GameManager.Instance.Flash();
        GameManager.Instance.NextRoom(destination);
    }
}
