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
                            // Aumentamos el daño del jugador directamente en el Servidor
                            stats.danioMeleeJugador.Value += datosArma.danioExtra;

                            //  Notificamos al Cliente objetivo para que añada el arma a su inventario visual
                            AgregarArmaAlClienteClientRpc(datosArma.nombreArma, datosArma.danioExtra, new ClientRpcParams
                            {
                                Send = new ClientRpcSendParams
                                {
                                    TargetClientIds = new ulong[] { stats.OwnerClientId }
                                }
                            });
                        }
                        break;
                }

                // Destruye el objeto del mundo de forma sincronizada en toda la red
                if (NetworkObject != null && NetworkObject.IsSpawned)
                {
                    NetworkObject.Despawn();
                }
            }
        }
    }

    [ClientRpc]
    private void AgregarArmaAlClienteClientRpc(string nombreArma, int danio, ClientRpcParams clientRpcParams = default)
    {
        // Este código solo se ejecuta en el jugador local que recogió el arma
        var localPlayer = NetworkManager.Singleton?.LocalClient?.PlayerObject;
        if (localPlayer != null && localPlayer.TryGetComponent<InventarioPlayer>(out var inventario))
        {
            // Si tenemos el objeto asignado localmente, lo agregamos
            if (datosArma != null)
            {
                inventario.AgregarItemLocal(datosArma);
            }
            else
            {
                // De respaldo creamos una instancia temporal con los datos recibidos
                itemsMenu nuevaArma = ScriptableObject.CreateInstance<itemsMenu>();
                nuevaArma.nombreArma = nombreArma;
                nuevaArma.danioExtra = danio;
                inventario.AgregarItemLocal(nuevaArma);
            }

            //Debug.Log($"[INVENTARIO] ¡Has recogido del suelo: {nombreArma}!");
        }
    }

    private void OnDrawGizmos()
    {
        // Dibuja una esfera de color para visualizar el recolectable en el editor
        Gizmos.color = tipoDeItem switch
        {
            TipoItem.Oro => Color.yellow,
            TipoItem.Hierba => Color.green,
            TipoItem.Sabiduria => Color.cyan,
            TipoItem.Arma => Color.red,
            _ => Color.white
        };

        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}