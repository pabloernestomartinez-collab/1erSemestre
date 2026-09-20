using Unity.Netcode;
using UnityEngine;
using TMPro;

public class UIMochila : MonoBehaviour
{
    [Header("Referencias de UI (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI textoVida;
    [SerializeField] private TextMeshProUGUI textoOro;
    [SerializeField] private TextMeshProUGUI textoHierba;
    [SerializeField] private TextMeshProUGUI textoSabiduria;

    private PlayerStats localPlayerStats;

    private void Update()
    {
        // 1. Buscamos el PlayerStats del jugador local si aún no lo tenemos vinculado
        if (localPlayerStats == null)
        {
            VincularmconJugadorLocal();
        }
    }

    private void VincularmconJugadorLocal()
    {
        var localClient = NetworkManager.Singleton?.LocalClient;
        if (localClient != null && localClient.PlayerObject != null)
        {
            if (localClient.PlayerObject.TryGetComponent<PlayerStats>(out var stats))
            {
                localPlayerStats = stats;
                SuscribirAEventos();
                ActualizarTodaLaUI();
            }
        }
    }

    private void SuscribirAEventos()
    {
        if (localPlayerStats == null) return;

        localPlayerStats.vidaActual.OnValueChanged += AlCambiarVida;
        localPlayerStats.oro.OnValueChanged += AlCambiarOro;
        localPlayerStats.hierba.OnValueChanged += AlCambiarHierba;
        localPlayerStats.sabiduria.OnValueChanged += AlCambiarSabiduria;
    }

    private void OnDisable()
    {
        if (localPlayerStats != null)
        {
            localPlayerStats.vidaActual.OnValueChanged -= AlCambiarVida;
            localPlayerStats.oro.OnValueChanged -= AlCambiarOro;
            localPlayerStats.hierba.OnValueChanged -= AlCambiarHierba;
            localPlayerStats.sabiduria.OnValueChanged -= AlCambiarSabiduria;
        }
    }

    // --- MÉTODOS DE ACTUALIZACIÓN INDIVIDUAL ---

    private void AlCambiarVida(int valorAnterior, int valorNuevo)
    {
        if (textoVida != null)
            textoVida.text = $"Vida: {valorNuevo} / {localPlayerStats.vidaMaxima}";
    }

    private void AlCambiarOro(int valorAnterior, int valorNuevo)
    {
        if (textoOro != null)
            textoOro.text = $"Oro: {valorNuevo}";
    }

    private void AlCambiarHierba(int valorAnterior, int valorNuevo)
    {
        if (textoHierba != null)
            textoHierba.text = $"Hierba: {valorNuevo}";
    }

    private void AlCambiarSabiduria(int valorAnterior, int valorNuevo)
    {
        if (textoSabiduria != null)
            textoSabiduria.text = $"Sabiduría: {valorNuevo}";
    }

    // Actualiza todos los valores de golpe cuando abre el Canvas por primera vez
    private void ActualizarTodaLaUI()
    {
        if (localPlayerStats == null) return;

        AlCambiarVida(0, localPlayerStats.vidaActual.Value);
        AlCambiarOro(0, localPlayerStats.oro.Value);
        AlCambiarHierba(0, localPlayerStats.hierba.Value);
        AlCambiarSabiduria(0, localPlayerStats.sabiduria.Value);
    }
}