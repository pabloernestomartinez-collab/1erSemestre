using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class itemsMenu : MonoBehaviour
{
    [SerializeField] Image imagenItems;
    [SerializeField] TextMeshProUGUI nombreItems;
    public void crearItems (menuInventario datosItems)
    {
        imagenItems.sprite = datosItems.imagenObjeto;
        nombreItems.text = datosItems.nombreObjeto;

    }

}
