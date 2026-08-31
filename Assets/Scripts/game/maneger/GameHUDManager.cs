using System.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 🔥 NUEVO: Necesario por si decides usar un Slider para la barra de vida

public class GameHUDManager : NetworkBehaviour
{
    public static GameHUDManager Instance { get; private set; }

    [Header("Textos del Canvas")] //respetar el orden: Hierro, Madera, Fuego, Agua, Piedra
    [SerializeField] private TextMeshProUGUI hierro;
    [SerializeField] private TextMeshProUGUI madera;
    [SerializeField] private TextMeshProUGUI fuego;
    [SerializeField] private TextMeshProUGUI agua;
    [SerializeField] private TextMeshProUGUI piedra;
    [SerializeField] private TextMeshProUGUI puntosText;

    [Header("UI de Vida del Player")]
    [SerializeField] private TextMeshProUGUI vidaText;
    [SerializeField] private Slider vidaSlider;

    private PlayerStats jugadorLocalStats;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(EsperarYVincularJugador());
    }

    private IEnumerator EsperarYVincularJugador()
    {
        // Esperamos a salir del lobby de forma segura si aplica
        while (SceneManager.GetActiveScene().name == "lobby")
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.2f);

        // Bucle para buscar al jugador asignado a este cliente local
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

        // Cuando lo encuentra, nos suscribimos a su evento de cambio de estadísticas
        jugadorLocalStats.OnStatsChanged += ActualizarPantallaVisual;

        // Hacemos la primera actualización para que no arranque en blanco
        ActualizarPantallaVisual();
    }

    private void ActualizarPantallaVisual()
    {
        if (jugadorLocalStats == null) return;

        if (hierro != null) hierro.text = "Hierro: " + jugadorLocalStats.hierro.Value;
        if (madera != null) madera.text = "Madera: " + jugadorLocalStats.madera.Value;
        if (fuego != null) fuego.text = "Fuego: " + jugadorLocalStats.fuego.Value;
        if (agua != null) agua.text = "Agua: " + jugadorLocalStats.agua.Value;
        if (piedra != null) piedra.text = "Piedra: " + jugadorLocalStats.piedra.Value;
        if (puntosText != null) puntosText.text = "Puntos: " + jugadorLocalStats.puntos.Value;

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
        if (jugadorLocalStats != null) jugadorLocalStats.OnStatsChanged -= ActualizarPantallaVisual;
        if (Instance == this) Instance = null;
    }
}