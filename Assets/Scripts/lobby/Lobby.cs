using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    private void OnGUI()
    {
        // Si el NetworkManager aún no está corriendo (nadie ha pulsado Host ni Client)
        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));

            if (GUILayout.Button("Crear Partida (Host)"))
            {
                NetworkManager.Singleton.StartHost();
            }

            if (GUILayout.Button("Unirse a Partida (Client)"))
            {
                NetworkManager.Singleton.StartClient();
            }

            GUILayout.EndArea();
        }
        else
        {
            // Si ya estamos conectados en el menú de espera
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));

            int jugadoresConectados = NetworkManager.Singleton.ConnectedClients.Count;
            GUILayout.Label($"Jugadores en el lobby: {jugadoresConectados} / 2");

            // SOLO el Host (Server) tiene permitido cambiar de escena a todos los jugadores
            if (NetworkManager.Singleton.IsServer)
            {
                GUILayout.Label("Esperando a que el cliente se conecte...");

                // Opcional: Puedes bloquear el botón hasta que seamos 2 jugadores
                if (jugadoresConectados >= 2)
                {
                    if (GUILayout.Button("¡EMPEZAR JUEGO!"))
                    {
                        // Usamos el SceneManager de Netcode para que cambie la escena a TODOS a la vez
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
}
