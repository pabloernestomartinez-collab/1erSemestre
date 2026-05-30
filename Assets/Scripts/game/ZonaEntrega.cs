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
                // Identificamos quién es el jugador que tiene la pantalla de esta PC
                ulong idDelJugadorReal = NetworkManager.Singleton.LocalClientId;

                // Si la moneda tiene guardado al jugador que la lleva, usamos ese ID por seguridad
                if (moneda.jugadorOBJ != null && moneda.jugadorOBJ.TryGetComponent<NetworkObject>(out NetworkObject pNet))
                {
                    idDelJugadorReal = pNet.OwnerClientId;
                }

                // 🔥 LA CORRECCIÓN CRÍTICA: 
                // Quitamos el filtro "IsOwner". Si la moneda entró a la meta y está agarrada, 
                // obligamos a que se ejecute el ServerRpc. 
                // Al ser un ServerRpc con permission "Everyone", el Cliente tiene permitido enviar este mensaje.
                EntregarMonedaServerRpc(idDelJugadorReal, monedaNetObj.NetworkObjectId);
            }
            return;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void EntregarMonedaServerRpc(ulong jugadorId, ulong monedaNetworkId)
    {
        // 🔒 EL CANDADO SE QUEDA: Solo el servidor procesa puntos y mueve objetos
        if (!NetworkManager.Singleton.IsServer) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(monedaNetworkId, out NetworkObject monedaNetObj))
        {
            // El Servidor busca al jugador real usando el ID que viajó desde el cliente
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(jugadorId, out var cliente))
            {
                if (cliente.PlayerObject.TryGetComponent<PlayerScore>(out PlayerScore puntosJugador))
                {
                    // El Servidor modifica la NetworkVariable legalmente
                    puntosJugador.AddPoints(1);
                    Debug.Log($"[SERVIDOR REAL] Punto otorgado legalmente al jugador {jugadorId}");
                }
            }
            else
            {
                Debug.LogError($"[SERVIDOR REAL] No se encontró al cliente {jugadorId}");
            }

            // El servidor congela la moneda en el origen
            if (monedaNetObj.TryGetComponent<MonedaFisica>(out MonedaFisica scriptMoneda))
            {
                scriptMoneda.DetenerSeguimientoYFijarEnOrigen();
            }
        }
    }
}