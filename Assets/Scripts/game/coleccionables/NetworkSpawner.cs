using Unity.Netcode;
using UnityEngine;

public class NetworkSpawner : NetworkBehaviour
{
    [Header("Configuración del Spawner")]
    [SerializeField] private GameObject[] prefabsItems;
    [SerializeField] private int cantidadA_Spawnear = 50;

    [Header("Zona de Spawn (Delimitador)")]
    [SerializeField] private BoxCollider zonaSpawn; // Asigna el BoxCollider del mapa en el Inspector

    [Header("Altura de Generación")]
    [SerializeField] private float alturaFijaY = 0.5f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        SpawnearMundo();
    }

    private void SpawnearMundo()
    {
        if (prefabsItems == null || prefabsItems.Length == 0)
        {
            Debug.LogWarning("[NetworkSpawner] No hay prefabs asignados en 'prefabsItems'.");
            return;
        }

        if (zonaSpawn == null)
        {
            Debug.LogError("[NetworkSpawner] Debes asignar un BoxCollider en 'zonaSpawn'.");
            return;
        }

        // Extrae los límites globales (min y max) del BoxCollider en la escena
        Bounds limites = zonaSpawn.bounds;

        int instanciadosExitosos = 0;

        for (int i = 0; i < cantidadA_Spawnear; i++)
        {
            GameObject prefabElegido = prefabsItems[Random.Range(0, prefabsItems.Length)];
            if (prefabElegido == null) continue;

            // Genera coordenadas aleatorias dentro de los bordes exactos de la caja
            float randomX = Random.Range(limites.min.x, limites.max.x);
            float randomZ = Random.Range(limites.min.z, limites.max.z);

            Vector3 posicionFinal = new Vector3(randomX, alturaFijaY, randomZ);

            GameObject nuevoItem = Instantiate(prefabElegido, posicionFinal, Quaternion.identity);

            if (nuevoItem.TryGetComponent<NetworkObject>(out NetworkObject netObj))
            {
                netObj.Spawn();
                instanciadosExitosos++;
            }
        }

        Debug.Log($"[SERVIDOR] Se generaron {instanciadosExitosos} de {cantidadA_Spawnear} ítems dentro del área del BoxCollider.");
    }
}