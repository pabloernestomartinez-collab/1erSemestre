using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class enemy : NetworkBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] private enemigosData enemigosData;

    [Header("Tipo de Enemigo")]
    [SerializeField] private bool esBoss = false; // Marcar en TRUE solo en el Prefab del Boss Final

    [Header("Vida del Enemigo (Netcode)")]
    [SerializeField] private int vidaMaxima = 100;
    public NetworkVariable<int> vidaActual = new NetworkVariable<int>(100);

    [Header("Animación de Muerte (Caída)")]
    [SerializeField] private float duracionCaida = 0.4f;               // Tiempo que tarda en caer 90° de costado
    [SerializeField] private float tiempoEnElSueloAntesDespawn = 0.8f; // Tiempo que queda tirado antes de desaparecer

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
    private Transform jugadorObjetivo = null;
    private float tiempoSiguienteAtaque = 0f;
    private bool estaAtacandoMelee = false;
    private bool estaMuerto = false;

    public int GetVidaMaxima() => vidaMaxima;

    public override void OnNetworkSpawn()
    {
        agente = GetComponent<NavMeshAgent>();

        if (senalVisualGolpe != null)
        {
            senalVisualGolpe.SetActive(false);
        }

        if (agente != null && enemigosData != null)
        {
            agente.speed = enemigosData.EnemigoVelocidad;
        }

        if (IsServer)
        {
            vidaActual.Value = vidaMaxima;
        }

        if (!IsServer && agente != null)
        {
            agente.enabled = false;
        }
    }

    void Update()
    {
        if (!IsServer || estaMuerto) return;

        if (jugadorObjetivo == null || agente == null || !agente.enabled || !agente.isOnNavMesh) return;

        if (estaAtacandoMelee) return;

        agente.SetDestination(jugadorObjetivo.position);

        float distanciaAlJugador = Vector3.Distance(transform.position, jugadorObjetivo.position);

        if (Time.time >= tiempoSiguienteAtaque)
        {
            if (enemigosData.Emelee && distanciaAlJugador <= rangoMelee)
            {
                StartCoroutine(SecuenciaAtaqueMelee());
            }
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

        if (agente != null && agente.isOnNavMesh && !estaMuerto)
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

        if (vidaActual.Value <= 0)
        {
            estaMuerto = true;
            ProcesarMuerte(jugadorAtacante);
        }
    }

    private void ProcesarMuerte(GameObject jugadorAtacante)
    {
        // Otorgar Puntos al Asesino
        if (jugadorAtacante != null)
        {
            if (jugadorAtacante.TryGetComponent<PlayerStats>(out PlayerStats statsAsesino))
            {
                int puntosAOtorgar = (enemigosData != null) ? Mathf.RoundToInt(enemigosData.EnemigoVelocidad * 10f) : 50;
                statsAsesino.SumarPuntos(puntosAOtorgar);
            }
        }

        if (esBoss)
        {
            NotificarVictoriaClientRpc();
        }

        // Detener NavMeshAgent para liberar la rotación del objeto
        if (agente != null)
        {
            if (agente.isOnNavMesh) agente.isStopped = true;
            agente.enabled = false;
        }

        // Iniciar la secuencia de caída de 90° antes de hacer Despawn
        StartCoroutine(SecuenciaCaidaYDespawnServidor());
    }

    private IEnumerator SecuenciaCaidaYDespawnServidor()
    {
        // Avisar a todos los clientes (incluido el Host) que reproduzcan la caída visual
        EjecutarCaidaMuerteClientRpc();

        // Esperar el tiempo de caída + el tiempo en el suelo
        yield return new WaitForSeconds(duracionCaida + tiempoEnElSueloAntesDespawn);

        // Destruir en la red
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

    [ClientRpc]
    private void EjecutarCaidaMuerteClientRpc()
    {
        // Desactivar colliders para que el cadáver no bloquee al jugador ni reciba más golpes
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // Apagar señal de ataque si había quedado encendida al morir
        if (senalVisualGolpe != null)
        {
            senalVisualGolpe.SetActive(false);
        }

        // Si tiene Animator, lo pausamos para que la animación de caminar no pelee contra la rotación de caída
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.enabled = false;
        }

        StartCoroutine(AnimarRotacionCaida());
    }

    private IEnumerator AnimarRotacionCaida()
    {
        Quaternion rotacionInicial = transform.rotation;
        // Rotar 90 grados en el eje Z local (caída de costado)
        Quaternion rotacionFinal = rotacionInicial * Quaternion.Euler(0f, 0f, 90f);

        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracionCaida)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = Mathf.Clamp01(tiempoTranscurrido / duracionCaida);

            // Curva acelerada al final para simular gravedad al caer
            float curvaCaida = progreso * progreso;
            transform.rotation = Quaternion.Slerp(rotacionInicial, rotacionFinal, curvaCaida);

            yield return null;
        }

        transform.rotation = rotacionFinal;
    }

    [ClientRpc]
    private void NotificarVictoriaClientRpc()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient?.PlayerObject != null)
        {
            var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (playerObj.TryGetComponent<MovimientoPlayer>(out var mov)) mov.enabled = false;
            if (playerObj.TryGetComponent<PlayerAttack>(out var atk)) atk.enabled = false;
        }

        if (GameHUDManager.Instance != null)
        {
            GameHUDManager.Instance.MostrarPantallaVictoria();
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