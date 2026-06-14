using Unity.Netcode;
using UnityEngine;

public class escudo : MonoBehaviour
{
    [SerializeField] private int cantidadAumentar = 1;
    [SerializeField] private float velocidadRotacion = 100f;

    void Update()
    {
        transform.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer) return;

        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerScore>(out PlayerScore scoreJugador))
            {
                // 🔥 Llama a la función específica de escudos
                scoreJugador.SumarEscudo(cantidadAumentar);
                Destroy(gameObject);
            }
        }
    }
}