using UnityEngine;

// Mecânica "enclosing space": duas paredes da sala se aproximam lentamente.
// Se encostarem no jogador, a sala reinicia (GameManager.RestartRoom).
// No Modo Memória as paredes avançam mais rápido (pressão por usar a memória).
// Como usar: coloque este script num objeto vazio da sala e arraste as duas paredes
// (Cubes) para wallA e wallB. Elas se movem uma em direção à outra no eixo X local.
public class EnclosingRoom : MonoBehaviour
{
    public Transform wallA;
    public Transform wallB;
    [Tooltip("Velocidade em metros/segundo no modo normal")]
    public float closeSpeed = 0.05f;
    [Tooltip("Multiplicador no Modo Memória")]
    public float memoryModeMultiplier = 3f;
    [Tooltip("Distância mínima entre as paredes antes de 'esmagar' (game over da sala)")]
    public float minGap = 1.2f;

    bool stopped;

    public void StopClosing() { stopped = true; } // chamado quando o puzzle é resolvido

    void Update()
    {
        if (stopped || wallA == null || wallB == null) return;
        if (GameManager.IsBriefing) return; // pausa durante a memorização

        float speed = closeSpeed;
        if (ModeSwitcher.Instance != null && ModeSwitcher.Instance.InMemoryMode)
            speed *= memoryModeMultiplier;

        Vector3 dir = (wallB.position - wallA.position).normalized;
        wallA.position += dir * speed * Time.deltaTime;
        wallB.position -= dir * speed * Time.deltaTime;

        if (Vector3.Distance(wallA.position, wallB.position) <= minGap)
            GameManager.Instance.RestartRoom();
    }
}
