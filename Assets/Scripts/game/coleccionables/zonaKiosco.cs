using UnityEngine;

public class zonaKiosco : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si el objeto que entró es el Player
        if (other.CompareTag("Player"))
        {
            // Opcional: nos aseguramos de que solo la persona que controla el personaje abra la UI localmente
            if (other.TryGetComponent<Unity.Netcode.NetworkObject>(out var netObj) && netObj.IsOwner)
            {
                if (MenuManager.Instance != null)
                {
                    MenuManager.Instance.AbrirKiosco();
                    Debug.Log("Entraste a la zona del Kiosco. Interfaz abierta.");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Al salir de la zona, se cierra automáticamente la interfaz por comodidad
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<Unity.Netcode.NetworkObject>(out var netObj) && netObj.IsOwner)
            {
                if (MenuManager.Instance != null)
                {
                    MenuManager.Instance.CerrarTodo();
                    Debug.Log("Saliste de la zona del Kiosco. Interfaz cerrada.");
                }
            }
        }
    }
}