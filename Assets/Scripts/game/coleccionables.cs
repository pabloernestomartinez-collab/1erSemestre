using Unity.Netcode; // Requerido para heredar de NetworkBehaviour y usar NetworkObject, IsServer, etc.
using UnityEngine;

public class coleccionables : NetworkBehaviour
{
    // El Prefab del objeto que queremos crear (moneda, gema, etc.). 
    // REQUISITO CRÍTICO: Este prefab DEBE tener el componente 'NetworkObject' añadido y estar registrado en la lista de Prefabs del NetworkManager.
    [SerializeField] private GameObject collectiblePrefab;

    // Este método nativo de Netcode se ejecuta automáticamente en cuanto el objeto "despierta" en la red al cargar la escena.
    public override void OnNetworkSpawn()
    {
        // FILTRO DE AUTORIDAD MÁS IMPORTANTE: Solo la computadora que actúa como Servidor (o Host) ejecutará lo que está adentro.
        // Los Clientes (jugadores invitados) ignorarán por completo este bloque, evitando que tiren monedas por su cuenta.
        if (IsServer)
        {
            SpawnColeccionables();
        }
    }

    // Método privado encargado de la lógica matemática y física de la creación
    private void SpawnColeccionables()
    {
        // Un bucle simple que se repetirá 3 veces para generar 3 objetos en el mapa
        for (int i = 0; i < 3; i++)
        {
            // Calculamos una posición aleatoria en los ejes X y Z (entre -4 y 4), fijando la altura Y en 10f para que caigan desde el cielo.
            Vector3 randomPos = new Vector3(Random.Range(-4f, 4f), 10f, Random.Range(-4f, 4f));

            // PASO 1 (Local): Instanciamos el objeto usando el método tradicional de Unity. 
            // En este milisegundo exacto, el objeto SOLO existe en la memoria del Servidor y nadie más lo puede ver.
            GameObject go = Instantiate(collectiblePrefab, randomPos, Quaternion.identity);

            // PASO 2 (Red): Buscamos el componente 'NetworkObject' del clon que acabamos de crear y llamamos a '.Spawn()'.
            // Esta es la orden mágica que toma el objeto del servidor, lo empaqueta y replica clones idénticos en las pantallas de TODOS los clientes conectados.
            go.GetComponent<NetworkObject>().Spawn();
        }
    }
}