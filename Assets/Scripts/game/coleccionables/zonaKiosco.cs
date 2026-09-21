using UnityEngine;
using Unity.Netcode;

public class zonaKiosco : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<NetworkObject>(out var netObj) && netObj.IsOwner)            // Solo abrimos la UI si el objeto que entró pertenece al cliente local

            {
                if (MenuManager.Instance != null)
                {
                    MenuManager.Instance.AbrirKiosco();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Solo cerramos la UI si el objeto que salió pertenece al cliente local
            if (other.TryGetComponent<NetworkObject>(out var netObj) && netObj.IsOwner)
            {
                if (MenuManager.Instance != null)
                {
                    MenuManager.Instance.CerrarTodo();
                }
            }
        }
    }
}