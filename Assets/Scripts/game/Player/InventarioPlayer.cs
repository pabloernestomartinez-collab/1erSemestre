using System.Collections.Generic;
using UnityEngine;

public class InventarioPlayer : MonoBehaviour
{
    // Lista de ítems en posesión
    public List<itemsMenu> listaDeItems = new List<itemsMenu>();

    // Evento para notificar a la UI de la mochila que hay un ítem nuevo
    public System.Action OnInventarioCambiado;

    public void AgregarItem(itemsMenu nuevoItem)
    {
        listaDeItems.Add(nuevoItem);
        Debug.Log($"Añadido {nuevoItem.nombreArma} al inventario.");

        // Notificamos a la UI para que cree el ítem visualmente
        OnInventarioCambiado?.Invoke();
    }
}