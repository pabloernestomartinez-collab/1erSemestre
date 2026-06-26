using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameGameStateManager : NetworkBehaviour
{
    public static GameGameStateManager Instance { get; private set; }

    [Header("Marcadores de Fin de Partida")]
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
        regresandoAlLobby = false;
        StartCoroutine(EsperarYVincularUIFin());
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

    private IEnumerator EsperarYVincularUIFin()
    {
        while (SceneManager.GetActiveScene().name == "lobby")
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.2f);

        GameObject objFinCliente = GameObject.Find("TextoFinCliente");
        if (objFinCliente != null)
        {
            textoFinCliente = objFinCliente.GetComponent<TextMeshProUGUI>();
            textoFinCliente.gameObject.SetActive(false);
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
