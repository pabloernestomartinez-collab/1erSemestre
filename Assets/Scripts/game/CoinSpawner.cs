using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject monedaPrefab; // Arrastra aquí el PREFAB de tu moneda

    // Variables para el temporizador
    private float tiempoPorSpawn = 2.0f; // Tiempo en segundos entre cada moneda
    private float cronometro = 0f;

    public override void OnNetworkSpawn()
    {
        // Al iniciar en la red, reseteamos el cronómetro
        cronometro = 0f;
    }

    void Update()
    {
        // REGLA DE ORO: Solo el servidor calcula el tiempo y spawnea objetos.
        // Si no somos el servidor, este Update no hace nada.
        if (!IsServer) return;

        // Sumamos el tiempo que pasó desde el último frame
        cronometro += Time.deltaTime;

        // Si el cronómetro llega o supera 1 segundo...
        if (cronometro >= tiempoPorSpawn)
        {
            GenerarMonedaAleatoria();
            cronometro = 0f; // Reiniciamos el cronómetro para el siguiente segundo
        }
    }

    private void GenerarMonedaAleatoria()
    {
        if (monedaPrefab == null)
        {
            Debug.LogError("❌ [CoinSpawner] ¡Falta asignar el Prefab de la Moneda!");
            return;
        }

        // Tus rangos de posición personalizados
        float xAleatorio = Random.Range(-4f, 4f);
        float zAleatorio = Random.Range(-4f, 4f);
        float yPosicion = 10f;

        Vector3 posicionAleatoria = new Vector3(xAleatorio, yPosicion, zAleatorio);

        // 1. Instanciamos en la memoria del Servidor
        GameObject nuevaMoneda = Instantiate(monedaPrefab, posicionAleatoria, Quaternion.identity);

        // 2. Le pedimos a Netcode que la duplique en las pantallas de todos los jugadores
        nuevaMoneda.GetComponent<NetworkObject>().Spawn();

        Debug.Log($"[Servidor] Moneda automática creada en: {posicionAleatoria}");
    }
}