using TMPro;
using Unity.Netcode;
using UnityEngine;

public class reloj : NetworkBehaviour
{
    [Header("Configuración del Tiempo")]
    // 1. Cambiamos el valor por defecto a 600f (10 minutos)
    [SerializeField] private float tiempoInicialSegundos = 600f;

    [Header("Componentes de UI")]
    [SerializeField] private TextMeshProUGUI textoReloj;

    // 2. Cambiamos también el valor inicial de la NetworkVariable a 600f
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
            tiempoRestante.Value = tiempoInicialSegundos;
        }
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
            TerminarPartidaPorTiempo(); // <- Se ejecuta solo en el Servidor
        }
    }
    private void AlCambiarTiempo(float valorViejo, float valorNuevo)
    {
        ActualizarTextoVisual(valorNuevo);
    }
    private void ActualizarTextoVisual(float tiempoEnSegundos)
    {
        if (textoReloj == null) return;
        if (tiempoEnSegundos < 0) tiempoEnSegundos = 0;
        int minutos = Mathf.FloorToInt(tiempoEnSegundos / 60f);// google...
        int segundos = Mathf.FloorToInt(tiempoEnSegundos % 60f);
        textoReloj.text = string.Format("{0:00}:{1:00}", minutos, segundos);
    }
    private void TerminarPartidaPorTiempo()
    {
        //Debug.Log("¡El tiempo se ha agotado en el Servidor! Congelando jugadores...");
        CongelarTodosLosJugadoresRpc();// El servidor da una orden masiva que viajará a las pantallas de TODOS los jugadores
    }
    [Rpc(SendTo.Everyone)]
    private void CongelarTodosLosJugadoresRpc()
    {
        GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");// Buscamos en la escena local a TODOS los objetos que tengan la etiqueta "Player"
        foreach (GameObject jugador in jugadores)
        {
            if (jugador.TryGetComponent<PlayerMovement>(out PlayerMovement movimiento))//  Apagamos su script de movimiento para bloquear el teclado/mando
            {
                movimiento.enabled = false;
            }
            if (jugador.TryGetComponent<Rigidbody>(out Rigidbody rb))// Frenamos en seco sus físicas para que no se sigan deslizando por inercia
            {
                rb.linearVelocity = Vector3.zero; // google
                rb.angularVelocity = Vector3.zero; // Evita que se quede rotando sobre sí mismo
                rb.isKinematic = true; // Lo vuelve una "estatua" para que la gravedad no lo mueva
            }
        }
    }
}