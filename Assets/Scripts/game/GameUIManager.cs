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

    //  Guardará el texto del resultado para mostrarlo en el cartel
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

        if (!IsServer)
        {
            if (TryGetComponent<Canvas>(out Canvas miCanvas))
            {
                miCanvas.enabled = false;
            }
            else if (GetComponentInChildren<Canvas>() != null)
            {
                GetComponentInChildren<Canvas>().enabled = false;
            }
        }

        StartCoroutine(EsperarYVincularUI());
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this) Instance = null;
    }

    public override void OnDestroy()
    {
        //  Le decimos a Netcode que haga su limpieza interna primero
        base.OnDestroy();

        //  limpieza del Singleton
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private IEnumerator EsperarYVincularUI()
    {
        while (SceneManager.GetActiveScene().name == "lobby")
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.2f);

        if (IsServer)
        {
            GameObject objHost = GameObject.Find("TextoPuntajeHost");
            GameObject objCliente = GameObject.Find("TextoPuntajeCliente");

            if (objHost != null) textoHost = objHost.GetComponent<TextMeshProUGUI>();
            if (objCliente != null) textoCliente = objCliente.GetComponent<TextMeshProUGUI>();

            ActualizarMarcador(0, 0);
            ActualizarMarcador(1, 0);
        }
    }

    public void ActualizarMarcador(ulong jugadorId, int nuevosPuntos)
    {
        if (!IsServer) return;

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
    //  Ahora el Rpc recibe el texto de quién fue el ganador
    // ====================================================================
    [Rpc(SendTo.Everyone)]
    public void MostrarBotonesFinPartidaRpc(string mensajeResultado)
    {
        textoGanador = mensajeResultado;
        mostrarMenuFin = true;
    }

    private void OnGUI()
    {
        if (!mostrarMenuFin) return;
        if (!IsServer) return; //  el Cliente no ve este recuadro

        float xCentro = (Screen.width / 2) - 150;
        float yCentro = (Screen.height / 2) - 90; // Aumentamos un poquito el alto del menú

        GUILayout.BeginArea(new Rect(xCentro, yCentro, 300, 180), GUI.skin.box);

        GUILayout.Label("=== ¡TIEMPO AGOTADO! ===", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
        GUILayout.Space(5);

        //   EL CARTEL DEL GANADOR EN LA PANTALLA::::: google
        GUIStyle estiloGanador = new GUIStyle(GUI.skin.box);
        estiloGanador.alignment = TextAnchor.MiddleCenter;
        estiloGanador.normal.textColor = Color.yellow; // Resalta el texto en amarillo
        GUILayout.Box(textoGanador, estiloGanador, GUILayout.Height(30));

        GUILayout.Space(10);

        if (GUILayout.Button("¿Jugar otra partida?"))
        {
            NetworkManager.Singleton.SceneManager.LoadScene("lobby", LoadSceneMode.Single);
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Salir del juego"))
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("lobby");
        }

        GUILayout.EndArea();
    }
}