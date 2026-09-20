using UnityEngine;

public class CamaraSiguePlayer : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -8f); // Distancia e inclinación
    [SerializeField] private float suavizado = 10f;                     // Suavidad de seguimiento

    private Transform objetivoPlayer;

    private void LateUpdate()
    {
        // 1. Si no tenemos objetivo, buscamos al Player local que nos pertenece
        if (objetivoPlayer == null)
        {
            BuscarJugadorLocal();
            return;
        }

        // 2. Calculamos la posición deseada basada en la posición Y ROTACIÓN del Player
        Vector3 posicionDeseada = objetivoPlayer.position + (objetivoPlayer.rotation * offset);

        // 3. Movemos suavemente la cámara a esa posición
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, Time.deltaTime * suavizado);

        // 4. Hacemos que la cámara mire siempre hacia el jugador
        transform.LookAt(objetivoPlayer.position + Vector3.up * 1.5f);
    }

    private void BuscarJugadorLocal()
    {
        // Busca entre todos los jugadores en red y encuentra al que nos pertenece (IsOwner)
        MovimientoPlayer[] jugadores = FindObjectsOfType<MovimientoPlayer>();
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
