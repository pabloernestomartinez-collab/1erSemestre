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
    [SerializeField] private float duracionSenalVisual = 0.2f; // Cuánto tiempo se queda prendido en pantalla

    [Header("Rangos de Ataque (Opcionales para ajustar)")]
    [SerializeField] private float rangoMelee = 2.2f;
    [SerializeField] private float rangoDistancia = 15f;
    [SerializeField] private float cooldownAtaque = 1.5f; // Tiempo de espera entre golpes

    private NavMeshAgent agente;
    private Transform jugadorObjetivo = null; // Guarda al jugador que está persiguiendo
    private float tiempoSiguienteAtaque = 0f;

    public override void OnNetworkSpawn()
    {
        agente = GetComponent<NavMeshAgent>();

        // Aseguramos que la señal visual empiece apagada en todos lados
        if (senalVisualGolpe != null)
        {
            senalVisualGolpe.SetActive(false);
        }

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

        if (jugadorObjetivo == null || agente == null || !agente.enabled || !agente.isOnNavMesh) return;

        agente.SetDestination(jugadorObjetivo.position); // Persecución

        float distanciaAlJugador = Vector3.Distance(transform.position, jugadorObjetivo.position);

        if (Time.time >= tiempoSiguienteAtaque)
        {
            // Intentar Ataque Cuerpo a Cuerpo (Melee)
            if (enemigosData.Emelee && distanciaAlJugador <= rangoMelee)
            {
                EjecutarAtaqueMelee();
                Debug.Log("ataque melee");
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

        ControlarVisualAtaqueClientRpc(true);

        PlayerStats stats = jugadorObjetivo.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.RecibirDanio(enemigosData.EnemigoAtaque);
        }

        StartCoroutine(ApagarSenalVisualDespuesDeTiempo());
    }

    // Corrutina en el servidor para esperar y enviar la orden de apagado
    private IEnumerator ApagarSenalVisualDespuesDeTiempo()
    {
        yield return new WaitForSeconds(duracionSenalVisual);
        ControlarVisualAtaqueClientRpc(false); // 🔥 Le avisa a todos que se apague
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
        Vector3 direccionHaciaJugador = (jugadorObjetivo.position - puntoDisparo.position).normalized;
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

    // 🔥 CONTROLADO Y ACTUALIZADO: Agregamos el parámetro 'jugadorAtacante' con valor por defecto null
    public void RecibirDanio(int cantidadDanioBase, GameObject jugadorAtacante = null)
    {
        if (!IsServer) return;

        int defensaMultiplicadora = (enemigosData != null) ? enemigosData.EnemigoDefensa : 1;
        int danioFinal = cantidadDanioBase * defensaMultiplicadora;

        vidaActual.Value -= danioFinal;

        if (vidaActual.Value <= 0)
        {
            // 🔥 ASIGNACIÓN DE PUNTOS: Si el enemigo muere y conocemos al atacante, le damos sus puntos
            if (jugadorAtacante != null && enemigosData != null)
            {
                if (jugadorAtacante.TryGetComponent<PlayerStats>(out PlayerStats statsAsesino))
                {
                    // Redondeamos el float de EnemigoVelocidad al entero más cercano
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