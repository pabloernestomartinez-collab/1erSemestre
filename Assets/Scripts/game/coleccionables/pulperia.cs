using UnityEngine;

public class pulperia : MonoBehaviour
{
    [SerializeField] GameObject prefabsItemsMenu;
    [SerializeField] int maximaCantidadItems;
    [SerializeField] menuInventario[] listaItems;

    private itemsMenu itemsMenu;

    private void Start()
    {
        Transform padrePulperia = GameObject.FindWithTag("pulperia").transform;
        for (int i = 0; i < listaItems.Length; i++)
        {
            GameObject pulperiaObj = GameObject.Instantiate(prefabsItemsMenu, Vector2.zero, Quaternion.identity, padrePulperia);
            itemsMenu = pulperiaObj.GetComponent<itemsMenu>();
            itemsMenu.crearItems(listaItems[i]);
        }
    }
}