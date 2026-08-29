using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class patrullaje : NetworkBehaviour
{
    [SerializeField] private Transform[] Waypoints;
    private int puntoActual = 0;
    private NavMeshAgent agente;

    public override void OnNetworkSpawn()
    {
        agente = GetComponent<NavMeshAgent>(); // Guardamos el componente de navegación del enemigo


        if (!IsServer) return;

        StartCoroutine(BucleTeletransporte()); // Iniciamos el bucle de teletransporte una SOLA vez al nacer

    }

    private IEnumerator BucleTeletransporte()
    {
        // cada 5 segundos durante toda la partida
        while (true)
        {
            yield return new WaitForSeconds(20f);

            if (agente != null && agente.enabled)//  Apagamos el NavMesh para evitar que la física se rompa con el cambio brusco

            {
                agente.enabled = false;
            }

            transform.position = Waypoints[puntoActual].position;

            if (agente != null) // Volvemos a prender el NavMesh en su nueva ubicación para que siga persiguiendo

            {
                agente.enabled = true;
            }

            puntoActual = (puntoActual + 1) % Waypoints.Length;
        }
    }
}
