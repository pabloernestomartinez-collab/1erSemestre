using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI; //Para usar el NavMesh 

public class enemy : NetworkBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] private enemigosData enemigosData;

    [Header("Parámetros de Caza")]
    [SerializeField] private float velocidadPersecucion = 3.5f;

    private NavMeshAgent agente;
    private Transform jugadorObjetivo = null; // Guarda al jugador que está persiguiendo

    public override void OnNetworkSpawn()
    {
        agente = GetComponent<NavMeshAgent>();

        // Configuramos la velocidad del enemigo usando los datos de nuestro scriptable u objeto
        if (agente != null)
        {
            agente.speed = velocidadPersecucion;
        }

        // El NavMesh solo debe activarse y calcular caminos en el Servidor.
        if (!IsServer && agente != null)
        {
            agente.enabled = false;
        }
    }

    void Update()
    {
        if (!IsServer) return;

        // Si tenemos un objetivo asignado, actualizamos su posición en el mapa para que lo persiga
        if (jugadorObjetivo != null && agente != null && agente.enabled)
        {
            agente.SetDestination(jugadorObjetivo.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        // Si un jugador entra en el área de detección y no tenemos objetivo, lo fijamos como presa
        if (other.CompareTag("Player") && jugadorObjetivo == null)
        {
            jugadorObjetivo = other.transform;

            if (other.TryGetComponent<NetworkObject>(out var netObj))
            {
                //Debug.Log($"[{enemigosData.EnemigoNombre}] ¡Fijando objetivo! Persiguiendo al Jugador {netObj.OwnerClientId}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        // Si el jugador que estábamos persiguiendo sale del rango, el enemigo se frena
        if (other.CompareTag("Player") && other.transform == jugadorObjetivo)
        {
            //Debug.Log($"[{enemigosData.EnemigoNombre}] El objetivo escapó. Volviendo a estado de alerta.");
            jugadorObjetivo = null;

            if (agente != null && agente.enabled)
            {
                agente.ResetPath(); // Borra la ruta actual para que se detenga
            }
        }
    }
}