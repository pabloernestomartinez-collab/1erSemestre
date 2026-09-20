using UnityEngine;

[CreateAssetMenu(fileName = "NuevoObjeto", menuName = "Objeto Tienda")]
public class menuInventario : ScriptableObject
{
    [Header("Información Visual")]
    public Sprite imagenObjeto;
    public string nombreObjeto;
    [TextArea] public string descripcion;

    [Header("Costo del Intercambio")]
    public int costoOro;
    public int costoHierba;
    public int costoSabiduria;
    public int costoPuntos;
}