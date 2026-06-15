using UnityEngine;

// Mecânica "amnésia/memória limitada": a pista (texto na parede) aparece quando o
// jogador chega perto e desaparece após alguns segundos. Depois, só pode ser revista
// no Modo Memória (se houver um eco com tag MemoryEcho).
// Como usar: crie um 3D Object > 3D Text (TextMesh) com o conteúdo educacional,
// adicione este script e um BoxCollider (Is Trigger = ON, aumente o tamanho para a área de leitura).
[RequireComponent(typeof(Collider))]
public class FadingClue : MonoBehaviour
{
    [Tooltip("Segundos que a pista fica visível antes de 'ser esquecida'")]
    public float visibleSeconds = 6f;

    MeshRenderer rend;
    bool consumed;

    void Start()
    {
        rend = GetComponent<MeshRenderer>();
        rend.enabled = false; // começa invisível até o jogador chegar perto
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (consumed || !other.CompareTag("Player")) return;
        consumed = true;
        rend.enabled = true;
        Invoke(nameof(Forget), visibleSeconds);
    }

    void Forget()
    {
        rend.enabled = false; // "amnésia": a informação some
    }
}
