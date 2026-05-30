using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class perdi : NetworkBehaviour
{
    private bool yaMurio = false; // Evita que se envíen múltiples peticiones mientras cae

    void Update()
    {
        if (!IsOwner) return;
        if (yaMurio) return;

        // Si la posición en el eje Y cae por debajo de -10 (caer al vacío)
        if (transform.position.y < -10f)
        {
            yaMurio = true; // Bloqueamos el Update local
            RequestDeathServerRpc(); // LLAMADA EN RED: El cliente local le envía una petición al servidor
        }
    }

    [ServerRpc]
    void RequestDeathServerRpc()
    {
        // 1. El servidor ordena a los clientes que ejecuten la UI individual de Game Over
        ShowGameOverClientRpc();

        // 2. Apagamos las físicas del jugador en el servidor para que no siga cayendo
        if (TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.isKinematic = true;

        // Escondemos el objeto en una coordenada muerta
        transform.position = new Vector3(0, -999f, 0);

        // 🔥 NUEVO - LLAMADA AL MENÚ DE FIN DE JUEGO EN RED:
        // El Servidor le ordena al UIManager global que despliegue los botones (Reiniciar/Salir)
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.MostrarBotonesFinPartidaRpc();
        }
    }

    [ClientRpc]
    void ShowGameOverClientRpc()
    {
        // El cartel de "Game Over" solo debe verlo el dueño de este personaje específico
        if (IsOwner)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                Transform goMenu = canvas.transform.Find("gameover"); // Busca al hijo llamado "gameover"
                if (goMenu != null)
                {
                    goMenu.gameObject.SetActive(true); // Muestra el menú visual local
                }
            }

            // Congela los inputs del teclado/mando
            if (TryGetComponent(out PlayerMovement moveScript))
            {
                moveScript.enabled = false;
            }
        }
    }
}