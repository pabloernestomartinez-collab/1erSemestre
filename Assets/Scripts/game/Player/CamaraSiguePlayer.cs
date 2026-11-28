using UnityEngine;
using Unity.Netcode;

public class CamaraSiguePlayer : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -8f);
    [SerializeField] private float suavizado = 10f;

    private Transform objetivoPlayer;
    private PlayerAttack playerAttack;
    private Quaternion rotacionEstable = Quaternion.identity;

    private void LateUpdate()
    {
        if (objetivoPlayer == null)
        {
            BuscarJugadorLocal();
            return;
        }

        // Solo actualizamos la rotación de la cámara si el jugador NO está haciendo el giro de ataque
        bool girandoEnAtaque = playerAttack != null && playerAttack.EstaGirando;
        if (!girandoEnAtaque)
        {
            rotacionEstable = objetivoPlayer.rotation;
        }

        Vector3 posicionDeseada = objetivoPlayer.position + (rotacionEstable * offset);

        transform.position = Vector3.Lerp(transform.position, posicionDeseada, Time.deltaTime * suavizado);
        transform.LookAt(objetivoPlayer.position + Vector3.up * 1.5f);
    }

    private void BuscarJugadorLocal()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.LocalClient != null &&
            NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            AsignarJugador(NetworkManager.Singleton.LocalClient.PlayerObject.transform);
            return;
        }

        MovimientoPlayer[] jugadores = Object.FindObjectsByType<MovimientoPlayer>(FindObjectsSortMode.None);
        foreach (var p in jugadores)
        {
            if (p.IsOwner)
            {
                AsignarJugador(p.transform);
                break;
            }
        }
    }

    private void AsignarJugador(Transform playerTransform)
    {
        objetivoPlayer = playerTransform;
        playerAttack = playerTransform.GetComponent<PlayerAttack>();
        rotacionEstable = playerTransform.rotation;
    }
}