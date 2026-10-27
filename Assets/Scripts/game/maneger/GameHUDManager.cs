using System.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHUDManager : MonoBehaviour
{
    public static GameHUDManager Instance { get; private set; }

    // Propiedad alias para compatibilidad
    public static GameHUDManager Instancia => Instance;

    [Header("Textos del Canvas")]
    [SerializeField] private TextMeshProUGUI oroText;
    [SerializeField] private TextMeshProUGUI hierbaText;
    [SerializeField] private TextMeshProUGUI sabiduriaText;
    [SerializeField] private TextMeshProUGUI puntosText; // Score del jugador durante la partida

    [Header("UI de Vida del Player")]
    [SerializeField] private TextMeshProUGUI vidaText;
    [SerializeField] private Slider vidaSlider;

    [Header("UI de Game Over (Derrota)")]
    [SerializeField] private GameObject panelGameOver;
    [SerializeField] private TextMeshProUGUI puntuacionGameOverText;
    [SerializeField] private Button botonVolverGameOver;

    [Header("UI de Victoria (Boss Derrotado)")]
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private TextMeshProUGUI puntuacionVictoriaText;
    [SerializeField] private Button botonVolverVictoria;

    private PlayerStats jugadorLocalStats;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Ocultar paneles de fin de partida al iniciar
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);

        // Asignar los listeners de los botones
        if (botonVolverGameOver != null)
        {
            botonVolverGameOver.onClick.AddListener(VolverAlMenuPrincipal);
        }

        if (botonVolverVictoria != null)
        {
            botonVolverVictoria.onClick.AddListener(VolverAlMenuPrincipal);
        }
    }

    private void Start()
    {
        StartCoroutine(EsperarYVincularJugador());
    }

    private IEnumerator EsperarYVincularJugador()
    {
        while (SceneManager.GetActiveScene().name.ToLower() == "lobby")
        {
            yield return new WaitForSeconds(0.1f);
        }

        while (jugadorLocalStats == null)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
            {
                var jugadorObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
                if (jugadorObj != null && jugadorObj.TryGetComponent<PlayerStats>(out var stats))
                {
                    jugadorLocalStats = stats;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        jugadorLocalStats.OnStatsChanged += ActualizarPantallaVisual;
        SuscribirANetworkVariables();
        ActualizarPantallaVisual();
    }

    private void SuscribirANetworkVariables()
    {
        if (jugadorLocalStats == null) return;

        jugadorLocalStats.oro.OnValueChanged += OnVariableChanged;
        jugadorLocalStats.hierba.OnValueChanged += OnVariableChanged;
        jugadorLocalStats.sabiduria.OnValueChanged += OnVariableChanged;
        jugadorLocalStats.puntuacion.OnValueChanged += OnVariableChanged;
        jugadorLocalStats.puntosVida.OnValueChanged += OnVariableChanged;
        jugadorLocalStats.puntosVidaMax.OnValueChanged += OnVariableChanged;
    }

    private void DesuscribirDeNetworkVariables()
    {
        if (jugadorLocalStats == null) return;

        jugadorLocalStats.oro.OnValueChanged -= OnVariableChanged;
        jugadorLocalStats.hierba.OnValueChanged -= OnVariableChanged;
        jugadorLocalStats.sabiduria.OnValueChanged -= OnVariableChanged;
        jugadorLocalStats.puntuacion.OnValueChanged -= OnVariableChanged;
        jugadorLocalStats.puntosVida.OnValueChanged -= OnVariableChanged;
        jugadorLocalStats.puntosVidaMax.OnValueChanged -= OnVariableChanged;
    }

    private void OnVariableChanged(int valorAnterior, int valorNuevo)
    {
        ActualizarPantallaVisual();
    }

    public void ActualizarPantallaVisual()
    {
        if (jugadorLocalStats == null) return;

        if (oroText != null) oroText.text = jugadorLocalStats.oro.Value.ToString();
        if (hierbaText != null) hierbaText.text = jugadorLocalStats.hierba.Value.ToString();
        if (sabiduriaText != null) sabiduriaText.text = jugadorLocalStats.sabiduria.Value.ToString();
        if (puntosText != null) puntosText.text = jugadorLocalStats.puntuacion.Value.ToString();

        int vidaAct = jugadorLocalStats.GetVidaActual();
        int vidaMax = jugadorLocalStats.GetVidaMaxima();

        if (vidaText != null)
        {
            vidaText.text = $"{vidaAct} / {vidaMax}";
        }

        if (vidaSlider != null)
        {
            vidaSlider.maxValue = vidaMax;
            vidaSlider.value = vidaAct;
        }
    }

    // --- PANTALLAS DE FIN DE PARTIDA ---

    public void MostrarGameOver()
    {
        if (panelGameOver != null)
        {
            if (jugadorLocalStats != null && puntuacionGameOverText != null)
            {
                puntuacionGameOverText.text = $"Puntuación Final: {jugadorLocalStats.GetPuntuacion()}";
            }
            panelGameOver.SetActive(true);
        }
    }

    public void MostrarPantallaVictoria()
    {
        if (panelVictoria != null)
        {
            if (jugadorLocalStats != null && puntuacionVictoriaText != null)
            {
                puntuacionVictoriaText.text = $"Puntuación Final: {jugadorLocalStats.GetPuntuacion()}";
            }
            panelVictoria.SetActive(true);
        }
    }

    public void VolverAlMenuPrincipal()
    {
        // 1. Desconectar de la red de forma limpia
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        // 2. Cargar la escena del lobby
        SceneManager.LoadScene("lobby");
    }

    private void OnDestroy()
    {
        DesuscribirDeNetworkVariables();

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