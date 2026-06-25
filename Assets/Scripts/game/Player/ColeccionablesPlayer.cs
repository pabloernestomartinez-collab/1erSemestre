using Unity.Netcode;
using UnityEngine;
using TMPro;

public class ColeccionablesPlayer : NetworkBehaviour
{
    [Header("UI Textos")]
    // Los hacemos públicos para poder asignarlos de forma segura
    public TextMeshProUGUI texto222UI;
    public TextMeshProUGUI texto224UI;
    public TextMeshProUGUI texto226UI;
    public TextMeshProUGUI texto228UI;
    public TextMeshProUGUI texto230UI;
    public TextMeshProUGUI texto232UI;
    public TextMeshProUGUI texto234UI;


    // VARIABLES DE RED SEPARADAS
    private readonly NetworkVariable<int> cant222 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<int> cant224 = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public void ForzarActualizacionVisual()
    {
        Actualizar222Visual(cant222.Value);
        Actualizar224Visual(cant224.Value);
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Buscamos el texto de las espadas por su nombre exacto
            GameObject objEspadas = GameObject.Find("Texto222");
            if (objEspadas != null) texto222UI = objEspadas.GetComponent<TextMeshProUGUI>();

            // Buscamos el texto de los escudos por su nombre exacto
            GameObject objEscudos = GameObject.Find("Texto224");
            if (objEscudos != null) texto224UI = objEscudos.GetComponent<TextMeshProUGUI>();

            // Dibujamos los valores iniciales (0 y 0)
            Actualizar222Visual(cant222.Value);
            Actualizar224Visual(cant224.Value);

            // Nos suscribimos a los dos eventos por separado
            cant222.OnValueChanged += (viejo, nuevo) => Actualizar222Visual(nuevo);
            cant224.OnValueChanged += (viejo, nuevo) => Actualizar224Visual(nuevo);
        }
    }

    // 🔥 FUNCIÓN EXCLUSIVA PARA SUMAR ESPADAS (La llama el script espada)
    public void Sumar222(int cantidad)
    {
        if (!IsServer) return;
        cant222.Value += cantidad;
    }

    // 🔥 FUNCIÓN EXCLUSIVA PARA SUMAR ESCUDOS (La llama el script escudo)
    public void Sumar224(int cantidad)
    {
        if (!IsServer) return;
        cant224.Value += cantidad;
    }

    private void Actualizar222Visual(int valor)
    {
        if (texto222UI != null) texto222UI.text = "222: " + valor.ToString();
    }

    private void Actualizar224Visual(int valor)
    {
        if (texto224UI != null) texto224UI.text = "224: " + valor.ToString();
    }
}