using Unity.Netcode;
using UnityEngine;

public class ItemRecolectable : NetworkBehaviour
{
    public enum TipoItem { Oro, Hierba, Sabiduria, Arma }

    [Header("Configuración General")]
    [SerializeField] private TipoItem tipoDeItem;

    [Header("Recursos Acumulables (Oro / Hierba / Sabiduría)")]
    [SerializeField] private int cantidadAOtorgar = 1;

    [Header("Datos de Arma / Objeto de Mochila (Solo si TipoItem = Arma)")]
    [SerializeField] private itemsMenu datosArma;

    private void OnTriggerEnter(Collider other)
    {
        // Solo el Servidor procesa las colisiones de red
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();

            if (stats != null)
            {
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

                    case TipoItem.Arma:
                        if (datosArma != null)
                        {
                            // 1. Aumentamos el daño del jugador en el Servidor
                            stats.ComprarArmaServerRpc(0, datosArma.danioExtra);

                            // 2. Si el jugador tiene el componente de Inventario, le añadimos el arma
                            if (other.TryGetComponent<InventarioPlayer>(out var inventario))
                            {
                                // Llamamos a una función ClientRpc para enviarlo a su pantalla local
                                AgregarArmaAlClienteClientRpc(datosArma.nombreArma, new ClientRpcParams
                                {
                                    Send = new ClientRpcSendParams
                                    {
                                        TargetClientIds = new ulong[] { stats.OwnerClientId }
                                    }
                                });
                            }
                        }
                        break;
                }

                // Destruye el objeto del mundo de forma sincronizada en toda la red
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    [ClientRpc]
    private void AgregarArmaAlClienteClientRpc(string nombreArma, ClientRpcParams clientRpcParams = default)
    {
        // Este código solo se ejecuta en el jugador que recogió el arma
        var localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayer != null && localPlayer.TryGetComponent<InventarioPlayer>(out var inventario))
        {
            // Agrega el arma a su inventario/mochila local
            // (puedes buscar el ScriptableObject o pasarlo directamente)
            Debug.Log($"¡Has recogido del suelo: {nombreArma}!");
        }
    }
}