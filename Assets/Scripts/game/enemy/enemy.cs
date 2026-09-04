using System.Collections;
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
    [SerializeField] private GameObject prefabProyectil;
    [SerializeField] private Transform puntoDisparo;    // arma del enemigo

    [Header("Señal Visual de Ataque (Melee)")]
    [SerializeField] private GameObject senalVisualGolpe;
    [SerializeField] private float duracionSenalVisual = 0.5f; // Tiempo de aviso previo al golpe

    [Header("Rangos de Ataque (Opcionales para ajustar)")]
    [SerializeField] private float rangoMelee = 2.2f;
    [SerializeField] private float rangoDistancia = 15f;
    [SerializeField] private float cooldownAtaque = 1.5f; // Tiempo de espera entre golpes

    private NavMeshAgent agente;
    private Transform jugadorObjetivo = null; // Guarda al jugador que está persiguiendo
    private float tiempoSiguienteAtaque = 0f;
    private bool estaAtacandoMelee = false;   // Previene iniciar ataques solapados

    public override void OnNetworkSpawn()
    {
        agente = GetComponent<NavMeshAgent>();

        // señal visual empieza apagada 
        if (senalVisualGolpe != null)
        {
            senalVisualGolpe.SetActive(false);
        }

        if (agente != null && enemigosData != null)
        {
            agente.speed = enemigosData.EnemigoVelocidad;
        }

        // inicializamos la vida 
        if (IsServer)
        {
            vidaActual.Value = vidaMaxima;
        }

        //  NavMesh solo  en el Servidor.
        if (!IsServer && agente != null)
        {
            agente.enabled = false;
        }
    }

    void Update()
    {
        if (!IsServer) return;

        if (jugadorObjetivo == null || agente == null || !agente.enabled || !agente.isOnNavMesh) return;

        // if está preparando el golpe melee, detenemos el movimiento
        if (estaAtacandoMelee) return;

        agente.SetDestination(jugadorObjetivo.position); // Persecución

        float distanciaAlJugador = Vector3.Distance(transform.position, jugadorObjetivo.position);

        if (Time.time >= tiempoSiguienteAtaque)
        {
            // Intentar Ataque Cuerpo a Cuerpo
            if (enemigosData.Emelee && distanciaAlJugador <= rangoMelee)
            {
                StartCoroutine(SecuenciaAtaqueMelee());
            }
            // Intentar Ataque a Distancia
            else if (enemigosData.Distancia && distanciaAlJugador <= rangoDistancia && distanciaAlJugador > rangoMelee)
            {
                EjecutarAtaqueADistancia();
            }
        }
    }

    private IEnumerator SecuenciaAtaqueMelee()
    {
        estaAtacandoMelee = true;
        tiempoSiguienteAtaque = Time.time + cooldownAtaque;

        if (agente != null && agente.isOnNavMesh)
        {
            agente.isStopped = true;
        }

        ControlarVisualAtaqueClientRpc(true);

        yield return new WaitForSeconds(duracionSenalVisual);

        ControlarVisualAtaqueClientRpc(false);

        if (jugadorObjetivo != null)
        {
            float distanciaActual = Vector3.Distance(transform.position, jugadorObjetivo.position);

            if (distanciaActual <= rangoMelee)
            {
                PlayerStats stats = jugadorObjetivo.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.RecibirDanio(enemigosData.EnemigoAtaque);
                }
            }
        }

        if (agente != null && agente.isOnNavMesh)
        {
            agente.isStopped = false;
        }

        estaAtacandoMelee = false;
    }

    [ClientRpc]
    private void ControlarVisualAtaqueClientRpc(bool activar)
    {
        if (senalVisualGolpe != null)
        {
            senalVisualGolpe.SetActive(activar);
        }
    }

    private void EjecutarAtaqueADistancia()
    {
        if (prefabProyectil == null || puntoDisparo == null) return;

        tiempoSiguienteAtaque = Time.time + cooldownAtaque;

        Vector3 objetivoAjustado = new Vector3(jugadorObjetivo.position.x, puntoDisparo.position.y, jugadorObjetivo.position.z);

        Vector3 direccionHaciaJugador = (objetivoAjustado - puntoDisparo.position).normalized;

        // Evitamos rotaciones extrañas si están en la misma posición vertical: CONSEJO DE GOOGLE
        if (direccionHaciaJugador != Vector3.zero)
        {
            Quaternion rotacionHaciaJugador = Quaternion.LookRotation(direccionHaciaJugador);

            GameObject proyectilInstance = Instantiate(prefabProyectil, puntoDisparo.position, rotacionHaciaJugador);

            if (proyectilInstance.TryGetComponent<ProyectilEnemigo>(out ProyectilEnemigo scriptProyectil))
            {
                scriptProyectil.ConfigurarProyectil(enemigosData.EnemigoAtaque, GetComponent<Collider>());
            }

            if (proyectilInstance.TryGetComponent<NetworkObject>(out NetworkObject netObj))
            {
                netObj.Spawn();
            }
        }
    }

    public void RecibirDanio(int cantidadDanioBase, GameObject jugadorAtacante = null)
    {
        if (!IsServer) return;

        int defensaMultiplicadora = (enemigosData != null) ? enemigosData.EnemigoDefensa : 1;
        int danioFinal = cantidadDanioBase * defensaMultiplicadora;

        vidaActual.Value -= danioFinal;

        if (vidaActual.Value <= 0)
        {
            if (jugadorAtacante != null && enemigosData != null)
            {
                if (jugadorAtacante.TryGetComponent<PlayerStats>(out PlayerStats statsAsesino))
                {
                    int puntosAOtorgar = Mathf.RoundToInt(enemigosData.EnemigoVelocidad);
                    statsAsesino.SumarPuntos(puntosAOtorgar);
                }
            }

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