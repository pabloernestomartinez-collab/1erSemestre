using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement; // Para reiniciar la escena

public class perdi : NetworkBehaviour
{
    [SerializeField] private GameObject gameOverMenu; // Arrastra tu Panel aquí
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            gameOverMenu = GameObject.Find("Canvas").transform.Find("gameover").gameObject; // Busca gamerover (aunque esté desactivado)

        }
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
        // El servidor confirma la muerte y le avisa al cliente específico
        ShowGameOverClientRpc();

        gameObject.SetActive(false);// El servidor quita al jugador de la partida físicamente

    }

    [ClientRpc]
    void ShowGameOverClientRpc()
    {
        if (IsOwner)
        {
            //// Bloqueamos el cursor para poder hacer clic en el botón
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;

            // Activamos el menú
            if (gameOverMenu != null)
                gameOverMenu.SetActive(true);

            //// Desactivamos el script de movimiento para que no siga "cayendo"
            //if (TryGetComponent(out PlayerMovement moveScript))
            //    moveScript.enabled = false;
        }
    }

    // Esta función la llamará el botón de la UI
    public void ReiniciarPartida()
    {
        // En Netcode, lo más sano para reiniciar es desconectar y volver al menú principal
        // o que el Server cambie de escena usando NetworkSceneManager.
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}