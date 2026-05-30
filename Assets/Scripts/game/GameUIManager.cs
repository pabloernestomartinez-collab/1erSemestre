using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;

public class GameUIManager : NetworkBehaviour
{
    // Instancia pública para poder llamarlo desde cualquier script fácilmente
    public static GameUIManager Instance { get; private set; }

    private TextMeshProUGUI textoHost;
    private TextMeshProUGUI textoCliente;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        // Iniciamos la corrutina para esperar de forma segura que la escena cargue la UI
        StartCoroutine(EsperarYVincularUI());
    }

    private IEnumerator EsperarYVincularUI()
    {
        // Esperamos a salir del menú de Lobby si es necesario
        while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "lobby")
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.2f);

        // Buscamos los dos textos en el Canvas por su nombre exacto
        GameObject objHost = GameObject.Find("TextoPuntajeHost");
        GameObject objCliente = GameObject.Find("TextoPuntajeCliente");

        if (objHost != null) textoHost = objHost.GetComponent<TextMeshProUGUI>();
        if (objCliente != null) textoCliente = objCliente.GetComponent<TextMeshProUGUI>();

        // Dibujo inicial
        ActualizarMarcador(0, 0);
        ActualizarMarcador(1, 0);
    }

    /// <summary>
    /// Método público para actualizar el texto de cualquier jugador en todas las pantallas.
    /// </summary>
    public void ActualizarMarcador(ulong jugadorId, int nuevosPuntos)
    {
        // Si el ID es 0, asumimos que es el Host/Servidor

        if (jugadorId == 0)
        {
            if (textoHost != null) textoHost.text = "Host Puntos: " + nuevosPuntos;
            Debug.Log(jugadorId);
        }
        // Si el ID es mayor a 0, es un Cliente (Jugador 1, 2, etc.)
        else
        {
            if (textoCliente != null) textoCliente.text = "Cliente Puntos: " + nuevosPuntos;
            //Debug.Log(jugadorId);
        }
    }
}