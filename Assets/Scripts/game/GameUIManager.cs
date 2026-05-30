using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameUIManager : NetworkBehaviour
{
    public static GameUIManager Instance { get; private set; }

    private TextMeshProUGUI textoHost;
    private TextMeshProUGUI textoCliente;

    // 🔥 NUEVA VARIABLE: Para vincular un texto exclusivo que verá el cliente al final
    private TextMeshProUGUI textoFinCliente;

    private bool mostrarMenuFin = false;
    private string textoGanador = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        mostrarMenuFin = false;
        textoGanador = "";

        StartCoroutine(EsperarYVincularUI());
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this) Instance = null;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (Instance == this) Instance = null;
    }

    private IEnumerator EsperarYVincularUI()
    {
        while (SceneManager.GetActiveScene().name == "lobby")
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.2f);

        // Buscamos los marcadores tradicionales
        GameObject objHost = GameObject.Find("TextoPuntajeHost");
        GameObject objCliente = GameObject.Find("TextoPuntajeCliente");

        if (objHost != null) textoHost = objHost.GetComponent<TextMeshProUGUI>();
        if (objCliente != null) textoCliente = objCliente.GetComponent<TextMeshProUGUI>();

        // 🔥 NUEVO: Buscamos el objeto de texto especial para el fin del cliente
        GameObject objFinCliente = GameObject.Find("TextoFinCliente");
        if (objFinCliente != null)
        {
            textoFinCliente = objFinCliente.GetComponent<TextMeshProUGUI>();
            textoFinCliente.gameObject.SetActive(false); // Lo aseguramos apagado al inicio
        }

        if (IsServer)
        {
            ActualizarMarcador(0, 0);
            ActualizarMarcador(1, 0);
        }
    }

    public void ActualizarMarcador(ulong jugadorId, int nuevosPuntos)
    {
        if (!IsServer) return;
        ActualizarMarcadorEnClientesRpc(jugadorId, nuevosPuntos);
    }

    [Rpc(SendTo.Everyone)]
    private void ActualizarMarcadorEnClientesRpc(ulong jugadorId, int nuevosPuntos)
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
    // 🏆 MODIFICADO: Este método lo reciben AMBOS al terminar la partida
    // ====================================================================
    [Rpc(SendTo.Everyone)]
    public void MostrarBotonesFinPartidaRpc(string mensajeResultado)
    {
        textoGanador = mensajeResultado;
        mostrarMenuFin = true; // Activa el OnGUI del Host

        // 🔥 SI SOMOS EL CLIENTE: Mostramos el cartel gigante en su pantalla
        if (!IsServer)
        {
            if (textoFinCliente != null)
            {
                textoFinCliente.gameObject.SetActive(true);
                // Le ponemos el mensaje que calculó el servidor (Ej: "🏆 ¡GANÓ EL HOST! (5 vs 3)")
                textoFinCliente.text = "=== PARTIDO TERMINADO ===\n\n" + mensajeResultado;
            }
        }
    }

    private void OnGUI()
    {
        if (!mostrarMenuFin) return;
        if (!IsServer) return; // El cliente NO entra aquí, su pantalla queda limpia de botones

        // --- El Host dibuja sus botones de control ---
        float xCentro = (Screen.width / 2) - 150;
        float yCentro = (Screen.height / 2) - 90;

        GUILayout.BeginArea(new Rect(xCentro, yCentro, 300, 180), GUI.skin.box);

        GUILayout.Label("=== ¡TIEMPO AGOTADO! ===", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
        GUILayout.Space(5);

        GUIStyle estiloGanador = new GUIStyle(GUI.skin.box);
        estiloGanador.alignment = TextAnchor.MiddleCenter;
        estiloGanador.normal.textColor = Color.yellow;
        GUILayout.Box(textoGanador, estiloGanador, GUILayout.Height(30));

        GUILayout.Space(10);

        if (GUILayout.Button("¿Jugar otra partida?"))
        {
            NetworkManager.Singleton.SceneManager.LoadScene("lobby", LoadSceneMode.Single);
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Volver a Windows"))
        {
            StartCoroutine(CierreOrdenadoJuego());
        }

        GUILayout.EndArea();
    }

    private IEnumerator CierreOrdenadoJuego()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        yield return null;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}