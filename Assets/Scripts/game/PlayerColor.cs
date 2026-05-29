using Unity.Netcode; 
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    private readonly NetworkVariable<Color> netColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    [SerializeField] private MeshRenderer meshRenderer; // Referencia al componente visual del player

    public override void OnNetworkSpawn()    // Se ejecuta automáticamente cuando el personaje es creado e introducido en la red de Netcode

    {
        if (IsOwner)
        {
            netColor.Value = new Color(Random.value, Random.value, Random.value);
        }

        ApplyColor(netColor.Value);

        netColor.OnValueChanged += OnColorChanged;
    }

    private void OnColorChanged(Color previousValue, Color newValue)
    {
        ApplyColor(newValue);
    }

    private void ApplyColor(Color color) // google.....
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = color;  // Accedemos al material del objeto y cambiamos su color base por el color de red

        }
    }

  
}