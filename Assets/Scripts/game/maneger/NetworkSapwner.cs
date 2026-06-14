using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkSpawner : NetworkBehaviour
{
    [Header("Prefabs de Red")]
    [SerializeField] private GameObject prefabEspada;
    [SerializeField] private GameObject prefabEscudo;

    [Header("Configuración del Spawn")]
    [SerializeField] private int cantidadInicialCadaUno = 5;
    [SerializeField] private float radioSpawn = 20f; // Qué tan lejos del centro pueden aparecer
    [SerializeField] private float alturaSpawn = 1f;   // Altura para que floten del suelo

    public override void OnNetworkSpawn()
    {
        // 🔥 SEGURIDAD MULTIJUGADOR: Solo el Servidor/Host tiene el poder de spawnear objetos.
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
    }

    private void SpawnearObjetoEnRed(GameObject prefab, Vector3 posicion)
    {
        if (prefab == null) return;

        // 1. Instanciamos el objeto de manera tradicional en el Servidor
        GameObject nuevoObjeto = Instantiate(prefab, posicion, Quaternion.identity);

        // 2. Le pedimos su componente NetworkObject
        if (nuevoObjeto.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        {
            // 🔥 ¡EL TRUCO MÁGICO! Esto le avisa a Netcode que lo clone en todos los clientes sincronizadamente
            netObj.Spawn();
        }
        else
        {
            Debug.LogError($"[Spawner] ¡El prefab {prefab.name} no tiene el componente NetworkObject pegado!");
        }
    }

    private Vector3 GenerarPosicionAleatoria()
    {
        // Genera un punto aleatorio en un círculo alrededor del centro del mapa (0,0,0)
        Vector2 puntoCirculo = Random.insideUnitCircle * radioSpawn;
        return new Vector3(puntoCirculo.x, alturaSpawn, puntoCirculo.y);
    }

    // Dibujar el radio en el editor de Unity para guiarte visualmente
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Vector3.up * alturaSpawn, radioSpawn);
    }
}