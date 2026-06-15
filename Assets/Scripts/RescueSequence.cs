using UnityEngine;

// Controla a sequência de ações de resgate de uma sala (protocolo de primeiros socorros).
// As ações (InteractionStep) precisam ser feitas NA ORDEM. Quando todas são concluídas,
// trava as paredes e ativa o teleporte de saída.
public class RescueSequence : MonoBehaviour
{
    public EnclosingRoom room;
    public Teleporter exitTeleporter;
    public InteractionStep[] steps; // na ordem correta do protocolo

    int current;

    public bool IsCurrent(InteractionStep s)
    {
        return current < steps.Length && steps[current] == s;
    }

    public string CurrentHint
    {
        get { return current < steps.Length ? steps[current].actionName : ""; }
    }

    public void CompleteStep(InteractionStep s)
    {
        current++;
        if (current >= steps.Length)
        {
            if (room != null) room.StopClosing();
            if (exitTeleporter != null) exitTeleporter.Activate();
            GameManager.Instance.ShowMessage("RESGATE CONCLUÍDO! As paredes pararam. Entre no feixe VERDE para a próxima cena.");
        }
        else
        {
            GameManager.Instance.ShowMessage("Próximo passo: " + CurrentHint);
        }
    }
}
