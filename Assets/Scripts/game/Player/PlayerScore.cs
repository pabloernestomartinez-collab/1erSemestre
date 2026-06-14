using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PlayerScore : NetworkBehaviour
{
    [Header("UI Textos")]
    // Los hacemos públicos para poder asignarlos de forma segura
    public TextMeshProUGUI textoEspadasUI;
    public TextMeshProUGUI textoEscudosUI;

    // 🔥 DOS VARIABLES DE RED SEPARADAS
    private readonly NetworkVariable<int> cantEspadas = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<int> cantEscudos = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public void ForzarActualizacionVisual()
    {
        ActualizarEspadasVisual(cantEspadas.Value);
        ActualizarEscudosVisual(cantEscudos.Value);
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Buscamos el texto de las espadas por su nombre exacto
            GameObject objEspadas = GameObject.Find("TextoEspadas");
            if (objEspadas != null) textoEspadasUI = objEspadas.GetComponent<TextMeshProUGUI>();

            // Buscamos el texto de los escudos por su nombre exacto
            GameObject objEscudos = GameObject.Find("TextoEscudos");
            if (objEscudos != null) textoEscudosUI = objEscudos.GetComponent<TextMeshProUGUI>();

            // Dibujamos los valores iniciales (0 y 0)
            ActualizarEspadasVisual(cantEspadas.Value);
            ActualizarEscudosVisual(cantEscudos.Value);

            // Nos suscribimos a los dos eventos por separado
            cantEspadas.OnValueChanged += (viejo, nuevo) => ActualizarEspadasVisual(nuevo);
            cantEscudos.OnValueChanged += (viejo, nuevo) => ActualizarEscudosVisual(nuevo);
        }
    }

    // 🔥 FUNCIÓN EXCLUSIVA PARA SUMAR ESPADAS (La llama el script espada)
    public void SumarEspada(int cantidad)
    {
        if (!IsServer) return;
        cantEspadas.Value += cantidad;
    }

    // 🔥 FUNCIÓN EXCLUSIVA PARA SUMAR ESCUDOS (La llama el script escudo)
    public void SumarEscudo(int cantidad)
    {
        if (!IsServer) return;
        cantEscudos.Value += cantidad;
    }

    private void ActualizarEspadasVisual(int valor)
    {
        if (textoEspadasUI != null) textoEspadasUI.text = "⚔️ Espadas: " + valor.ToString();
    }

    private void ActualizarEscudosVisual(int valor)
    {
        if (textoEscudosUI != null) textoEscudosUI.text = "🛡️ Escudos: " + valor.ToString();
    }
}