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
    private bool estaMuerto = false;          // 🔥 Candado para evitar doble procesamiento de muerte

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

        // NavMesh solo en el Servidor.
        if (!IsServer && agente != null)
        {
            agente.enabled = false;
        }
    }

    void Update()
    {
        if (!IsServer || estaMuerto) return;

        if (jugadorObjetivo == null || agente == null || !agente.enabled || !agente.isOnNavMesh) return;

        // Si está preparando el golpe melee, detenemos el movimiento
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

        if (jugadorObjetivo != null && !estaMuerto)
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
        if (!IsServer || estaMuerto) return;

        int defensaMultiplicadora = (enemigosData != null) ? enemigosData.EnemigoDefensa : 1;
        int danioFinal = cantidadDanioBase * defensaMultiplicadora;

        vidaActual.Value -= danioFinal;

        // 🔥 Verificación de Muerte
        if (vidaActual.Value <= 0)
        {
            estaMuerto = true;
            ProcesarMuerte(jugadorAtacante);
        }
    }

    private void ProcesarMuerte(GameObject jugadorAtacante)
    {
        // 1. Otorgar Puntos al Asesino
        if (jugadorAtacante != null)
        {
            if (jugadorAtacante.TryGetComponent<PlayerStats>(out PlayerStats statsAsesino))
            {
                // Si enemigosData tiene 'EnemigoPuntos' usa ese valor; de lo contrario calcula con Velocidad
                int puntosAOtorgar = (enemigosData != null) ? Mathf.RoundToInt(enemigosData.EnemigoVelocidad * 10f) : 50;
                statsAsesino.SumarPuntos(puntosAOtorgar);

                Debug.Log($"💀 Enemigo eliminado. Jugador {statsAsesino.OwnerClientId} recibió {puntosAOtorgar} puntos.");
            }
        }

        // 2. Detener NavMeshAgent para evitar errores
        if (agente != null && agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.enabled = false;
        }

        // 3. Destruir en la red
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || estaMuerto) return;

        if (other.CompareTag("Player") && jugadorObjetivo == null)
        {
            jugadorObjetivo = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer || estaMuerto) return;

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