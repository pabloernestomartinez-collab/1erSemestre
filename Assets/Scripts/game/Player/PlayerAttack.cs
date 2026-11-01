using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : NetworkBehaviour
{
    [Header("Configuración del Ataque")]
    [SerializeField] private float rangoAtaque = 2.5f;     // Qué tan lejos llega el golpe
    [SerializeField] private float cooldownAtaque = 0.6f;   // Tiempo entre ataques
    [SerializeField] private Transform puntoAtaque;         // Objeto vacío al frente del jugador

    [Header("Parámetro del Animator")]
    [SerializeField] private string nombreBoolAtaque = "IsAttack"; // Nombre exacto de tu variable bool en el Animator

    [Header("Señal Visual de Ataque")]
    [SerializeField] private GameObject senalVisualGolpe;
    [SerializeField] private float duracionSenalVisual = 0.15f;

    private float tiempoSiguienteAtaque = 0f;
    private PlayerStats misStats;
    private Animator animator;

    public override void OnNetworkSpawn()
    {
        misStats = GetComponent<PlayerStats>();

        // Busca el Animator en este objeto o en los objetos hijos (Mesh)
        animator = GetComponentInChildren<Animator>();

        if (senalVisualGolpe != null)
        {
            senalVisualGolpe.SetActive(false);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && Time.time >= tiempoSiguienteAtaque)
        {
            tiempoSiguienteAtaque = Time.time + cooldownAtaque;

            int danioAInfligir = misStats != null ? misStats.GetDanioMelee() : 10;

            // 1. Activar animación de inmediato en el cliente local para respuesta instantánea
            ActivarAnimacionAtaque(true);

            // 2. Solicitar al servidor aplicar daño y sincronizar con los demás jugadores
            SolicitarAtaqueServerRpc(puntoAtaque != null ? puntoAtaque.position : transform.position, danioAInfligir);
        }
    }

    private void ActivarAnimacionAtaque(bool estado)
    {
        if (animator != null)
        {
            animator.SetBool(nombreBoolAtaque, estado);
        }
    }

    [ServerRpc]
    private void SolicitarAtaqueServerRpc(Vector3 posicionDelGolpe, int danio)
    {
        // Notificar a los demás clientes que reproduzcan la animación
        ReproducirAnimacionAtaqueClientRpc(true);

        ControlarVisualAtaqueClientRpc(true);

        Collider[] enemigosGolpeados = Physics.OverlapSphere(posicionDelGolpe, rangoAtaque);
        HashSet<enemy> enemigosProcesados = new HashSet<enemy>();

        foreach (Collider col in enemigosGolpeados)
        {
            if (col.CompareTag("Enemy"))
            {
                enemy scriptEnemigo = col.GetComponentInParent<enemy>();

                if (scriptEnemigo != null && !enemigosProcesados.Contains(scriptEnemigo))
                {
                    enemigosProcesados.Add(scriptEnemigo);
                    scriptEnemigo.RecibirDanio(danio, gameObject);
                }
            }
        }

        StartCoroutine(ApagarEfectosYAtaque());
    }

    [ClientRpc]
    private void ReproducirAnimacionAtaqueClientRpc(bool activar)
    {
        // El dueño ya activó la animación localmente en Update()
        if (IsOwner) return;

        ActivarAnimacionAtaque(activar);
    }

    private IEnumerator ApagarEfectosYAtaque()
    {
        yield return new WaitForSeconds(duracionSenalVisual);
        ControlarVisualAtaqueClientRpc(false);

        // Esperar el resto del cooldown antes de apagar el booleano
        float tiempoRestante = cooldownAtaque - duracionSenalVisual;
        if (tiempoRestante > 0)
        {
            yield return new WaitForSeconds(tiempoRestante);
        }

        // Apagar el bool de ataque en todos los clientes
        ResetearBoolAtaqueClientRpc();
    }

    [ClientRpc]
    private void ResetearBoolAtaqueClientRpc()
    {
        ActivarAnimacionAtaque(false);
    }

    [ClientRpc]
    private void ControlarVisualAtaqueClientRpc(bool activar)
    {
        if (senalVisualGolpe != null)
        {
            senalVisualGolpe.SetActive(activar);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (puntoAtaque == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoAtaque.position, rangoAtaque);
    }
}