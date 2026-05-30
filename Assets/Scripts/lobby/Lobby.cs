using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;// es para acceder al transporte de Netcode para cambiar la IP

public class Lobby : MonoBehaviour
{
    private string ipServidor = "127.0.0.1";// Variable para almacenar la IP

    private void Start()
    {

        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
        {
            NetworkManager.Singleton.Shutdown();
            //Debug.Log("[Lobby] Red reseteada y limpiada con éxito al entrar al menú.");
        }
    }
    private void OnGUI()
    {
        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            GUILayout.Label("Alumno: Pablo Martinez");
            GUILayout.Space(20); // Deja un espacio visual
            if (GUILayout.Button("Crear Partida (Host)"))
            {
                NetworkManager.Singleton.StartHost();
            }
            
            GUILayout.Space(20); // Deja un espacio visual
            GUILayout.Label("Dirección IP del Servidor:");
            ipServidor = GUILayout.TextField(ipServidor, 30);// Dibuja la casilla de texto en pantalla y actualiza la variable en tiempo real

            if (GUILayout.Button("Unirse a Partida (Client)"))
            {
                ConfigurarIpTransporte(ipServidor);// Antes de conectar, le inyectamos la IP escrita al componente de red
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

    private void ConfigurarIpTransporte(string nuevaIp)
    {
        if (NetworkManager.Singleton.gameObject.TryGetComponent<UnityTransport>(out UnityTransport transporte))        // Buscamos el componente UnityTransport que está pegado en el NetworkManager de tu escena
        {
            transporte.ConnectionData.Address = nuevaIp.Trim(); // .Trim() borra espacios en blanco accidentales y Le asignamos la IP que el usuario escribió en la casilla
        }
        else
        {
            //Debug.LogError("No se encontró el componente UnityTransport en el NetworkManager.");
        }
    }
}