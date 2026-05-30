using Unity.Netcode;
using UnityEngine;

public class MonedaFisica : NetworkBehaviour
{
    public Transform jugadorOBJ; // Guardará al jugador que debemos seguir
    public bool estaAgarrada = false; // la zona de entrega puede revisar este dato

    private void OnCollisionEnter(Collision collision)
    {
        if (estaAgarrada) return;
        if (!IsSpawned) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<NetworkObject>(out NetworkObject playerNetObject))
            {
                if (playerNetObject.IsOwner)
                {
                    AgarrarMonedaServerRpc(playerNetObject.OwnerClientId);
                }
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void AgarrarMonedaServerRpc(ulong jugadorQueAgarraId)
    {
        if (estaAgarrada) return;
        estaAgarrada = true;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(jugadorQueAgarraId, out var cliente))
        {
            GameObject jugadorGO = cliente.PlayerObject.gameObject;

            // Desactivamos físicas por completo en el servidor
            if (TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.isKinematic = true;
            if (TryGetComponent<Collider>(out Collider col)) col.isTrigger = true;

            // Cambiamos el dueño en la red
            GetComponent<NetworkObject>().ChangeOwnership(jugadorQueAgarraId);

            // Asignamos a quién seguir en el Servidor
            jugadorOBJ = jugadorGO.transform;

            // Le avisamos a todas las pantallas que guarden a qué jugador seguir
            SetSeguimientoClientRpc(jugadorGO.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SetSeguimientoClientRpc(ulong jugadorNetworkId)
    {
        estaAgarrada = true;

        // Desactivamos físicas en los clientes
        if (TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.isKinematic = true;
        if (TryGetComponent<Collider>(out Collider col)) col.isTrigger = true;

        // Cada cliente busca en su propia pantalla al jugador usando su ID de red
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(jugadorNetworkId, out NetworkObject jugadorNetObj))
        {
            jugadorOBJ = jugadorNetObj.transform;
        }
    }

    void Update()
    {
        // Si ya fue agarrada y tenemos a quién seguir, copiamos su posición frame a frame
        if (estaAgarrada && jugadorOBJ != null)
        {
            transform.position = jugadorOBJ.position + new Vector3(0f, 1f, 0f);
            transform.rotation = Quaternion.identity; // Se queda recta siempre
        }
    }

    public void DetenerSeguimientoYFijarEnOrigen()
    {
        if (!IsServer) return;

        estaAgarrada = false;
        jugadorOBJ = null;

        // Despawnear y destruir el objeto en red.
        // El parámetro 'true' le dice a Unity que destruya el objeto de la jerarquía 
        // tanto en el Host como en el Cliente al mismo tiempo de forma legal.
        if (GetComponent<NetworkObject>().IsSpawned)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}