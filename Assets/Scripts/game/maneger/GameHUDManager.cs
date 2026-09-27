using System.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHUDManager : MonoBehaviour
{
    public static GameHUDManager Instance { get; private set; }

    [Header("Textos del Canvas")]
    [SerializeField] private TextMeshProUGUI oroText;
    [SerializeField] private TextMeshProUGUI hierbaText;
    [SerializeField] private TextMeshProUGUI sabiduriaText;
    [SerializeField] private TextMeshProUGUI puntosText; // Muestra la Puntuación/Score del jugador

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
        jugadorLocalStats.puntuacion.OnValueChanged += OnVariableChanged; // Suscripción a puntuación real
        jugadorLocalStats.puntosVida.OnValueChanged += OnVariableChanged;
        jugadorLocalStats.puntosVidaMax.OnValueChanged += OnVariableChanged;
    }

    private void DesuscribirDeNetworkVariables()
    {
        if (jugadorLocalStats == null) return;

        jugadorLocalStats.oro.OnValueChanged -= OnVariableChanged;
        jugadorLocalStats.hierba.OnValueChanged -= OnVariableChanged;
        jugadorLocalStats.sabiduria.OnValueChanged -= OnVariableChanged;
        jugadorLocalStats.puntuacion.OnValueChanged -= OnVariableChanged; // Desuscripción de puntuación real
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
        if (puntosText != null) puntosText.text = jugadorLocalStats.puntuacion.Value.ToString(); // Muestra el valor real de puntuación

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