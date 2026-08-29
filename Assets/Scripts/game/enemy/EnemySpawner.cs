using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Prefabs Individuales de Enemigos")]
    [SerializeField] private GameObject prefabEnemigo1;
    [SerializeField] private GameObject prefabEnemigo2;
    //[SerializeField] private GameObject prefabEnemigo3;
    [Header("Configuración del Área")]
    [SerializeField] private float radioSpawn = 400f;
    [SerializeField] private float alturaSpawn = 1f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        StartCoroutine(SecuenciaSpawnEnemigos());// secuencia temporalizada
    }
    private IEnumerator SecuenciaSpawnEnemigos()//Ojo, este método no es normal; se va a ejecutar por etapas porque tiene: IEnumerator
    {

        yield return new WaitForSeconds(5f); // ESPERO 5 SEGUNDOS PARA QUE APAREZCA EL PRIMER ENEMIGO
        for (int i = 0; i < 20; i++)
        {
            Vector3 posicionAleatoria = new Vector3(Random.Range(-radioSpawn, radioSpawn),alturaSpawn,Random.Range(-radioSpawn, radioSpawn));//  posición aleatoria
            Quaternion rotacionAleatoria = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);// Rotación aleatoria para que miren a cualquier lado al nacer
            GameObject nuevoEnemigo = Instantiate(prefabEnemigo1, posicionAleatoria, rotacionAleatoria);// Instanciamos en el servidor
            if (nuevoEnemigo.TryGetComponent<NetworkObject>(out NetworkObject netObj))// Se lo mandamos por internet a los clientes
            {
                netObj.Spawn();
            }
        }

        yield return new WaitForSeconds(30f);

        for (int i = 0; i < 10; i++)
        {
            Vector3 posicionAleatoria = new Vector3(Random.Range(-radioSpawn, radioSpawn),alturaSpawn,Random.Range(-radioSpawn, radioSpawn));
            Quaternion rotacionAleatoria = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            GameObject nuevoEnemigo = Instantiate(prefabEnemigo2, posicionAleatoria, rotacionAleatoria);
            if (nuevoEnemigo.TryGetComponent<NetworkObject>(out NetworkObject netObj))
            {
                netObj.Spawn();
            }
        }

        //yield return new WaitForSeconds(30f);

        ////for (int i = 0; i < 2; i++)
        ////{
        //    Vector3 posicionAleatoria = new Vector3(Random.Range(-radioSpawn, radioSpawn),alturaSpawn,Random.Range(-radioSpawn, radioSpawn));
        //    Quaternion rotacionAleatoria = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        //    GameObject nuevoEnemigo = Instantiate(prefabEnemigo3, posicionAleatoria, rotacionAleatoria);
        //    if (nuevoEnemigo.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        //    {
        //        netObj.Spawn();
        //    }
        ////}
    }
}