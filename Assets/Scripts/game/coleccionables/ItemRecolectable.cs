using Unity.Netcode;
using UnityEngine;

public class ItemRecolectable : NetworkBehaviour
{
    public enum TipoItem { Oro, Hierba, Sabiduria }

    [Header("Configuración del Recolectable")]
    [SerializeField] private TipoItem tipoDeItem;
    [SerializeField] private int cantidadAOtorgar = 1;

    private void OnTriggerEnter(Collider other)
    {
        // Solo el Servidor procesa las colisiones de red
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();

            if (stats != null)
            {
                // Aumentamos la cantidad correspondiente según el tipo
                switch (tipoDeItem)
                {
                    case TipoItem.Oro:
                        stats.SumarOro(cantidadAOtorgar);
                        break;
                    case TipoItem.Hierba:
                        stats.SumarHierba(cantidadAOtorgar);
                        break;
                    case TipoItem.Sabiduria:
                        stats.SumarSabiduria(cantidadAOtorgar);
                        break;
                }

                // Despawn de red: destruye el ítem en el servidor y lo sincroniza con todos los clientes
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}