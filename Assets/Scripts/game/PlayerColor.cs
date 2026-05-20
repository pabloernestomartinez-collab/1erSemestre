using Unity.Netcode; // Requerido para usar NetworkBehaviour, NetworkVariable y los métodos de red
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    // --- LA VARIABLE DE RED (Sincronización Automática) ---
    // Definimos una variable que Netcode vigilará constantemente. 
    // - Inicializa en Blanco (Color.white).
    // - 'ReadPermission.Everyone': Todos las pantallas (Host y Clientes) pueden ver el color.
    // - 'WritePermission.Owner': Le da permiso al dueño del personaje de cambiar su propio color (Autoridad de Cliente).
    private readonly NetworkVariable<Color> netColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    [SerializeField] private MeshRenderer meshRenderer; // Referencia al componente visual del personaje (cubo, cápsula, etc.)

    // Se ejecuta automáticamente cuando el personaje es creado e introducido en la red de Netcode
    public override void OnNetworkSpawn()
    {
        // 1. FILTRO DE DUEÑO: Solo la computadora que controla a este personaje en específico elegirá el color.
        if (IsOwner)
        {
            // Generamos un color aleatorio (valores RGB al azar entre 0.0 y 1.0)
            // Al cambiar el '.Value' siendo el dueño, Netcode empaqueta este dato y lo envía automáticamente a todos los demás jugadores.
            netColor.Value = new Color(Random.value, Random.value, Random.value);
        }

        // 2. APLICACIÓN INICIAL: Como los clientes que se conectan tarde (o el Host) necesitan ver el color actual,
        // leemos directamente el valor que tenga la variable de red en este instante.
        ApplyColor(netColor.Value);

        // 3. SUSCRIPCIÓN AL EVENTO: Le decimos a Netcode que "escuche". 
        // Cada vez que la variable 'netColor' cambie de valor en la red, disparará de forma automática la función 'OnColorChanged'.
        netColor.OnValueChanged += OnColorChanged;
    }

    // Este método se ejecuta automáticamente CADA VEZ que el color cambia en la red
    // Recibe como parámetros el color viejo (previousValue) y el color nuevo (newValue)
    private void OnColorChanged(Color previousValue, Color newValue)
    {
        // Tomamos el nuevo valor que llegó de la red y lo mandamos a pintar en el modelo
        ApplyColor(newValue);
    }

    // Método auxiliar simple de Unity encargado de interactuar con el motor gráfico
    private void ApplyColor(Color color)
    {
        if (meshRenderer != null)
        {
            // Accedemos al material del objeto y cambiamos su color base por el color de red
            meshRenderer.material.color = color;
        }
    }

    // Se ejecuta automáticamente cuando el personaje desaparece de la red (por ejemplo, al desconectarse o morir)
    public override void OnNetworkDespawn()
    {
        // BUENA PRÁCTICA MULTIJUGADOR: Rompemos el enlace del evento. 
        // Si no nos desuscribimos aquí, Unity intentará mandar datos a un objeto que ya no existe, provocando errores graves de rendimiento.
        netColor.OnValueChanged -= OnColorChanged;
    }
}