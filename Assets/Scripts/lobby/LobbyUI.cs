using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [Header("Paneles de la Interfaz")]
    [SerializeField] private GameObject panelMenuPrincipal;
    [SerializeField] private GameObject panelLobbyEspera;

    [Header("Componentes - Menú Principal")]
    [SerializeField] private TMP_Text estadoText;
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private Button botonCrearHost;
    [SerializeField] private Button botonVerificarHost;
    [SerializeField] private Button botonUnirseCliente;
    [SerializeField] private Button botonSalir;

    [Header("Componentes - Panel de Espera")]
    [SerializeField] private TMP_Text jugadoresConectadosText;
    [SerializeField] private TMP_Text estadoLobbyText;
    [SerializeField] private Button botonEmpezarJuego;

    private string ipServidor = "127.0.0.1";
    private bool hostDetectado = false;
    private bool buscandoHost = false;

    private void Start()
    {
        hostDetectado = false;
        buscandoHost = false;

        // Configuración inicial de UI
        if (ipInputField != null)
        {
            ipInputField.text = ipServidor;
            ipInputField.onValueChanged.AddListener(OnIpInputChanged);
        }

        // Asignación de listeners a botones
        if (botonCrearHost != null) botonCrearHost.onClick.AddListener(OnBotonCrearHostClicked);
        if (botonVerificarHost != null) botonVerificarHost.onClick.AddListener(OnBotonVerificarHostClicked);
        if (botonUnirseCliente != null) botonUnirseCliente.onClick.AddListener(OnBotonUnirseClienteClicked);
        if (botonEmpezarJuego != null) botonEmpezarJuego.onClick.AddListener(OnBotonEmpezarJuegoClicked);
        if (botonSalir != null) botonSalir.onClick.AddListener(OnBotonSalirClicked);

        ActualizarEstadoTexto("Elige tu rol para comenzar.");

        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
        {
            NetworkManager.Singleton.Shutdown();
        }

        MostrarPanelMenuPrincipal();
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= AlDesconectarseDelServidor;
            NetworkManager.Singleton.OnClientDisconnectCallback += AlDesconectarseDelServidor;

            NetworkManager.Singleton.OnClientConnectedCallback -= AlConectarseConExito;
            NetworkManager.Singleton.OnClientConnectedCallback += AlConectarseConExito;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= AlDesconectarseDelServidor;
            NetworkManager.Singleton.OnClientConnectedCallback -= AlConectarseConExito;
        }
    }

    private void Update()
    {
        // Actualiza dinámicamente la información del lobby de espera cuando está activo
        if (panelLobbyEspera != null && panelLobbyEspera.activeSelf && NetworkManager.Singleton != null)
        {
            int cantidadJugadores = NetworkManager.Singleton.ConnectedClients.Count;

            if (jugadoresConectadosText != null)
            {
                jugadoresConectadosText.text = $"Jugadores en el lobby: {cantidadJugadores} / 2";
            }

            if (NetworkManager.Singleton.IsServer)
            {
                if (estadoLobbyText != null)
                {
                    estadoLobbyText.text = cantidadJugadores >= 1
                        ? "¡Listo para iniciar la partida!"
                        : "Esperando más jugadores para poder iniciar...";
                }

                if (botonEmpezarJuego != null)
                {
                    botonEmpezarJuego.gameObject.SetActive(true);
                    botonEmpezarJuego.interactable = cantidadJugadores >= 1;
                }
            }
            else
            {
                if (estadoLobbyText != null)
                {
                    estadoLobbyText.text = "¡Conectado! Esperando que el Host inicie la partida...";
                }

                if (botonEmpezarJuego != null)
                {
                    botonEmpezarJuego.gameObject.SetActive(false);
                }
            }
        }
    }

    // --- MANEJADORES DE EVENTOS DE BOTONES ---

    private void OnIpInputChanged(string nuevaIp)
    {
        ipServidor = nuevaIp;
    }

    private void OnBotonCrearHostClicked()
    {
        ConfigurarIpTransporte(ipServidor);
        if (NetworkManager.Singleton.StartHost())
        {
            MostrarPanelLobbyEspera();
        }
        else
        {
            ActualizarEstadoTexto("Error al iniciar el Host.");
        }
    }

    private void OnBotonVerificarHostClicked()
    {
        StartCoroutine(ComprobarSiExisteHost());
    }

    private void OnBotonUnirseClienteClicked()
    {
        ConfigurarIpTransporte(ipServidor);
        if (NetworkManager.Singleton.StartClient())
        {
            ActualizarEstadoTexto("Entrando como cliente...");
            MostrarPanelLobbyEspera();
        }
        else
        {
            ActualizarEstadoTexto("Error al conectar como cliente.");
        }
    }

    private void OnBotonEmpezarJuegoClicked()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("game", LoadSceneMode.Single);
        }
    }

    private void OnBotonSalirClicked()
    {
        StartCoroutine(CierreOrdenadoMenu());
    }

    // --- LÓGICA DE CONEXIÓN Y RED ---

    private void AlConectarseConExito(ulong id)
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            hostDetectado = true;
            buscandoHost = false;
            ActualizarEstadoTexto("¡Conectado exitosamente al Lobby!");
            ActualizarVisibilidadBotonesCliente();
        }
    }

    private void AlDesconectarseDelServidor(ulong idCliente)
    {
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsServer) return;

        if (buscandoHost)
        {
            hostDetectado = false;
            buscandoHost = false;
            ActualizarEstadoTexto("El Host aún no ha iniciado la partida.");
            ActualizarVisibilidadBotonesCliente();
            return;
        }

        if (SceneManager.GetActiveScene().name == "lobby")
        {
            hostDetectado = false;
            buscandoHost = false;
            ActualizarEstadoTexto("Partida terminada de forma limpia. Elige tu rol.");
            ActualizarVisibilidadBotonesCliente();
            MostrarPanelMenuPrincipal();
            return;
        }

        NetworkManager.Singleton.Shutdown();
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator ComprobarSiExisteHost()
    {
        if (NetworkManager.Singleton == null) yield break;

        hostDetectado = false;
        buscandoHost = true;
        ActualizarEstadoTexto("Buscando Host en la red...");
        ActualizarVisibilidadBotonesCliente();

        ConfigurarIpTransporte(ipServidor);
        NetworkManager.Singleton.StartClient();

        float tiempoEspera = 0f;
        while (tiempoEspera < 4f && !hostDetectado)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
            {
                hostDetectado = true;
                buscandoHost = false;
                ActualizarEstadoTexto("¡Host encontrado! Entrando...");
                ActualizarVisibilidadBotonesCliente();
                yield break;
            }

            tiempoEspera += Time.deltaTime;
            yield return null;
        }

        if (!hostDetectado)
        {
            if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
            buscandoHost = false;
            ActualizarEstadoTexto("El Host aún no ha iniciado la partida o la IP es incorrecta.");
            ActualizarVisibilidadBotonesCliente();
        }
    }

    private IEnumerator CierreOrdenadoMenu()
    {
        ActualizarEstadoTexto("Cerrando aplicación...");

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        yield return null;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ConfigurarIpTransporte(string nuevaIp)
    {
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.gameObject.TryGetComponent<UnityTransport>(out UnityTransport transporte))
        {
            transporte.ConnectionData.Address = nuevaIp.Trim();
        }
    }

    // --- CONTROL DE UI Y PANELES ---

    private void ActualizarEstadoTexto(string mensaje)
    {
        if (estadoText != null)
        {
            estadoText.text = $"Estado: {mensaje}";
        }
    }

    private void ActualizarVisibilidadBotonesCliente()
    {
        if (botonVerificarHost != null)
        {
            botonVerificarHost.gameObject.SetActive(!hostDetectado && !buscandoHost);
        }

        if (botonUnirseCliente != null)
        {
            botonUnirseCliente.gameObject.SetActive(hostDetectado && !buscandoHost);
        }
    }

    private void MostrarPanelMenuPrincipal()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelLobbyEspera != null) panelLobbyEspera.SetActive(false);
        ActualizarVisibilidadBotonesCliente();
    }

    private void MostrarPanelLobbyEspera()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
        if (panelLobbyEspera != null) panelLobbyEspera.SetActive(true);
    }
}