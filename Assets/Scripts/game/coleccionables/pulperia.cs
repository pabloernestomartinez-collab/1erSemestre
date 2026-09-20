//using System.CollectionsCollections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class pulperia : MonoBehaviour
{
    [Header("Catálogo de Armas")]
    [SerializeField] private List<itemsMenu> catalogoArmas;

    [Header("Referencias de UI")]
    [SerializeField] private Transform contenedorGrilla; // Objeto con 'GridLayoutGroup'
    [SerializeField] private GameObject prefabSlotItem;   // Prefab con 'SlotKioscoUI'

    private void OnEnable()
    {
        GenerarTienda();
    }

    private void GenerarTienda()
    {
        // Limpiamos la grilla previa si ya existía
        foreach (Transform child in contenedorGrilla)
        {
            Destroy(child.gameObject);
        }

        // Instanciamos un slot por cada arma definida en el catálogo
        foreach (itemsMenu arma in catalogoArmas)
        {
            GameObject nuevoSlot = Instantiate(prefabSlotItem, contenedorGrilla);
            if (nuevoSlot.TryGetComponent<SlotKioscoUI>(out var slotScript))
            {
                slotScript.ConfigurarSlot(arma, this);
            }
        }
    }

    public void ComprarArma(itemsMenu arma)
    {
        // Buscamos las estadísticas del jugador local
        var netObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (netObj != null && netObj.TryGetComponent<PlayerStats>(out PlayerStats stats))
        {
            if (stats.oro.Value >= arma.precioOro)
            {
                // Solicitamos al servidor procesar el cobro e incremento de daño
                stats.ComprarArmaServerRpc(arma.precioOro, arma.danioExtra);
                Debug.Log($"Compra solicitada: {arma.nombreArma} por {arma.precioOro} de Oro.");
            }
            else
            {
                Debug.LogWarning("No tienes suficiente Oro para comprar esta arma.");
            }
        }
    }
}