using Unity.Netcode;
using UnityEngine;

public class escudo : MonoBehaviour
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
            // Solo el servidor tiene permitido destruir objetos en la red
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning($"[Seguridad] {gameObject.name} se cayó del mapa (Y < -10) y fue destruido.");
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Candado multijugador: Solo el servidor procesa el impacto físico y los puntos
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer) return;

        // 2. Comparamos el Tag usando el gameObject que provocó la colisión
        if (collision.gameObject.CompareTag("Player"))
        {
            // 3. Buscamos el componente PlayerScore en el objeto que nos chocó
            if (collision.gameObject.TryGetComponent<ArmasPlayer>(out ArmasPlayer scoreJugador))
            {
                // 🔥 Llama a la función específica de escudos
                scoreJugador.SumarEscudo(cantidadAumentar);

                // Destruimos el objeto en el servidor
                Destroy(gameObject);
            }
        }
    }
}
