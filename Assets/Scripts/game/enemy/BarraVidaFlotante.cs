using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BarraVidaFlotante : NetworkBehaviour
{
    [Header("Referencias de la UI Flotante")]
    [SerializeField] private Slider sliderVida;

    private enemy scriptEnemigo;
    private Camera camaraPrincipal;

    private void Awake()
    {
        scriptEnemigo = GetComponent<enemy>();

        // Si no está asignado en el Inspector, buscar Slider en los componentes hijos
        if (sliderVida == null)
        {
            sliderVida = GetComponentInChildren<Slider>();
        }
    }

    public override void OnNetworkSpawn()
    {
        camaraPrincipal = Camera.main;

        if (scriptEnemigo != null)
        {
            // Establecer vida máxima
            if (sliderVida != null)
            {
                sliderVida.maxValue = scriptEnemigo.GetVidaMaxima();
                sliderVida.value = scriptEnemigo.vidaActual.Value;
            }

            // Suscribirse al cambio de la NetworkVariable de la vida del enemigo
            scriptEnemigo.vidaActual.OnValueChanged += OnVidaChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (scriptEnemigo != null)
        {
            scriptEnemigo.vidaActual.OnValueChanged -= OnVidaChanged;
        }
    }

    private void OnVidaChanged(int vidaAnterior, int vidaNueva)
    {
        if (sliderVida != null)
        {
            sliderVida.value = vidaNueva;
        }
    }

    private void LateUpdate()
    {
        // Rotar el Slider hacia la cámara (Billboard)
        if (sliderVida != null && camaraPrincipal != null)
        {
            sliderVida.transform.rotation = camaraPrincipal.transform.rotation;
        }
    }
}