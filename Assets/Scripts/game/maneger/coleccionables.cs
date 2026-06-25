using UnityEngine;
using TMPro;

public class coleccionables : MonoBehaviour
{
    // Las variables donde el GameUIManager va a inyectar los textos de la UI
    [HideInInspector] public TextMeshProUGUI texto222UI;
    [HideInInspector] public TextMeshProUGUI texto224UI;

    // Tus variables de conteo de red o normales...
    private int cantidad222 = 0;
    private int cantidad224 = 0;

    // El método que llama el UI Manager para inicializar los textos en pantalla
    public void ForzarActualizacionVisual()
    {
        if (texto222UI != null) texto222UI.text = "222: " + cantidad222;
        if (texto224UI != null) texto224UI.text = "224: " + cantidad224;
    }
}