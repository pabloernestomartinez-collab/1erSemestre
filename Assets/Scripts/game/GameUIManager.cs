using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement; // Necesario para reiniciar la escena

public class GameUIManager : NetworkBehaviour
{
    public static GameUIManager Instance { get; private set; }
    private TextMeshProUGUI textoHost;
    private TextMeshProUGUI textoCliente;
    private bool mostrarMenuFin = false;    // Controla si se deben dibujar los botones de Fin de Juego


    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        mostrarMenuFin = false;
        StartCoroutine(EsperarYVincularUI());
    }

    private IEnumerator EsperarYVincularUI()
    {
        while (SceneManager.GetActiveScene().name == "Lobby")
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.2f);

        GameObject objHost = GameObject.Find("TextoPuntajeHost");
        GameObject objCliente = GameObject.Find("TextoPuntajeCliente");

        if (objHost != null) textoHost = objHost.GetComponent<TextMeshProUGUI>();
        if (objCliente != null) textoCliente = objCliente.GetComponent<TextMeshProUGUI>();

        ActualizarMarcador(0, 0);
        ActualizarMarcador(1, 0);
    }

    public void ActualizarMarcador(ulong jugadorId, int nuevosPuntos)
    {
        if (jugadorId == 0)
        {
            if (textoHost != null) textoHost.text = "Host Puntos: " + nuevosPuntos;
        }
        else
        {
            if (textoCliente != null) textoCliente.text = "Cliente Puntos: " + nuevosPuntos;
        }
    }

    // ====================================================================
    //                MENÚ DE FIN DE JUEGO EN RED
    // ====================================================================

    [Rpc(SendTo.Server)] // El Servidor llama a esto y se ejecuta en Host 
    public void MostrarBotonesFinPartidaRpc()
    {
        mostrarMenuFin = true;
    }

    private void OnGUI()
    {
        if (!mostrarMenuFin) return;
        if (!IsServer) return; // Solo el Host lo ve y lo opera

        float xCentro = (Screen.width / 2) - 150;
        float yCentro = (Screen.height / 2) - 75;

        GUILayout.BeginArea(new Rect(xCentro, yCentro, 300, 150), GUI.skin.box);

        GUILayout.Label("=== ¡TIEMPO AGOTADO! ===", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
        GUILayout.Space(10);

        if (GUILayout.Button("¿Jugar otra partida?"))
        {
                 // El Host recarga la escena de juego. Esto vacía automáticamente las puntuaciones y mantiene al cliente pegado en la partida sin tener que reconectarse de cero.
            NetworkManager.Singleton.SceneManager.LoadScene("lobby", LoadSceneMode.Single);
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Salir del juego"))// APAGADO TOTAL
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("Lobby");
        }

        GUILayout.EndArea();
    }
}