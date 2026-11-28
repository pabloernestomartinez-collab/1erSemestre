using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : NetworkBehaviour
{
    [Header("Configuración del Ataque")]
    [SerializeField] private float rangoAtaque = 2.5f;
    [SerializeField] private float cooldownAtaque = 0.6f;
    [SerializeField] private Transform puntoAtaque;

    [Header("Giro de 360° al Atacar")]
    [SerializeField] private Transform modeloVisual;          // Opcional: asigna el hijo del modelo 3D (si queda vacío usa el Animator)
    [SerializeField] private float duracionGiro360 = 0.5f;
    [SerializeField] private bool girarHaciaLaDerecha = true;

    [Header("Parámetro del Animator")]
    [SerializeField] private string nombreBoolAtaque = "IsAttack";

    [Header("Señal Visual de Ataque")]
    [SerializeField] private GameObject senalVisualGolpe;
    [SerializeField] private float duracionSenalVisual = 0.15f;

    private float tiempoSiguienteAtaque = 0f;
    private PlayerStats misStats;
    private Animator animator;
    private Coroutine corrutinaGiro;

    public bool EstaGirando => corrutinaGiro != null;

    public override void OnNetworkSpawn()
    {
        misStats = GetComponent<PlayerStats>();
        animator = GetComponentInChildren<Animator>();

        // Si no se asignó manualmente en el Inspector, usa el transform del modelo animado
        if (modeloVisual == null && animator != null)
        {
            modeloVisual = animator.transform;
        }

        if (senalVisualGolpe != null)
        {
            senalVisualGolpe.SetActive(false);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        bool presionoEspacio = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool presionoClicDerecho = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;

        if ((presionoEspacio || presionoClicDerecho) && Time.time >= tiempoSiguienteAtaque)
        {
            EjecutarAtaque();
        }
    }

    private void EjecutarAtaque()
    {
        tiempoSiguienteAtaque = Time.time + cooldownAtaque;

        int danioAInfligir = misStats != null ? misStats.GetDanioMelee() : 10;

        ActivarAnimacionAtaque(true);
        IniciarGiro360();

        SolicitarAtaqueServerRpc(puntoAtaque != null ? puntoAtaque.position : transform.position, danioAInfligir);
    }

    private void IniciarGiro360()
    {
        if (modeloVisual == null) return;

        if (corrutinaGiro != null)
        {
            StopCoroutine(corrutinaGiro);
            modeloVisual.localRotation = Quaternion.identity; // Asegura que vuelva a su frente original
        }
        corrutinaGiro = StartCoroutine(Girar360DuranteAtaque());
    }

    private IEnumerator Girar360DuranteAtaque()
    {
        float tiempoTranscurrido = 0f;
        float gradosTotales = girarHaciaLaDerecha ? 360f : -360f;
        float velocidadAngular = gradosTotales / duracionGiro360;

        while (tiempoTranscurrido < duracionGiro360)
        {
            float delta = Time.deltaTime;
            if (tiempoTranscurrido + delta > duracionGiro360)
            {
                delta = duracionGiro360 - tiempoTranscurrido;
            }

            // Rota únicamente el modelo hijo en su eje local Y sin afectar la raíz ni la cámara
            modeloVisual.Rotate(Vector3.up, velocidadAngular * delta, Space.Self);

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        // Restaura exactamente la orientación frontal original al terminar la vuelta
        modeloVisual.localRotation = Quaternion.identity;
        corrutinaGiro = null;
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
        if (IsOwner) return;

        ActivarAnimacionAtaque(activar);
        if (activar)
        {
            IniciarGiro360();
        }
    }

    private IEnumerator ApagarEfectosYAtaque()
    {
        yield return new WaitForSeconds(duracionSenalVisual);
        ControlarVisualAtaqueClientRpc(false);

        float tiempoRestante = cooldownAtaque - duracionSenalVisual;
        if (tiempoRestante > 0)
        {
            yield return new WaitForSeconds(tiempoRestante);
        }

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