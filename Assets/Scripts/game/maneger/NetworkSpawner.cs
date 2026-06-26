using Unity.Netcode;
using UnityEngine;

public class NetworkSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] prefabsItems;
    [SerializeField] private float radioSpawn = 400f;
    [SerializeField] private int cantidadA_Spawnear = 50;

    // 🔥 CAMBIO CRÍTICO: Usamos OnNetworkSpawn para que se ejecute en el momento exacto de la red
    public override void OnNetworkSpawn()
    {
        // REGLA DE ORO: Solo el servidor/host calcula y distribuye los objetos
        if (!IsServer) return;

        SpawnearMundo();
    }

    private void SpawnearMundo()
    {
        for (int i = 0; i < cantidadA_Spawnear; i++)
        {
            // Elegimos un ítem al azar de la lista
            GameObject prefabElegido = prefabsItems[Random.Range(0, prefabsItems.Length)];

            if (prefabElegido == null) continue;

            // Calculamos posición aleatoria en tu mapa gigante
            Vector3 posicionAleatoria = new Vector3(
                Random.Range(-radioSpawn, radioSpawn),
                0.5f, // Altura para que no atraviese el suelo
                Random.Range(-radioSpawn, radioSpawn)
            );

            // Instanciamos en el servidor
            GameObject nuevoItem = Instantiate(prefabElegido, posicionAleatoria, Quaternion.identity);

            // Le indicamos a la red que este objeto debe aparecer en las pantallas de todos los clientes
            if (nuevoItem.TryGetComponent<NetworkObject>(out NetworkObject netObj))
            {
                netObj.Spawn();
            }
        }
    }
}