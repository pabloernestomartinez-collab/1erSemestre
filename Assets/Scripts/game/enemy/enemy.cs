using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class enemy : NetworkBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] private enemigosData enemigosData;

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

        // Buscamos el componente de vida en el jugador que estamos persiguiendo
        PlayerStats stats = jugadorObjetivo.GetComponent<PlayerStats>();

        if (stats != null)
        {
            // El servidor le ordena al jugador restar la fuerza de ataque definida en tu ScriptableObject
            stats.RecibirDanio(enemigosData.EnemigoAtaque);
        }
    }

    private void EjecutarAtaqueADistancia()
    {
        tiempoSiguienteAtaque = Time.time + cooldownAtaque;

        // AQUÍ INSTANCIARÁS TU PREFAB DE FLECHA/HECHIZO EN EL SERVIDOR:
        // GameObject proyectil = Instantiate(prefabProyectil, puntoDisparo.position, Quaternion.identity);
        // proyectil.GetComponent<NetworkObject>().Spawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        // Si un jugador entra en el área de detección lo fijamos como presa
        if (other.CompareTag("Player") && jugadorObjetivo == null)
        {
            jugadorObjetivo = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        // Si el jugador que estábamos persiguiendo sale del rango el enemigo pierde el interés
        if (other.CompareTag("Player") && other.transform == jugadorObjetivo)
        {
            jugadorObjetivo = null;

            if (agente != null && agente.enabled)
            {
                agente.ResetPath(); // Borra la ruta actual para que se detenga inmediatamente
            }
        }
    }
}