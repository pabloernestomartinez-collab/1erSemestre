using Unity.Netcode;
using UnityEngine;

public class NetworkSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] prefabsItems;
    [SerializeField] private float radioSpawn = 400f;
    [SerializeField] private int cantidadA_Spawnear = 50;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        SpawnearMundo();
    }

    private void SpawnearMundo()
    {
        for (int i = 0; i < cantidadA_Spawnear; i++)
        {
            GameObject prefabElegido = prefabsItems[Random.Range(0, prefabsItems.Length)]; // Elegimos un ítem al azar de la lista


            if (prefabElegido == null) continue;

            Vector3 posicionAleatoria = new Vector3(Random.Range(-radioSpawn, radioSpawn),0.5f,Random.Range(-radioSpawn, radioSpawn)); // posición aleatoria 


            GameObject nuevoItem = Instantiate(prefabElegido, posicionAleatoria, Quaternion.identity); // Instanciamos en el servidor


            // Le indicamos a la red que este objeto debe aparecer en las pantallas de todos los clientes
            if (nuevoItem.TryGetComponent<NetworkObject>(out NetworkObject netObj))
            {
                netObj.Spawn();
            }
        }
    }
}