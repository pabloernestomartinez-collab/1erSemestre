using Unity.Netcode;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    [Header("Paleta de Colores")]
    [SerializeField] private Color colorParaHost = Color.blue;   // Color que usará el ID 0
    [SerializeField] private Color colorParaCliente = Color.red; // Color que usará el ID 1

    [SerializeField] private MeshRenderer meshRenderer; // Referencia al componente visual del player

    private readonly NetworkVariable<Color> netColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );    // solo el servidor escribe


    public override void OnNetworkSpawn()
    {
        netColor.OnValueChanged += OnColorChanged;        // Suscribimos el evento para enterarnos cuando el color cambie en la red


        if (IsServer)
        {
            if (OwnerClientId == 0)            // Evaluamos el ID real del dueño de este personaje

            {
                netColor.Value = colorParaHost;
            }
            else
            {
                netColor.Value = colorParaCliente;
            }
        }

        ApplyColor(netColor.Value);        // Aplicamos el color inicial que ya tenga la variable de red

    }

    public override void OnNetworkDespawn()
    {
        netColor.OnValueChanged -= OnColorChanged;        // Buena práctica: desvincularse al destruir el personaje

    }

    private void OnColorChanged(Color previousValue, Color newValue)
    {
        ApplyColor(newValue);
    }

    private void ApplyColor(Color color)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = color;            // Accedemos al material del objeto y cambiamos su color base por el color de red

        }
    }
}