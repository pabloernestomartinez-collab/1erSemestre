using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkSpawner : NetworkBehaviour
{
    [Header("Prefabs de Red")]
    [SerializeField] private GameObject prefabEspada;
    [SerializeField] private GameObject prefabEscudo;
    [SerializeField] private GameObject prefab222;
    [SerializeField] private GameObject prefab224;
    [SerializeField] private GameObject prefab226;
    [SerializeField] private GameObject prefab228;
    [SerializeField] private GameObject prefab230;
    [SerializeField] private GameObject prefab232;
    [SerializeField] private GameObject prefab234;

    [Header("Configuración del Spawn")]
    private int cantidadInicialCadaUno = 2;
    private float radioSpawn = 20f; // Qué tan lejos del centro pueden aparecer
    private float alturaSpawn = 3f;   // Altura para que floten del suelo

    public override void OnNetworkSpawn()
    {
        // Los clientes ignoran este script por completo.
        if (!IsServer) return;

        SpawnearObjetosIniciales();
    }

    private void SpawnearObjetosIniciales()
    {
        // Spawneamos las espadas
        for (int i = 0; i < cantidadInicialCadaUno; i++)
        {
            Vector3 posicionAleatoria = GenerarPosicionAleatoria();
            SpawnearObjetoEnRed(prefabEspada, posicionAleatoria);
        }

        // Spawneamos los escudos
        for (int i = 0; i < cantidadInicialCadaUno; i++)
        {
            Vector3 posicionAleatoria = GenerarPosicionAleatoria();
            SpawnearObjetoEnRed(prefabEscudo, posicionAleatoria);
        }

        // Spawneamos los 222
        for (int i = 0; i < cantidadInicialCadaUno; i++)
        {
            Vector3 posicionAleatoria = GenerarPosicionAleatoria();
            SpawnearObjetoEnRed(prefab222, posicionAleatoria);
        }
        // Spawneamos los 224
        for (int i = 0; i < cantidadInicialCadaUno; i++)
        {
            Vector3 posicionAleatoria = GenerarPosicionAleatoria();
            SpawnearObjetoEnRed(prefab224, posicionAleatoria);
        }
        // Spawneamos los 226
        for (int i = 0; i < cantidadInicialCadaUno; i++)
        {
            Vector3 posicionAleatoria = GenerarPosicionAleatoria();
            SpawnearObjetoEnRed(prefab226, posicionAleatoria);
        }
        // Spawneamos los 228
        for (int i = 0; i < cantidadInicialCadaUno; i++)
        {
            Vector3 posicionAleatoria = GenerarPosicionAleatoria();
            SpawnearObjetoEnRed(prefab228, posicionAleatoria);
        }
        // Spawneamos los 230
        for (int i = 0; i < cantidadInicialCadaUno; i++)
        {
            Vector3 posicionAleatoria = GenerarPosicionAleatoria();
            SpawnearObjetoEnRed(prefab230, posicionAleatoria);
        }
        // Spawneamos los 232
        for (int i = 0; i < cantidadInicialCadaUno; i++)
        {
            Vector3 posicionAleatoria = GenerarPosicionAleatoria();
            SpawnearObjetoEnRed(prefab232, posicionAleatoria);
        }
        // Spawneamos los 234
        for (int i = 0; i < cantidadInicialCadaUno; i++)
        {
            Vector3 posicionAleatoria = GenerarPosicionAleatoria();
            SpawnearObjetoEnRed(prefab234, posicionAleatoria);
        }

    }

    private void SpawnearObjetoEnRed(GameObject prefab, Vector3 posicion)
    {
        //if (prefab == null) return;

        // 1. Instanciamos el objeto de manera tradicional en el Servidor
        GameObject nuevoObjeto = Instantiate(prefab, posicion, Quaternion.identity);

        // 2. Le pedimos su componente NetworkObject
        if (nuevoObjeto.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        {
            netObj.Spawn();
        }
    
    }

    private Vector3 GenerarPosicionAleatoria()
    {
        // Genera un punto aleatorio en un círculo alrededor del centro del mapa (0,0,0)
        Vector2 puntoCirculo = Random.insideUnitCircle * radioSpawn;
        return new Vector3(puntoCirculo.x, alturaSpawn, puntoCirculo.y);
    }


}