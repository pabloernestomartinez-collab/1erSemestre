using UnityEngine;
using Unity.Netcode;

public class CamaraSiguePlayer : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -8f); // Distancia e inclinación
    [SerializeField] private float suavizado = 10f;                     // Suavidad de seguimiento

    private Transform objetivoPlayer;

    private void LateUpdate()
    {
        if (objetivoPlayer == null)
        {
            BuscarJugadorLocal();
            return;
        }

        Vector3 posicionDeseada = objetivoPlayer.position + (objetivoPlayer.rotation * offset);

        transform.position = Vector3.Lerp(transform.position, posicionDeseada, Time.deltaTime * suavizado);

        transform.LookAt(objetivoPlayer.position + Vector3.up * 1.5f);
    }

    private void BuscarJugadorLocal()
    {
        // Intento principal: Referencia directa de Netcode
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.LocalClient != null &&
            NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            objetivoPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
            return;
        }

        // Respaldo: Búsqueda eficiente en la escena
        MovimientoPlayer[] jugadores = Object.FindObjectsByType<MovimientoPlayer>(FindObjectsSortMode.None);
        foreach (var p in jugadores)
        {
            if (p.IsOwner)
            {
                objetivoPlayer = p.transform;
                break;
            }
        }
    }
}