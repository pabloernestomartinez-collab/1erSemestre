using Unity.Netcode; // Requerido para heredar de NetworkBehaviour y usar ServerRpc/ClientRpc
using UnityEngine;
using UnityEngine.SceneManagement; // Requerido para gestionar el reinicio y carga de escenas locales

public class perdi : NetworkBehaviour
{
    void Update()
    {
        // FILTRO DE RED: Solo la computadora que es dueña de este personaje ejecutará la detección de caída.
        // Evita que el Host o los rivales calculen si tu personaje se cayó.
        if (!IsOwner) return;

        // Si la posición en el eje Y cae por debajo de -10 (caer al vacío)
        if (transform.position.y < -10f)
        {
            // LLAMADA EN RED: El cliente local le envía una petición urgente al servidor para procesar su muerte.
            RequestDeathServerRpc();
        }
    }

    // SERVER RPC: Esta función se sintoniza desde el cliente pero se ejecuta SÓLO con la autoridad del Servidor.
    [ServerRpc]
    void RequestDeathServerRpc()
    {
        // El servidor ordena a todos los clientes que ejecuten la UI (especialmente al dueño)
        ShowGameOverClientRpc();

        // CONTROL DE FÍSICAS EN EL SERVIDOR:
        // Apagamos las físicas del jugador para que no siga cayendo infinitamente en el vacío del servidor.
        if (TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.isKinematic = true;

        // TELETRANSPORTE DE SEGURIDAD: En lugar de destruir el objeto o usar SetActive(false) (lo que rompería el Netcode),
        // el servidor lo esconde en una coordenada "muerta" invisible para los demás jugadores.
        transform.position = new Vector3(0, -999f, 0);
    }

    // CLIENT RPC: Esta función la ordena el servidor, pero se ejecuta en las pantallas de TODOS los clientes conectados.
    [ClientRpc]
    void ShowGameOverClientRpc()
    {
        // FILTRO DE RED LOCAL: Solo nos interesa activar la pantalla de "Game Over" al jugador que realmente perdió.
        // Evita que al Jugador 1 le aparezca el cartel de derrota cuando el que se cayó fue el Jugador 2.
        if (IsOwner)
        {
            // BÚSQUEDA TARDÍA Y SEGURA: Buscamos el Canvas en este preciso instante. 
            // Como el juego ya está corriendo hace tiempo, el Canvas existirá al 100% y no dará NullReferenceException.
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                // Busca al hijo llamado "gameover" (incluso si estaba desactivado de antemano)
                Transform goMenu = canvas.transform.Find("gameover");
                if (goMenu != null)
                {
                    // Muestra el menú visual en la pantalla del jugador afectado
                    goMenu.gameObject.SetActive(true);
                }
            }

            // DESACTIVACIÓN DE CONTROL: Apaga el script de movimiento local. 
            // Esto congela los inputs del teclado/mando para que el jugador no pueda seguir moviéndose tras morir.
            if (TryGetComponent(out PlayerMovement moveScript))
                moveScript.enabled = false;
        }
    }

    // FUNCIÓN DE BOTÓN: Este método está diseñado para ser arrastrado al evento OnClick() de tu botón de reinicio en la UI.
    public void ReiniciarPartida()
    {
        // APAGADO DE RED: Desconecta limpiamente la instancia actual de red (ya seas Host o Cliente).
        // Esto evita errores de sincronización acumulados al intentar recargar escenas a la fuerza.
        NetworkManager.Singleton.Shutdown();

        // Retorna a los jugadores de manera segura a la escena del menú principal o Lobby (Index 0 en Build Settings)
        SceneManager.LoadScene(0);
    }
}