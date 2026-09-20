using System.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHUDManager : NetworkBehaviour
{
    public static GameHUDManager Instance { get; private set; }

    [Header("Textos del Canvas")]
    [SerializeField] private TextMeshProUGUI oroText;
    [SerializeField] private TextMeshProUGUI hierbaText;
    [SerializeField] private TextMeshProUGUI sabiduriaText;
    [SerializeField] private TextMeshProUGUI puntosText;

    [Header("UI de Vida del Player")]
    [SerializeField] private TextMeshProUGUI vidaText;
    [SerializeField] private Slider vidaSlider;

    private PlayerStats jugadorLocalStats;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(EsperarYVincularJugador());
    }

    private IEnumerator EsperarYVincularJugador()
    {
        // Esperamos a salir de escenas no jugables (ej. lobby) si aplica
        while (SceneManager.GetActiveScene().name.ToLower() == "lobby")
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.2f);

        // Busca al jugador local en la red
        while (jugadorLocalStats == null)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
            {
                var jugadorObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
                if (jugadorObj != null)
                {
                    jugadorLocalStats = jugadorObj.GetComponent<PlayerStats>();
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        // Suscripción al evento personalizado OnStatsChanged
        jugadorLocalStats.OnStatsChanged += ActualizarPantallaVisual;

        // Suscripción directa a las NetworkVariables por respaldo
        SuscribirANetworkVariables();

        // Primera actualización visual
        ActualizarPantallaVisual();
    }

    private void SuscribirANetworkVariables()
    {
        if (jugadorLocalStats == null) return;

        jugadorLocalStats.oro.OnValueChanged += (vAnt, vNuevo) => ActualizarPantallaVisual();
        jugadorLocalStats.hierba.OnValueChanged += (vAnt, vNuevo) => ActualizarPantallaVisual();
        jugadorLocalStats.sabiduria.OnValueChanged += (vAnt, vNuevo) => ActualizarPantallaVisual();
        jugadorLocalStats.puntos.OnValueChanged += (vAnt, vNuevo) => ActualizarPantallaVisual();
        jugadorLocalStats.vidaActual.OnValueChanged += (vAnt, vNuevo) => ActualizarPantallaVisual();
    }

    private void ActualizarPantallaVisual()
    {
        if (jugadorLocalStats == null) return;

        // Actualizamos los contadores de los 3 recursos + puntos
        if (oroText != null) oroText.text = "Oro: " + jugadorLocalStats.oro.Value;
        if (hierbaText != null) hierbaText.text = "Hierba: " + jugadorLocalStats.hierba.Value;
        if (sabiduriaText != null) sabiduriaText.text = "Sabiduría: " + jugadorLocalStats.sabiduria.Value;
        if (puntosText != null) puntosText.text = "Puntos: " + jugadorLocalStats.puntos.Value;

        // Actualizamos la barra y texto de vida
        int vidaAct = jugadorLocalStats.vidaActual.Value;
        int vidaMax = jugadorLocalStats.GetVidaMaxima();

        if (vidaText != null)
        {
            vidaText.text = $"Vida: {vidaAct} / {vidaMax}";
        }

        if (vidaSlider != null)
        {
            vidaSlider.maxValue = vidaMax;
            vidaSlider.value = vidaAct;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (jugadorLocalStats != null)
        {
            jugadorLocalStats.OnStatsChanged -= ActualizarPantallaVisual;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}