using Unity.Netcode;
using UnityEngine;
//using UnityEngine.SceneManagement;

public class perdi : NetworkBehaviour
{
    private bool yaMurio = false; // Evita que se envíen múltiples peticiones mientras cae

    void Update()
    {
        if (!IsOwner) return;
        if (yaMurio) return;
        if (transform.position.y < -10f)// Si la posición en el eje Y cae por debajo de -10 (caer al vacío)
        {
            yaMurio = true; // Bloqueamos el Update local
            RequestDeathServerRpc(); // LLAMADA EN RED: El cliente local le envía una petición al servidor
        }
    }

    [ServerRpc]
    void RequestDeathServerRpc()
    {
        ShowGameOverClientRpc();

        if (TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.isKinematic = true;
        transform.position = new Vector3(0, -999f, 0);

        
        //  un texto personalizado al UIManager para que no tire error.
        if (GameUIManager.Instance != null)
        {
            // OwnerClientId nos dice el ID del que se cayó. 
            // Si es 0 se cayó el Host, si es 1 se cayó el Cliente.
            string mensajeMuerte = OwnerClientId == 0 ? "💀 ¡El Host se cayó al vacío!" : "💀 ¡El Cliente se cayó al vacío!";//google...

            GameUIManager.Instance.MostrarBotonesFinPartidaRpc(mensajeMuerte);
        }
    }

    [ClientRpc]
    void ShowGameOverClientRpc()
    {
        if (IsOwner)// El cartel de "Game Over" solo debe verlo el dueño de este personaje específico
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