using UnityEngine;
using TMPro;

public class SlotMochilaUI : MonoBehaviour
{
    [Header("Referencias del Slot")]
    [SerializeField] private TextMeshProUGUI textoNombre;

    public void ConfigurarSlot(itemsMenu item)
    {
        if (textoNombre == null) return;

        if (item != null)
        {
            textoNombre.text = item.nombreArma;
        }
        else
        {
            textoNombre.text = string.Empty;
        }
    }
}