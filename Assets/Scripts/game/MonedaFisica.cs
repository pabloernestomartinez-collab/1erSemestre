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
                if (playerNetObject.IsOwner)  // Solo el dueño local de este cuerpo puede solicitar el agarre

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

            if (TryGetComponent<NetworkTransform>(out NetworkTransform netTransform))   // Al desactivar el NetworkTransform en el Servidor, este deja de forzar posiciones viejas en los clientes

            {
                netTransform.enabled = false;
            }

            if (TryGetComponent<Rigidbody>(out Rigidbody rb))  //  APAGAMOS LAS FÍSICAS LOCALES DEL SERVIDOR

            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            GetComponent<NetworkObject>().ChangeOwnership(jugadorQueAgarraId);            //  ACTUALIZAMOS JERARQUÍA EN RED

            GetComponent<NetworkObject>().TrySetParent(jugadorGO.transform, false); // 'false' ayuda a mantener la escala original del prefab

            transform.localPosition = new Vector3(0f, 2f, 0f);     //  POSICIONAMOS EN EL SERVIDOR

            transform.localRotation = Quaternion.identity;

            FijarPosicionLocalClientRpc();  // Forzamos visualmente a que todas las pantallas ejecuten el pegado de inmediato

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