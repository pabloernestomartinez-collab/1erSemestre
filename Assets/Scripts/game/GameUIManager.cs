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
    private TextMeshProUGUI textoFinCliente;
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
        regresandoAlLobby = false; // Reiniciamos el candado al empezar
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


    [Rpc(SendTo.Everyone)]
    public void MostrarBotonesFinPartidaRpc(string mensajeResultado)
    {
        textoGanador = mensajeResultado;
        mostrarMenuFin = true; // Activa el OnGUI del Host

        if (!IsServer)
        {
            if (textoFinCliente != null)
            {
                textoFinCliente.gameObject.SetActive(true);
                // Le ponemos el mensaje que calculó el servidor
                textoFinCliente.text = "=== PARTIDO TERMINADO ===\n\n" + mensajeResultado;
            }

            // if somos el cliente, iniciamos la cuenta regresiva automática de 5 segundos
            if (!regresandoAlLobby)
            {
                regresandoAlLobby = true;
                StartCoroutine(EsperarYVolverAlLobby());
            }
        }
    }

    private IEnumerator EsperarYVolverAlLobby()
    {
        // Esperamos los 5 segundos con el texto de ganador en pantalla
        yield return new WaitForSeconds(5f);

        Debug.Log("[Cliente] 5 segundos cumplidos. Regresando ordenadamente al menú...");

        // Desconectamos la red de Netcode de forma prolija para este cliente
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        // Esperamos un frame para que los sockets se liberen correctamente
        yield return null;

        // Regresamos a la escena del menú usando el nombre correcto en minúsculas
        SceneManager.LoadScene("lobby");
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
            // Corregido también con el sistema de apagado limpio que diseñamos antes
            StartCoroutine(ReiniciarPartidaHost());
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Volver a Windows"))
        {
            StartCoroutine(CierreOrdenadoJuego());
        }

        GUILayout.EndArea();
    }

    private IEnumerator ReiniciarPartidaHost()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        yield return null;
        SceneManager.LoadScene("lobby");
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