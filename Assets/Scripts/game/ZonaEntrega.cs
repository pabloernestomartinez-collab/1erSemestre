using Unity.Netcode;
using UnityEngine;

public class ZonaEntrega : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<MonedaFisica>(out MonedaFisica moneda))
        {
            // Si NO está agarrada, ignoramos el choque por completo
            if (!moneda.estaAgarrada) return;

            if (moneda.TryGetComponent<NetworkObject>(out NetworkObject monedaNetObj))
            {
                if (monedaNetObj.IsOwner)
                {
                    EntregarMonedaServerRpc(monedaNetObj.OwnerClientId, monedaNetObj.NetworkObjectId);
                }
            }
            return;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void EntregarMonedaServerRpc(ulong jugadorId, ulong monedaNetworkId)
    {


        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(monedaNetworkId, out NetworkObject monedaNetObj))
        {
            // 1. Buscamos al jugador para sumarle los puntos
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(jugadorId, out var cliente))
            {
                if (cliente.PlayerObject.TryGetComponent<PlayerScore>(out PlayerScore puntosJugador))
                {
                    puntosJugador.AddPoints(1);
                }
            }

            // 2. Le ordenamos a la moneda que se quede fija en (0,0,0)
            if (monedaNetObj.TryGetComponent<MonedaFisica>(out MonedaFisica scriptMoneda))
            {
                scriptMoneda.DetenerSeguimientoYFijarEnOrigen();
            }

            Debug.Log($"[Servidor] Moneda {monedaNetworkId} cobrada con éxito al jugador {jugadorId} y movida a (0,0,0)");
        }
    }
}