using Unity.Netcode;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    // Creamos una variable de red que guarda un Color
    // Le damos permisos para que cualquiera pueda leer, pero solo el dueño (Owner) pueda cambiarla
    private readonly NetworkVariable<Color> netColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    [SerializeField] private MeshRenderer meshRenderer;

    public override void OnNetworkSpawn()
    {
        // 1. Si soy el dueño del objeto, elijo un color al azar
        if (IsOwner)
        {
            netColor.Value = new Color(Random.value, Random.value, Random.value);
        }

        // 2. Aplicamos el color inicial
        ApplyColor(netColor.Value);

        // 3. Nos suscribimos al cambio para que si el color cambia en el futuro, se actualice
        netColor.OnValueChanged += OnColorChanged;
    }

    private void OnColorChanged(Color previousValue, Color newValue)
    {
        ApplyColor(newValue);
    }

    private void ApplyColor(Color color)
    {
        meshRenderer.material.color = color;
    }

    public override void OnNetworkDespawn()
    {
        // Es buena práctica desuscribirse al destruir el objeto
        netColor.OnValueChanged -= OnColorChanged;
    }
}