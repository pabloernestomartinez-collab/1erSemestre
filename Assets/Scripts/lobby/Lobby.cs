using Unity.Netcode; // Necesario para acceder a las funciones multijugador de Netcode (Host, Client, NetworkManager)
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario si fueras a usar el cambio de escenas tradicional de Unity 

public class Lobby : MonoBehaviour
{
    private void OnGUI() // se ejecuta varias veces por frame para dibujar interfaces rápidas

    {
        
        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)// Si no soy Servidor Y tampoco soy Cliente, significa que el juego acaba de arrancar y nadie está conectado.
        {
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));// Creamos un área rectangular en la pantalla de 300x300 píxeles, ubicada en la esquina superior izquierda (X:10, Y:10)

            
            if (GUILayout.Button("Crear Partida (Host)"))// Dibujamos un botón. Si el jugador hace clic en él, se ejecuta el código de adentro
            {
                
                NetworkManager.Singleton.StartHost();// StartHost() convierte esta computadora en el SERVIDOR y en un JUGADOR al mismo tiempo (Pantalla principal)
            }

            
            if (GUILayout.Button("Unirse a Partida (Client)"))// Dibujamos un segundo botón para los invitados
            {
                
                NetworkManager.Singleton.StartClient();// StartClient() busca una partida existente en la red local/IP y se conecta como un jugador invitado
            }

            
            GUILayout.EndArea();// Cerramos el área de dibujo de botones para no alterar el resto de la interfaz
        }
        
        
        else// Si el código entra aquí (else), significa que ya le dimos a Host o a Client y ya estamos dentro de la red.
        {
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));// Volvemos a delimitar el área de dibujo en el mismo espacio (10, 10)

            
            int jugadoresConectados = NetworkManager.Singleton.ConnectedClients.Count;// Preguntamos al NetworkManager cuántas computadoras están conectadas actualmente al servidor

            
            GUILayout.Label($"Jugadores en el lobby: {jugadoresConectados} / 2");// Mostramos un texto en pantalla que se actualiza en tiempo real: "Jugadores en el lobby: X / 2"

            
            
            if (NetworkManager.Singleton.IsServer)// En Netcode, solo el Servidor/Host tiene permitido decidir cuándo inicia la partida o cambiar de mapa
            {
                
                GUILayout.Label("Esperando a que el cliente se conecte...");// Le mostramos un mensaje de estado al Host

                
                if (jugadoresConectados >= 2)// REGLA DE INICIO: Solo dejamos empezar si hay 2 o más personas en la sala
                {
                    
                    if (GUILayout.Button("¡EMPEZAR JUEGO!"))// Si ya entró el Cliente, al Host le aparece el botón definitivo para jugar
                    {
                        
                        
                        NetworkManager.Singleton.SceneManager.LoadScene("game", LoadSceneMode.Single);// Le ordena al Servidor cargar la escena "game" y, al mismo tiempo, obliga a TODOS los clientes conectados a cargar esa misma escena de forma perfectamente sincronizada.
                    }
                }
                else
                {
                    
                    GUILayout.Box("Esperando más jugadores para poder iniciar...");// if el Host está solo se bloquea el botón
                }
            }
            
            else// if NO es el servidor, significa que somos el CLIENTE (Jugador 2)
            {
                
                GUILayout.Label("¡Conectado! Esperando que el Host inicie la partida..."); // le damos un texto para que espere al Host
            }

            
            GUILayout.EndArea();// Cerramos el área de dibujo
        }
    }
}