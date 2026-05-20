using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class perdi : NetworkBehaviour
{
    // Ya no usamos la variable gameOverMenu aquí adentro de forma directa buscando en la escena

    public override void OnNetworkSpawn()
    {
        // Dejamos esto vacío. ¡Ya no buscamos el Canvas al spawnear!
    }

    void Update()
    {
        if (!IsOwner) return;

        if (transform.position.y < -10f)
        {
            // Avisamos al servidor que morimos
            RequestDeathServerRpc();
        }
    }

    [ServerRpc]
    void RequestDeathServerRpc()
    {
        ShowGameOverClientRpc();

        // RECOMENDACIÓN MULTIJUGADOR: En lugar de SetActive(false), que rompe la sincronización del NetworkObject,
        // movemos al jugador a una zona muerta o desactivamos sus componentes visuales/físicos.
        if (TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.isKinematic = true;
        transform.position = new Vector3(0, -999f, 0); // Lo mandamos muy abajo del mapa
    }

    [ClientRpc]
    void ShowGameOverClientRpc()
    {
        if (IsOwner)
        {
            // BUSCAMOS EL MENÚ JUSTO EN EL MOMENTO DE MORIR (Cuando la escena ya cargó seguro)
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                Transform goMenu = canvas.transform.Find("gameover");
                if (goMenu != null)
                {
                    goMenu.gameObject.SetActive(true);
                }
            }

            // Desactivamos el script de movimiento para frenar los inputs
            if (TryGetComponent(out PlayerMovement moveScript))
                moveScript.enabled = false;
        }
    }

    public void ReiniciarPartida()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(0); // Te manda de vuelta de forma segura a la escena del Menú/Lobby (Index 0)
    }
}