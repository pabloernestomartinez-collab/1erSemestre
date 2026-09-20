using UnityEngine;

public class crafteo : MonoBehaviour
{
    [SerializeField] GameObject prefabsItemsMenu;
    [SerializeField] int maximaCantidadItems;
    [SerializeField] menuInventario[] listaItems;

    private itemsMenu itemsMenu;

    private void Start()
    {
        Transform padrePulperia = GameObject.FindWithTag("crafteo").transform;
        for (int i = 0; i < listaItems.Length; i++)
        {
            GameObject pulperiaObj = GameObject.Instantiate(prefabsItemsMenu, Vector2.zero, Quaternion.identity, padrePulperia);
            itemsMenu = pulperiaObj.GetComponent<itemsMenu>();
            itemsMenu.crearItems(listaItems[i]);
        }
    }
}