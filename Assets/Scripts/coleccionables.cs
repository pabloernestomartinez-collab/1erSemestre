using Unity.Netcode;
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

    private void SpawnColeccionables()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(-4f, 4f),10f,Random.Range(-4f, 4f));
            GameObject go = Instantiate(collectiblePrefab, randomPos, Quaternion.identity); // Instanciamos el objeto en el servidor
            go.GetComponent<NetworkObject>().Spawn(); // Esto hace que aparezca en todos los clientes
        }
    }
}