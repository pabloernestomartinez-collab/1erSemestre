using Unity.Netcode;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    [Header("Paleta de Colores")]
    [SerializeField] private Color colorParaHost = Color.blue;   // Color que usará el ID 0
    [SerializeField] private Color colorParaCliente = Color.red; // Color que usará el ID 1

    [Header("Referencia Visual")]
    [SerializeField] private Renderer playerRenderer; // Acepta tanto MeshRenderer como SkinnedMeshRenderer

    private readonly NetworkVariable<Color> netColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        // Si no se asignó en el Inspector, se busca automáticamente en el objeto o sus hijos
        if (playerRenderer == null)
        {
            playerRenderer = GetComponentInChildren<Renderer>();
        }

        netColor.OnValueChanged += OnColorChanged;

        if (IsServer)
        {
            if (OwnerClientId == 0)
            {
                netColor.Value = colorParaHost;
            }
            else
            {
                netColor.Value = colorParaCliente;
            }
        }

        ApplyColor(netColor.Value);
    }

    public override void OnNetworkDespawn()
    {
        netColor.OnValueChanged -= OnColorChanged;
    }

    private void OnColorChanged(Color previousValue, Color newValue)
    {
        ApplyColor(newValue);
    }

    private void ApplyColor(Color color)
    {
        if (playerRenderer != null)
        {
            // Modificamos el color en el material de la malla
            playerRenderer.material.color = color;
        }
    }
}