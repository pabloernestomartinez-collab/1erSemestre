using Unity.Netcode;
using UnityEngine;

public class espada : MonoBehaviour
{
    [SerializeField] private int cantidadAumentar = 1;
    [SerializeField] private float velocidadRotacion = 100f;

    void Update()
    {
        // 1. Rotación visual continua
        transform.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime);

        // 2. 🔥 CONDICIONAL DE SEGURIDAD: Si cae al vacío (Y menor a -10)
        if (transform.position.y < -10f)
        {
            // Solo el servidor tiene permiso para destruir objetos en red
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning($"[Seguridad] {gameObject.name} se cayó del mapa (Y < -10) y fue destruido.");
                Destroy(gameObject);
            }
        }
    }

    // Tu detección de colisión física (Collider sólido)
    private void OnCollisionEnter(Collision collision)
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<PlayerScore>(out PlayerScore scoreJugador))
            {
                scoreJugador.SumarEspada(cantidadAumentar);
                Destroy(gameObject);
            }
        }
    }
}