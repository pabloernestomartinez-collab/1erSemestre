using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InventarioPlayer : NetworkBehaviour
{
    public List<itemsMenu> listaDeItems = new List<itemsMenu>();
    public Action OnInventarioCambiado;

    public void AgregarItemLocal(itemsMenu nuevoItem)
    {
        if (nuevoItem == null) return;
        listaDeItems.Add(nuevoItem);
        OnInventarioCambiado?.Invoke();
    }
}