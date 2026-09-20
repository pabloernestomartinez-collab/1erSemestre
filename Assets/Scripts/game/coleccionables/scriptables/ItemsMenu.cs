using UnityEngine;

[CreateAssetMenu(fileName = "NuevoArmamento", menuName = "Kiosco/Armamento")]
public class itemsMenu : ScriptableObject
{
    [Header("Información del Armamento")]
    public string nombreArma = "Espada de Hierro";
    public Sprite icono;
    public string descripcion = "Aumenta el daño cuerpo a cuerpo.";

    [Header("Precio y Stats")]
    public int precioOro = 10;
    public int danioExtra = 15;
}