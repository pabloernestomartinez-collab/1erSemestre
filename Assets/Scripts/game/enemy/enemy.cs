using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class enemy : NetworkBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] private enemigosData enemigosData;

    [Header("Vida del Enemigo (Netcode)")]
    [SerializeField] private int vidaMaxima = 100;
    public NetworkVariable<int> vidaActual = new NetworkVariable<int>(100);

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
        if (agente != null && enemigosData != null)
        {
            agente.speed = enemigosData.EnemigoVelocidad;
        }

        // SI SOMOS EL SERVIDOR, inicializamos la vida usando el valor configurado
        if (IsServer)
        {
            vidaActual.Value = vidaMaxima;
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

        // Si no hay jugador que perseguir, permitimos que el script siga vivo por si recibe daño de la nada
        if (jugadorObjetivo == null || agente == null || !agente.enabled || !agente.isOnNavMesh) return;

        agente.SetDestination(jugadorObjetivo.position); // Persecución

        float distanciaAlJugador = Vector3.Distance(transform.position, jugadorObjetivo.position);

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
        if (prefabProyectil == null || puntoDisparo == null) return;

        tiempoSiguienteAtaque = Time.time + cooldownAtaque;
        Vector3 direccionHaciaJugador = (jugadorObjetivo.position - puntoDisparo.position).normalized;
        Quaternion rotacionHaciaJugador = Quaternion.LookRotation(direccionHaciaJugador);

        GameObject proyectilInstance = Instantiate(prefabProyectil, puntoDisparo.position, rotacionHaciaJugador);

        if (proyectilInstance.TryGetComponent<ProyectilEnemigo>(out ProyectilEnemigo scriptProyectil))
        {
            scriptProyectil.ConfigurarProyectil(enemigosData.EnemigoAtaque);
        }

        if (proyectilInstance.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        {
            netObj.Spawn();
        }
    }

    // Método central para procesar golpes del jugador
    public void RecibirDanio(int cantidadDanioBase)
    {
        if (!IsServer) return;

        // Aseguramos que existan los datos del scriptable para evitar errores
        int defensaMultiplicadora = (enemigosData != null) ? enemigosData.EnemigoDefensa : 1;

        //  El daño final es el daño del jugador multiplicado por la resistencia/fuerza de defensa del enemigo
        int danioFinal = cantidadDanioBase * defensaMultiplicadora;

        vidaActual.Value -= danioFinal;

       
        if (vidaActual.Value <= 0)
        {
            GetComponent<NetworkObject>().Despawn();
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

            if (agente != null && agente.enabled && agente.isOnNavMesh)
            {
                agente.ResetPath();
            }
        }
    }
}