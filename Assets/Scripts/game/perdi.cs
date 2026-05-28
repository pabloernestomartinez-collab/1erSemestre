using Unity.Netcode; 
using UnityEngine;
using UnityEngine.SceneManagement; 

public class perdi : NetworkBehaviour
{
    void Update()
    {
        if (!IsOwner) return;

        if (transform.position.y < -10f)        // Si la posición en el eje Y cae por debajo de -10 (caer al vacío)

        {
            RequestDeathServerRpc();     // LLAMADA EN RED: El cliente local le envía una petición urgente al servidor para procesar su muerte.

        }
    }

    [ServerRpc]    // help: SERVER RPC: Esta función se sintoniza desde el cliente pero se ejecuta SÓLO con la autoridad del Servidor.

    void RequestDeathServerRpc()
    {
        ShowGameOverClientRpc();        // El servidor ordena a todos los clientes que ejecuten la UI


        if (TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.isKinematic = true;        // Apagamos las físicas del jugador para que no siga cayendo


        transform.position = new Vector3(0, -999f, 0);        // consejo de google: el servidor lo esconde en una coordenada "muerta" invisible para los demás jugadores.

    }

    [ClientRpc]
    void ShowGameOverClientRpc()
    {
        if (IsOwner)    // Evita que al Jugador 1 le aparezca el cartel de derrota cuando el que se cayó fue el Jugador 2

        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                Transform goMenu = canvas.transform.Find("gameover");   // Busca al hijo llamado "gameover" (incluso si estaba desactivado de antemano)

                if (goMenu != null)
                {
                    goMenu.gameObject.SetActive(true);  // Muestra el menú visual en la pantalla del jugador afectado

                }
            }

            if (TryGetComponent(out PlayerMovement moveScript))  // consejo de google: Esto congela los inputs del teclado/mando para que el jugador no pueda seguir moviéndose tras morir

                moveScript.enabled = false;
        }
    }

    //public void ReiniciarPartida()
    //{
    //    NetworkManager.Singleton.Shutdown();

    //    SceneManager.LoadScene(0);
    //}
}