using TMPro; // Necesario si usas TextMeshPro. Si usas el Text común, cambia por using UnityEngine.UI;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class reloj : NetworkBehaviour
{
    [Header("Configuración del Tiempo")]
    [SerializeField] private float tiempoInicialSegundos = 60f; // 1 minuto

    [Header("Componentes de UI")]
    [SerializeField] private TextMeshProUGUI textoReloj; // Arrastra tu componente de Texto aquí en el Inspector

    // Creamos la variable de red que guardará el tiempo restante.
    // - Todos pueden leerla (Everyone)
    // - Solo el Servidor puede modificarla (Server) para evitar que los clientes hagan trampa alterando el tiempo.
    private readonly NetworkVariable<float> tiempoRestante = new NetworkVariable<float>(
        60f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool juegoTerminado = false;

    public override void OnNetworkSpawn()
    {
        // 1. Si somos el Servidor/Host, inicializamos el tiempo del reloj
        if (IsServer)
        {
            tiempoRestante.Value = tiempoInicialSegundos;
        }

        // 2. Mostramos el tiempo inicial inmediatamente en la pantalla
        ActualizarTextoVisual(tiempoRestante.Value);

        // 3. Nos suscribimos al evento: cada vez que el servidor reste tiempo, los clientes actualizarán su pantalla
        tiempoRestante.OnValueChanged += AlCambiarTiempo;
    }

    public override void OnNetworkDespawn()
    {
        // Buena práctica multijugador para evitar errores de memoria al cerrar el juego
        tiempoRestante.OnValueChanged -= AlCambiarTiempo;
    }

    void Update()
    {
        // SÓLO EL SERVIDOR TIENE DERECHO A RESTAR TIEMPO
        if (!IsServer) return;
        if (juegoTerminado) return;

        if (tiempoRestante.Value > 0f)
        {
            // El servidor resta el tiempo del frame actual directamente sobre la variable de red
            tiempoRestante.Value -= Time.deltaTime;
        }
        else
        {
            // El tiempo llegó a cero
            tiempoRestante.Value = 0f;
            juegoTerminado = true;
            TerminarPartidaPorTiempo();
        }
    }

    // Este método se ejecuta automáticamente en las pantallas de TODOS los jugadores cada vez que la variable cambia
    private void AlCambiarTiempo(float valorViejo, float valorNuevo)
    {
        ActualizarTextoVisual(valorNuevo);
    }

    // Convierte los segundos flotantes a un formato estético de reloj de aguja o digital (Minutos:Segundos)
    private void ActualizarTextoVisual(float tiempoEnSegundos)
    {
        if (textoReloj == null) return;

        // Si el tiempo es menor a cero, lo fijamos en cero
        if (tiempoEnSegundos < 0) tiempoEnSegundos = 0;

        // Dividimos los segundos para sacar los minutos y el residuo para los segundos
        int minutos = Mathf.FloorToInt(tiempoEnSegundos / 60f);
        int segundos = Mathf.FloorToInt(tiempoEnSegundos % 60f);

        // Formatea el texto para que siempre muestre dos dígitos (ej: "01:05" o "00:59")
        textoReloj.text = string.Format("{0:00}:{1:00}", minutos, segundos);
    }

    private void TerminarPartidaPorTiempo()
    {
        Debug.Log("¡El tiempo se ha agotado! Aquí puedes llamar al ServerRpc o ClientRpc de Game Over");

        // Ejemplo: Podrías buscar a todos los jugadores y congelarlos, 
        // o disparar la pantalla de Game Over que hicimos en el script 'perdi'
    }
}