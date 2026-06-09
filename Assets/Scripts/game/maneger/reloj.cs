using TMPro;
using Unity.Netcode;
using UnityEngine;

public class reloj : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI textoReloj;

    // Seteamos los 600 segundos (10 minutos) directo en el código para evitar el inspector
    private readonly NetworkVariable<float> tiempoRestante = new NetworkVariable<float>(
        600f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool juegoTerminado = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            tiempoRestante.Value = 600f;
        }

        // Mostramos el formato correcto desde el primer segundo
        ActualizarTextoVisual(tiempoRestante.Value);
        tiempoRestante.OnValueChanged += AlCambiarTiempo;
    }

    public override void OnNetworkDespawn()
    {
        tiempoRestante.OnValueChanged -= AlCambiarTiempo;
    }

    void Update()
    {
        if (!IsServer) return;
        if (juegoTerminado) return;

        if (tiempoRestante.Value > 0f)
        {
            tiempoRestante.Value -= Time.deltaTime;
        }
        else
        {
            tiempoRestante.Value = 0f;
            juegoTerminado = true;

            // Llama a la función con el sufijo Rpc obligatorio
            EjecutarFinPartidaRpc();
        }
    }

    private void AlCambiarTiempo(float valorViejo, float valorNuevo)
    {
        ActualizarTextoVisual(valorNuevo);
    }

    // LA FUNCIÓN DE CONVERSIÓN VOLVIÓ (Totalmente limpia y segura)
    private void ActualizarTextoVisual(float tiempoEnSegundos)
    {
        if (textoReloj == null) return;
        if (tiempoEnSegundos < 0) tiempoEnSegundos = 0;

        // Pasamos los segundos brutos a Minutos y Segundos enteros
        int minutos = Mathf.FloorToInt(tiempoEnSegundos / 60f);
        int segundos = Mathf.FloorToInt(tiempoEnSegundos % 60f);

        // Formateamos el texto para que siempre muestre dos dígitos (ej: 09:05 en vez de 9:5)
        textoReloj.text = string.Format("{0:00}:{1:00}", minutos, segundos);
    }

    [Rpc(SendTo.Everyone)]
    private void EjecutarFinPartidaRpc()
    {
        GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject jugador in jugadores)
        {
            if (jugador.TryGetComponent<PlayerMovement>(out PlayerMovement movimiento))
            {
                movimiento.enabled = false;
            }
            if (jugador.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }
    }
}