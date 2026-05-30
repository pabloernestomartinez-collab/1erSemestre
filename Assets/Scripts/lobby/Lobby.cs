using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
// Necesitamos acceder al transporte de Netcode para cambiar la IP
using Unity.Netcode.Transports.UTP;

public class Lobby : MonoBehaviour
{
    // Variable para almacenar la IP. Por defecto tiene el bucle local (tú mismo)
    private string ipServidor = "127.0.0.1";

    private void Start()
    {
        // 🧼 LIMPIEZA DE EMERGENCIA: 
        // Si por algún motivo entramos a la escena del Lobby y el NetworkManager se quedó 
        // encendido o colgado de una sesión anterior, lo obligamos a apagarse de inmediato.
        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("[Lobby] Red reseteada y limpiada con éxito al entrar al menú.");
        }
    }




    private void OnGUI()
    {
        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));

            if (GUILayout.Button("Crear Partida (Host)"))
            {
                NetworkManager.Singleton.StartHost();
            }

            // --- NUEVO: ESPACIO Y CAMPO DE TEXTO PARA LA IP ---
            GUILayout.Space(20); // Deja un espacio visual
            GUILayout.Label("Dirección IP del Servidor:");

            // Dibuja la casilla de texto en pantalla y actualiza la variable en tiempo real
            ipServidor = GUILayout.TextField(ipServidor, 30);
            // --------------------------------------------------

            if (GUILayout.Button("Unirse a Partida (Client)"))
            {
                // Antes de conectar, le inyectamos la IP escrita al componente de red
                ConfigurarIpTransporte(ipServidor);

                NetworkManager.Singleton.StartClient();
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

    /// <summary>
    /// Busca el componente de transporte de Unity Netcode y le cambia la IP de destino.
    /// </summary>
    private void ConfigurarIpTransporte(string nuevaIp)
    {
        // Buscamos el componente UnityTransport que está pegado en el NetworkManager de tu escena
        if (NetworkManager.Singleton.gameObject.TryGetComponent<UnityTransport>(out UnityTransport transporte))
        {
            // Le asignamos la IP que el usuario escribió en la casilla
            transporte.ConnectionData.Address = nuevaIp.Trim(); // .Trim() borra espacios en blanco accidentales
            Debug.Log($"[Lobby] Configurando IP de conexión a: {nuevaIp.Trim()}");
        }
        else
        {
            Debug.LogError("❌ [Lobby] No se encontró el componente UnityTransport en el NetworkManager.");
        }
    }
}