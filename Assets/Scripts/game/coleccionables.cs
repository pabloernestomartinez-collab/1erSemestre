using Unity.Netcode; // Requerido para heredar de NetworkBehaviour y usar NetworkObject, IsServer, etc.
using UnityEngine;

public class coleccionables : NetworkBehaviour
{
    [SerializeField] private GameObject collectiblePrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnColeccionables();
        }
    }

    private void SpawnColeccionables()    // Método privado encargado de la lógica matemática y física de la creación

    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(-4f, 4f), 10f, Random.Range(-4f, 4f));

            GameObject go = Instantiate(collectiblePrefab, randomPos, Quaternion.identity);

            go.GetComponent<NetworkObject>().Spawn(); // la orden  del servidor, lo  replicaen las pantallas de TODOS player

        }
    }
}