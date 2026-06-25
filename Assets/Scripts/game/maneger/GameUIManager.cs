using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameUIManager : NetworkBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("Marcadores Globales")]
    private TextMeshProUGUI textoHost;
    private TextMeshProUGUI textoCliente;
    private TextMeshProUGUI textoFinCliente;

    [Header("Armas Locales")]
    [SerializeField] private TextMeshProUGUI textoEspadas;
    [SerializeField] private TextMeshProUGUI textoEscudos;

    [Header("Coleccionables Locales")]
    [SerializeField] private TextMeshProUGUI texto222;
    [SerializeField] private TextMeshProUGUI texto224;

    private bool mostrarMenuFin = false;
    private string textoGanador = "";
    private bool regresandoAlLobby = false;

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
        regresandoAlLobby = false;
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
        // Esperamos a salir del lobby de forma segura
        while (SceneManager.GetActiveScene().name == "lobby")
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.2f);

        // Buscamos los marcadores tradicionales en la jerarquía
        GameObject objHost = GameObject.Find("TextoPuntajeHost");
        GameObject objCliente = GameObject.Find("TextoPuntajeCliente");

        if (objHost != null) textoHost = objHost.GetComponent<TextMeshProUGUI>();
        if (objCliente != null) textoCliente = objCliente.GetComponent<TextMeshProUGUI>();

        GameObject objFinCliente = GameObject.Find("TextoFinCliente");
        if (objFinCliente != null)
        {
            textoFinCliente = objFinCliente.GetComponent<TextMeshProUGUI>();
            textoFinCliente.gameObject.SetActive(false);
        }

        //  Buscamos ambos scripts en el objeto del jugador
        ArmasPlayer scriptArmas = null;
        coleccionables scriptColeccionables = null;

        while (scriptArmas == null || scriptColeccionables == null)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
            {
                var jugadorObj = NetworkManager.Singleton.LocalClient?.PlayerObject;

                if (jugadorObj != null)
                {
                    // Intentamos obtener el script de armas si aún no lo tenemos
                    if (scriptArmas == null) scriptArmas = jugadorObj.GetComponent<ArmasPlayer>();

                    // Intentamos obtener el script de coleccionables si aún no lo tenemos
                    if (scriptColeccionables == null) scriptColeccionables = jugadorObj.GetComponent<coleccionables>();
                }
            }
            yield return new WaitForSeconds(0.1f); // Esperamos un frame de red
        }

        //  Vinculamos la UI de Armas
        if (scriptArmas != null)
        {
            scriptArmas.textoEspadasUI = textoEspadas;
            scriptArmas.textoEscudosUI = textoEscudos;
            scriptArmas.ForzarActualizacionVisual();
        }

        //  Vinculamos la UI de Coleccionables en su respectivo script
        if (scriptColeccionables != null)
        {
            scriptColeccionables.texto222UI = texto222;
            scriptColeccionables.texto224UI = texto224;
            scriptColeccionables.ForzarActualizacionVisual(); 
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

    [Rpc(SendTo.Everyone)]
    public void MostrarBotonesFinPartidaRpc(string mensajeResultado)
    {
        textoGanador = mensajeResultado;
        mostrarMenuFin = true;

        if (!IsServer)
        {
            if (textoFinCliente != null)
            {
                textoFinCliente.gameObject.SetActive(true);
                textoFinCliente.text = "=== PARTIDO TERMINADO ===\n\n" + mensajeResultado;
            }

            if (!regresandoAlLobby)
            {
                regresandoAlLobby = true;
                StartCoroutine(EsperarYVolverAlLobby());
            }
        }
    }

    private IEnumerator EsperarYVolverAlLobby()
    {
        yield return new WaitForSeconds(5f);
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
        yield return null;
        SceneManager.LoadScene("lobby");
    }

    private void OnGUI()
    {
        if (!mostrarMenuFin) return;
        if (!IsServer) return;

        float xCentro = (Screen.width / 2) - 150;
        float yCentro = (Screen.height / 2) - 90;

        GUILayout.BeginArea(new Rect(xCentro, yCentro, 300, 150), GUI.skin.box);
        GUILayout.Label("=== PARTIDO TERMINADO ===", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
        GUILayout.Space(5);

        GUIStyle estiloGanador = new GUIStyle(GUI.skin.box);
        estiloGanador.alignment = TextAnchor.MiddleCenter;
        estiloGanador.normal.textColor = Color.yellow;
        GUILayout.Box(textoGanador, estiloGanador, GUILayout.Height(30));

        GUILayout.Space(10);

        if (GUILayout.Button("¿Jugar otra partida?")) StartCoroutine(ReiniciarPartidaHost());
        GUILayout.Space(5);
        if (GUILayout.Button("Volver a Windows")) StartCoroutine(CierreOrdenadoJuego());

        GUILayout.EndArea();
    }

    private IEnumerator ReiniciarPartidaHost()
    {
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
        yield return null;
        SceneManager.LoadScene("lobby");
    }

    private IEnumerator CierreOrdenadoJuego()
    {
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
        yield return null;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}