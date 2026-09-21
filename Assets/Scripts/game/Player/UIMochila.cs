using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class UIMochila : MonoBehaviour
{
    [Header("Referencias de UI Mochila")]
    [SerializeField] private Transform contenedorGrilla;  // Objeto con GridLayoutGroup
    [SerializeField] private GameObject prefabSlotMochila; // Prefab UI del Slot de la Mochila

    private InventarioPlayer inventarioLocal;

    private void OnEnable()
    {
        // Al abrir la mochila, aseguramos vincular y refrescar datos
        StartCoroutine(EsperarYVincularPlayer());
    }

    private IEnumerator EsperarYVincularPlayer()
    {
        // Esperamos en bucle hasta que Netcode haya spawneado al jugador local
        while (NetworkManager.Singleton == null ||
               NetworkManager.Singleton.LocalClient == null ||
               NetworkManager.Singleton.LocalClient.PlayerObject == null)
        {
            yield return null;
        }

        var netObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (netObj.TryGetComponent<InventarioPlayer>(out var inv))
        {
            // Evitamos doble suscripción
            if (inventarioLocal != null)
            {
                inventarioLocal.OnInventarioCambiado -= RefrescarMochila;
            }

            inventarioLocal = inv;
            inventarioLocal.OnInventarioCambiado += RefrescarMochila;

            RefrescarMochila();
        }
    }

    public void RefrescarMochila()
    {
        if (inventarioLocal == null || contenedorGrilla == null) return;

        // Limpieza limpia y en sentido inverso (evita desorden en la grilla antes del re-instanciado)
        for (int i = contenedorGrilla.childCount - 1; i >= 0; i--)
        {
            Destroy(contenedorGrilla.GetChild(i).gameObject);
        }

        // Generar un slot por cada arma que el jugador ha comprado o recogido
        foreach (itemsMenu item in inventarioLocal.listaDeItems)
        {
            if (item == null) continue;

            GameObject nuevoSlot = Instantiate(prefabSlotMochila, contenedorGrilla);

            if (nuevoSlot.TryGetComponent<SlotMochilaUI>(out var slotScript))
            {
                slotScript.ConfigurarSlot(item);
            }
        }
    }

    private void OnDisable()
    {
        if (inventarioLocal != null)
        {
            inventarioLocal.OnInventarioCambiado -= RefrescarMochila;
        }
    }

    private void OnDestroy()
    {
        if (inventarioLocal != null)
        {
            inventarioLocal.OnInventarioCambiado -= RefrescarMochila;
        }
    }
}