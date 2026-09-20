using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotKioscoUI : MonoBehaviour
{
    [Header("Referencias UI Internas")]
    [SerializeField] private TextMeshProUGUI textoNombre;
    [SerializeField] private TextMeshProUGUI textoPrecio;
    [SerializeField] private Image imagenIcono;
    [SerializeField] private Button botonComprar;

    private itemsMenu armaAsignada;
    private pulperia tiendaManager;

    public void ConfigurarSlot(itemsMenu arma, pulperia manager)
    {
        armaAsignada = arma;
        tiendaManager = manager;

        if (textoNombre != null) textoNombre.text = arma.nombreArma;
        if (textoPrecio != null) textoPrecio.text = $"{arma.precioOro} Oro";
        if (imagenIcono != null && arma.icono != null) imagenIcono.sprite = arma.icono;

        botonComprar.onClick.RemoveAllListeners();
        botonComprar.onClick.AddListener(AlHacerClicEnComprar);
    }

    private void AlHacerClicEnComprar()
    {
        if (tiendaManager != null && armaAsignada != null)
        {
            tiendaManager.ComprarArma(armaAsignada);
        }
    }
}