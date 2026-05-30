using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using System.Collections;

public class Lobby : MonoBehaviour
{
    private string ipServidor = "127.0.0.1";

    private bool hostDetectado = false;
    private bool buscandoHost = false;
    private string mensajeEstado = "Elige tu rol para comenzar.";

    private void Start()
    {
        hostDetectado = false;
        buscandoHost = false;
        mensajeEstado = "Elige tu rol para comenzar.";

        // if la red quedó abierta a medias, forzamos un apagado limpio aquí también
        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            // Esto evita que el evento se ejecute 2 o 3 veces seguidas en la segunda partida.
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

    private void AlConectarseConExito(ulong id)
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            hostDetectado = true;
            buscandoHost = false;
            mensajeEstado = "✅ ¡Conectado exitosamente al Lobby!";
        }
    }

    private void AlDesconectarseDelServidor(ulong idCliente)
    {
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.IsServer) return;

        if (buscandoHost)
        {
            hostDetectado = false;
            buscandoHost = false;
            mensajeEstado = "❌ El Host aún no ha iniciado la partida.";
            return;
        }

        if (SceneManager.GetActiveScene().name == "lobby") 
        {
            hostDetectado = false;
            buscandoHost = false;
            mensajeEstado = "Partida terminada de forma limpia. Elige tu rol.";
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
        mensajeEstado = "🔍 Buscando Host en la red...";

        ConfigurarIpTransporte(ipServidor);
        NetworkManager.Singleton.StartClient();

        // Esperamos un máximo de 4 segundos a que Netcode conecte
        float tiempoEspera = 0f;
        while (tiempoEspera < 4f && !hostDetectado)
        {
            // if el motor nativo de Netcode confirma la conexión exitosa
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
            {
                hostDetectado = true;
                buscandoHost = false;
                mensajeEstado = "✅ ¡Host encontrado! Entrando...";
                yield break; //  Salimos de la corrutina exitosamente y NO apagamos la red.
            }

            tiempoEspera += Time.deltaTime;
            yield return null;
        }

        // Si pasaron los 4 segundos y nunca cambió 'hostDetectado' a true (el Host realmente no estaba)
        if (!hostDetectado)
        {
            if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
            buscandoHost = false;
            mensajeEstado = "❌ El Host aún no ha iniciado la partida o la IP es incorrecta.";
        }
    }

    private void OnGUI()
    {
        if (NetworkManager.Singleton == null) return;

        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 380));

            GUILayout.Label("Alumno: Pablito Martinez");
            GUILayout.Box($"Estado: {mensajeEstado}");
            GUILayout.Space(20);

            if (GUILayout.Button("Crear Partida (Host)"))
            {
                NetworkManager.Singleton.StartHost();
            }

            GUILayout.Space(20);
            GUILayout.Label("Dirección IP del Servidor:");
            ipServidor = GUILayout.TextField(ipServidor, 30);

            if (!hostDetectado && !buscandoHost)
            {
                if (GUILayout.Button("🔎 Verificar si el Host ya entró"))
                {
                    StartCoroutine(ComprobarSiExisteHost());
                }
            }

            if (hostDetectado && !buscandoHost)
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Unirse a Partida (Client)"))
                {
                    ConfigurarIpTransporte(ipServidor);
                    NetworkManager.Singleton.StartClient();
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.EndArea();
        }
        else
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            int jugadoresConectados = NetworkManager.Singleton.ConnectedClients.Count;
            GUILayout.Label($"Jugadores en el lobby: {jugadoresConectados} / 2");

            if (NetworkManager.Singleton.IsServer)
            {
                GUILayout.Label("Esperando a que el cliente se conecte...");
                if (jugadoresConectados >= 2)
                {
                    if (GUILayout.Button("¡EMPEZAR JUEGO!"))
                    {
                        NetworkManager.Singleton.SceneManager.LoadScene("game", LoadSceneMode.Single);
                    }
                }
                else
                {
                    GUILayout.Box("Esperando más jugadores para poder iniciar...");
                }
            }
            else
            {
                GUILayout.Label("¡Conectado! Esperando que el Host inicie la partida...");
            }
            GUILayout.EndArea();
        }
    }

    private void ConfigurarIpTransporte(string nuevaIp)
    {
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.gameObject.TryGetComponent<UnityTransport>(out UnityTransport transporte))
        {
            transporte.ConnectionData.Address = nuevaIp.Trim();
        }
    }
}