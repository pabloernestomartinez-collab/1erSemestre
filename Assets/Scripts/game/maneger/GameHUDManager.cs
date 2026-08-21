using System.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameHUDManager : NetworkBehaviour
{
    public static GameHUDManager Instance { get; private set; }

    [Header("Textos del Canvas")] //respetar el orden: Hierro, Madera, Fuego, Agua, Piedra
    [SerializeField] private TextMeshProUGUI hierro;
    [SerializeField] private TextMeshProUGUI madera;
    [SerializeField] private TextMeshProUGUI fuego;
    [SerializeField] private TextMeshProUGUI agua;
    [SerializeField] private TextMeshProUGUI piedra;

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

        // Actualizamos los strings usando el .Value de las NetworkVariables del jugador
        if (hierro != null) hierro.text = "Hierro: " + jugadorLocalStats.hierro.Value;
        if (madera != null) madera.text = "Madera: " + jugadorLocalStats.madera.Value;
        if (fuego != null) fuego.text = "Fuego: " + jugadorLocalStats.fuego.Value;
        if (agua != null) agua.text = "Agua: " + jugadorLocalStats.agua.Value;
        if (piedra != null) piedra.text = "Piedra: " + jugadorLocalStats.piedra.Value;
    }

    public override void OnNetworkDespawn()
    {
        if (jugadorLocalStats != null) jugadorLocalStats.OnStatsChanged -= ActualizarPantallaVisual;
        if (Instance == this) Instance = null;
    }
}