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

        // Arrancamos la rutina para buscar los textos en la escena actual
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
        // Esperamos a que salgamos del Lobby y la escena real cargue
        while (SceneManager.GetActiveScene().name == "lobby")
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.2f);

        // 🔥 CLAVE 1: SIN candado "if (IsServer)". 
        // Tanto el Host como el Cliente ejecutan estas líneas para buscar los textos en sus propias pantallas locales.
        GameObject objHost = GameObject.Find("TextoPuntajeHost");
        GameObject objCliente = GameObject.Find("TextoPuntajeCliente");

        if (objHost != null) textoHost = objHost.GetComponent<TextMeshProUGUI>();
        if (objCliente != null) textoCliente = objCliente.GetComponent<TextMeshProUGUI>();

        // Solo el servidor fuerza la inicialización de los marcadores en 0 al arrancar
        if (IsServer)
        {
            ActualizarMarcador(0, 0);
            ActualizarMarcador(1, 0);
        }
    }

    /// <summary>
    /// Esta función la llama tu script de puntos (el que detecta cuando entregan una moneda).
    /// </summary>
    public void ActualizarMarcador(ulong jugadorId, int nuevosPuntos)
    {
        // Solo el Servidor tiene permitido procesar el cambio de puntos
        if (!IsServer) return;

        // 🔥 CLAVE 2: El servidor le ordena a TODO EL MUNDO (Rpc) que actualice sus textos visuales
        ActualizarMarcadorEnClientesRpc(jugadorId, nuevosPuntos);
    }

    // 🔥 ESTE RPC VIAJA POR RED Y SE EJECUTA EN TODAS LAS COMPUTADORAS
    [Rpc(SendTo.Everyone)]
    private void ActualizarMarcadorEnClientesRpc(ulong jugadorId, int nuevosPuntos)
    {
        // Aquí adentro, tanto el Host como el Cliente modifican sus componentes visuales de TextMeshPro
        if (jugadorId == 0)
        {
            if (textoHost != null) textoHost.text = "Host Puntos: " + nuevosPuntos;
        }
        else
        {
            if (textoCliente != null) textoCliente.text = "Cliente Puntos: " + nuevosPuntos;
        }
    }

    [Rpc(SendTo.Everyone)]
    public void MostrarBotonesFinPartidaRpc(string mensajeResultado)
    {
        textoGanador = mensajeResultado;
        mostrarMenuFin = true;
    }

    private void OnGUI()
    {
        if (!mostrarMenuFin) return;
        if (!IsServer) return; // Recuerda que el Cliente no ve este recuadro gris

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

        if (GUILayout.Button("Volver a Winows"))
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

        yield return null; // Espera un frame para evitar errores de conexión colgada

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}