using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class enemy : NetworkBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] private enemigosData enemigosData;

    [Header("Referencias de Ataque a Distancia")]
    [SerializeField] private GameObject prefabProyectil; // El prefab de la bola de fuego
    [SerializeField] private Transform puntoDisparo;    // arma del enemigo

    [Header("Rangos de Ataque (Opcionales para ajustar)")]
    [SerializeField] private float rangoMelee = 2f;
    [SerializeField] private float rangoDistancia = 15f;
    [SerializeField] private float cooldownAtaque = 1.5f; // Tiempo de espera entre golpes

    private NavMeshAgent agente;
    private Transform jugadorObjetivo = null; // Guarda al jugador que está persiguiendo
    private float tiempoSiguienteAtaque = 0f;

    public override void OnNetworkSpawn()
    {
        agente = GetComponent<NavMeshAgent>();

        // Configuramos la velocidad del enemigo usando los datos de nuestro scriptable
        if (agente != null)
        {
            agente.speed = enemigosData.EnemigoVelocidad;
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

        if (jugadorObjetivo == null || agente == null || !agente.enabled) return;

        agente.SetDestination(jugadorObjetivo.position);// Persecución: Actualizamos la posición del jugador objetivo

        float distanciaAlJugador = Vector3.Distance(transform.position, jugadorObjetivo.position);// Ataques distancia

        if (Time.time >= tiempoSiguienteAtaque)
        {
            // Intentar Ataque Cuerpo a Cuerpo (Melee)
            if (enemigosData.Emelee && distanciaAlJugador <= rangoMelee)
            {
                EjecutarAtaqueMelee();
            }
            // Intentar Ataque a Distancia
            else if (enemigosData.Distancia && distanciaAlJugador <= rangoDistancia && distanciaAlJugador > rangoMelee)
            {
                EjecutarAtaqueADistancia();
            }
        }
    }

    private void EjecutarAtaqueMelee()
    {
        tiempoSiguienteAtaque = Time.time + cooldownAtaque;

        PlayerStats stats = jugadorObjetivo.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.RecibirDanio(enemigosData.EnemigoAtaque);
        }
    }

    private void EjecutarAtaqueADistancia()
    {
        tiempoSiguienteAtaque = Time.time + cooldownAtaque;
        Vector3 direccionHaciaJugador = (jugadorObjetivo.position - puntoDisparo.position).normalized;
        Quaternion rotacionHaciaJugador = Quaternion.LookRotation(direccionHaciaJugador);

        GameObject proyectilInstance = Instantiate(prefabProyectil, puntoDisparo.position, rotacionHaciaJugador);        // Instanciamos el proyectil 


        //  daño del ScriptableObject al proyectil para que sepa cuánto sacar al chocar
        if (proyectilInstance.TryGetComponent<ProyectilEnemigo>(out ProyectilEnemigo scriptProyectil))
        {
            scriptProyectil.ConfigurarProyectil(enemigosData.EnemigoAtaque);
        }

        if (proyectilInstance.TryGetComponent<NetworkObject>(out NetworkObject netObj))        //  Spawn en red

        {
            netObj.Spawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player") && jugadorObjetivo == null)
        {
            jugadorObjetivo = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player") && other.transform == jugadorObjetivo)
        {
            jugadorObjetivo = null;

            if (agente != null && agente.enabled)
            {
                agente.ResetPath();
            }
        }
    }
}