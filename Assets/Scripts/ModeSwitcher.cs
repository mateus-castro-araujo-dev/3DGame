using UnityEngine;

// Mecânica "switch modes": tecla Q alterna entre Modo Presente e Modo Memória.
// No Modo Memória: objetos da camada/tag "MemoryEcho" ficam visíveis (ecos das pistas),
// a tela ganha um tom azulado e as paredes do EnclosingRoom avançam mais rápido (custo de usar a memória).
// Como usar: coloque este script num objeto vazio "GameSystems".
// Marque os objetos-pista fantasma com a tag "MemoryEcho" (crie a tag no editor).
public class ModeSwitcher : MonoBehaviour
{
    public static ModeSwitcher Instance { get; private set; }
    public bool InMemoryMode { get; private set; }

    [Tooltip("Cor da luz ambiente no modo memória")]
    public Color memoryAmbient = new Color(0.25f, 0.35f, 0.7f);

    Color normalAmbient;

    void Awake()
    {
        Instance = this;
        normalAmbient = RenderSettings.ambientLight;
    }

    void Start()
    {
        ApplyMode();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !GameManager.IsBriefing)
        {
            InMemoryMode = !InMemoryMode;
            ApplyMode();
        }
    }

    void ApplyMode()
    {
        RenderSettings.ambientLight = InMemoryMode ? memoryAmbient : normalAmbient;
        // A visibilidade dos ecos (MemoryEcho) é controlada pelo GameManager, que
        // combina "Modo Memória" com "sala atual" — assim você só vê a pista da sua fase.
    }
}
