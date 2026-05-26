using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MonedaFisica : NetworkBehaviour
{
    private Coroutine devolverAutoridadCoroutine;

    // Detectamos el choque físico tradicional (Sin marcar Is Trigger)
    private void OnCollisionEnter(Collision collision)
    {
        // Verificamos si lo que chocó la moneda es un Jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            // Conseguimos el componente NetworkObject del jugador para saber quién es en la red
            if (collision.gameObject.TryGetComponent<NetworkObject>(out NetworkObject playerNetObject))
            {
                // Si el jugador que la chocó es el dueño local de esa pantalla Y NO es actualmente el dueño de la moneda
                if (playerNetObject.IsOwner && OwnerClientId != playerNetObject.OwnerClientId)
                {
                    // Le pedimos amablemente al servidor que nos transfiera la física de la moneda
                    SolicitarAutoridadServerRpc(playerNetObject.OwnerClientId);
                }
            }
        }
    }

    // RequireOwnership = false es VITAL aquí. Permite que un cliente que NO es dueño del objeto ejecute este RPC.
    [ServerRpc(RequireOwnership = false)]
    private void SolicitarAutoridadServerRpc(ulong nuevoDuenoId)
    {
        // El Servidor procesa la orden y cambia el dueño del NetworkObject al cliente que lo chocó
        GetComponent<NetworkObject>().ChangeOwnership(nuevoDuenoId);

        // --- SISTEMA DE RETORNO DE SEGURIDAD ---
        // Si el jugador se aleja, la moneda no debe quedarse asociada a él para siempre.
        // Cancelamos cualquier temporizador previo y arrancamos uno nuevo.
        if (devolverAutoridadCoroutine != null)
        {
            StopCoroutine(devolverAutoridadCoroutine);
        }
        devolverAutoridadCoroutine = StartCoroutine(DevolverAlServidorDespuesDeTiempo());
    }

    private IEnumerator DevolverAlServidorDespuesDeTiempo()
    {
        // Esperamos 3 segundos después del último empujón
        yield return new WaitForSeconds(3f);

        // Si el objeto sigue vivo y estamos en el servidor, le quitamos la propiedad al cliente
        // y la regresamos al Servidor (ID: 0 o NetworkManager.ServerClientId)
        if (IsServer && GetComponent<NetworkObject>() != null)
        {
            GetComponent<NetworkObject>().RemoveOwnership();
        }
    }
}
