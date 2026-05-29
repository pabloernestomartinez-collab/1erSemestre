using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections; // Necesario para usar Corrutinas

public class PlayerScore : NetworkBehaviour
{
    // Variable de red: Sincroniza los puntos automáticamente
    public readonly NetworkVariable<int> puntos = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private TextMeshProUGUI textoUI;

    public override void OnNetworkSpawn()
    {
        puntos.OnValueChanged += AlCambiarPuntos;

        if (IsOwner)
        {
            // 🔥 LA MEJORA: En lugar de buscar el texto de inmediato, 
            // iniciamos una corrutina que buscará el objeto de forma segura frame a frame.
            StartCoroutine(EsperarYVincularUI());
        }
    }

    public override void OnNetworkDespawn()
    {
        puntos.OnValueChanged -= AlCambiarPuntos;
    }

    public void AddPoints(int cantidad)
    {
        if (!IsServer) return;
        puntos.Value += cantidad;
    }

    private void AlCambiarPuntos(int valorViejo, int valorNuevo)
    {
        if (IsOwner)
        {
            ActualizarTextoEnPantalla(valorNuevo);
        }
    }

    // El "motor de búsqueda inteligente" que espera a la escena
    private IEnumerator EsperarYVincularUI()
    {
        // Esperamos un frame para dejar que la escena se acomode
        yield return null;

        int intentos = 0;
        GameObject objetoTexto = null;

        // Intentará buscar el objeto durante 5 segundos (por si la PC es lenta cargando la escena)
        while (objetoTexto == null && intentos < 100)
        {
            // Buscamos por el nombre exacto que le diste en tu jerarquía
            objetoTexto = GameObject.Find("TextoPuntajeObjeto");

            if (objetoTexto == null)
            {
                intentos++;
                yield return new WaitForSeconds(0.05f); // Espera un instante antes de reintentar
            }
        }

        if (objetoTexto != null)
        {
            textoUI = objetoTexto.GetComponent<TextMeshProUGUI>();
            Debug.Log("[PlayerScore] ¡Texto de puntaje vinculado exitosamente tras cargar la escena!");

            // Forzamos el primer dibujo con los puntos actuales
            ActualizarTextoEnPantalla(puntos.Value);
        }
        else
        {
            Debug.LogError("❌ [PlayerScore] Definitivamente no se encontró 'TextoPuntajeObjeto'. Revisa que el nombre en la jerarquía de la escena de juego sea idéntico.");
        }
    }

    private void ActualizarTextoEnPantalla(int puntosActuales)
    {
        if (textoUI != null)
        {
            textoUI.text = "Puntos: " + puntosActuales;
        }
    }
}