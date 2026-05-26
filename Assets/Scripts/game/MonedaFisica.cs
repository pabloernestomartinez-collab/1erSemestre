using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class MonedaFisica : NetworkBehaviour
{
    private bool estaAgarrada = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (estaAgarrada) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<NetworkObject>(out NetworkObject playerNetObject))
            {
                // Solo el dueño local de este cuerpo puede solicitar el agarre
                if (playerNetObject.IsOwner)
                {
                    AgarrarMonedaServerRpc(playerNetObject.OwnerClientId);
                }
            }
        }
    }

    // "SendTo.Server" reemplaza al [ServerRpc]
    // "RpcInvokePermission.Everyone" reemplaza al RequireOwnership = false
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void AgarrarMonedaServerRpc(ulong jugadorQueAgarraId)
    {
        if (estaAgarrada) return;
        estaAgarrada = true;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(jugadorQueAgarraId, out var cliente))
        {
            GameObject jugadorGO = cliente.PlayerObject.gameObject;

            // 1. APAGAMOS EL SCRIPT DE RED DE LA MONEDA:
            // Al desactivar el NetworkTransform en el Servidor, este deja de forzar posiciones viejas en los clientes
            if (TryGetComponent<NetworkTransform>(out NetworkTransform netTransform))
            {
                netTransform.enabled = false;
            }

            // 2. APAGAMOS LAS FÍSICAS LOCALES DEL SERVIDOR
            if (TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            // 3. ACTUALIZAMOS JERARQUÍA EN RED
            GetComponent<NetworkObject>().ChangeOwnership(jugadorQueAgarraId);
            GetComponent<NetworkObject>().TrySetParent(jugadorGO.transform, false); // 'false' ayuda a mantener la escala original del prefab

            // 4. POSICIONAMOS EN EL SERVIDOR
            transform.localPosition = new Vector3(0f, 2f, 0f);
            transform.localRotation = Quaternion.identity;

            // 5. ENVIAMOS ORDEN DIRECTA A LOS CLIENTES
            // Forzamos visualmente a que todas las pantallas ejecuten el pegado de inmediato
            FijarPosicionLocalClientRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void FijarPosicionLocalClientRpc()
    {
        // Doble seguridad para el Cliente: apagamos sus físicas y su NetworkTransform local
        if (TryGetComponent<NetworkTransform>(out NetworkTransform netTransform)) netTransform.enabled = false;
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // Forzamos la posición relativa en la pantalla del cliente
        transform.localPosition = new Vector3(0f, 2f, 0f);
        transform.localRotation = Quaternion.identity;
    }
}