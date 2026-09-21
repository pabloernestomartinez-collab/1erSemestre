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
    [SerializeField] private TextMeshProUGUI puntosText;

    [Header("Conteo de Armas")]
    [SerializeField] private TextMeshProUGUI espadasText;
    [SerializeField] private TextMeshProUGUI dagasText;

    [Header("UI de Vida del Player")]
    [SerializeField] private TextMeshProUGUI vidaText;
    [SerializeField] private Slider vidaSlider;

    private PlayerStats jugadorLocalStats;
    private InventarioPlayer jugadorLocalInventario;

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

        // Espera activa hasta encontrar el objeto jugador de la sesión local de Netcode
        while (jugadorLocalStats == null)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
            {
                var jugadorObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
                if (jugadorObj != null)
                {
                    if (jugadorObj.TryGetComponent<PlayerStats>(out var stats))
                    {
                        jugadorLocalStats = stats;
                    }

                    if (jugadorObj.TryGetComponent<InventarioPlayer>(out var inv))
                    {
                        jugadorLocalInventario = inv;
                        jugadorLocalInventario.OnInventarioCambiado += ActualizarPantallaVisual;
                    }
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
        jugadorLocalStats.puntos.OnValueChanged += OnVariableChanged;
        jugadorLocalStats.vidaActual.OnValueChanged += OnVariableChanged;
    }

    private void DesuscribirDeNetworkVariables()
    {
        if (jugadorLocalStats == null) return;

        jugadorLocalStats.oro.OnValueChanged -= OnVariableChanged;
        jugadorLocalStats.hierba.OnValueChanged -= OnVariableChanged;
        jugadorLocalStats.sabiduria.OnValueChanged -= OnVariableChanged;
        jugadorLocalStats.puntos.OnValueChanged -= OnVariableChanged;
        jugadorLocalStats.vidaActual.OnValueChanged -= OnVariableChanged;
    }

    private void OnVariableChanged(int valorAnterior, int valorNuevo)
    {
        ActualizarPantallaVisual();
    }

    public void ActualizarPantallaVisual()
    {
        if (jugadorLocalStats == null) return;

        if (oroText != null) oroText.text = "Oro: " + jugadorLocalStats.oro.Value;
        if (hierbaText != null) hierbaText.text = "Hierba: " + jugadorLocalStats.hierba.Value;
        if (sabiduriaText != null) sabiduriaText.text = "Sabiduría: " + jugadorLocalStats.sabiduria.Value;
        if (puntosText != null) puntosText.text = "Puntos: " + jugadorLocalStats.puntos.Value;

        ActualizarConteoArmas();

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

    private void ActualizarConteoArmas()
    {
        if (jugadorLocalInventario == null) return;

        int cantidadEspadas = 0;
        int cantidadDagas = 0;

        foreach (itemsMenu item in jugadorLocalInventario.listaDeItems)
        {
            if (item == null) continue;

            string nombre = item.nombreArma.ToLower();

            if (nombre.Contains("espada"))
            {
                cantidadEspadas++;
            }
            else if (nombre.Contains("daga"))
            {
                cantidadDagas++;
            }
        }

        if (espadasText != null) espadasText.text = "Espadas: " + cantidadEspadas;
        if (dagasText != null) dagasText.text = "Dagas: " + cantidadDagas;
    }

    private void OnDestroy()
    {
        DesuscribirDeNetworkVariables();

        if (jugadorLocalStats != null)
        {
            jugadorLocalStats.OnStatsChanged -= ActualizarPantallaVisual;
        }

        if (jugadorLocalInventario != null)
        {
            jugadorLocalInventario.OnInventarioCambiado -= ActualizarPantallaVisual;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}