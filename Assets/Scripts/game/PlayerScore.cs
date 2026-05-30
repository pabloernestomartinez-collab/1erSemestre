using Unity.Netcode;
using UnityEngine;

public class PlayerScore : NetworkBehaviour
{
    // Variable de red sincronizada
    public readonly NetworkVariable<int> puntos = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        // Escuchamos el cambio de puntos
        puntos.OnValueChanged += AlCambiarPuntos;

        // Forzamos actualización inicial en la UI
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ActualizarMarcador(OwnerClientId, puntos.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        puntos.OnValueChanged -= AlCambiarPuntos;
    }

    public void AddPoints(int cantidad)
    {
        if (!IsServer) return;
        puntos.Value += cantidad;
    }

    private void AlCambiarPuntos(int valorViejo, int valorNuevo)
    {
        // En cuanto la red avisa que cambiaron los puntos, actualizamos el administrador global
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ActualizarMarcador(OwnerClientId, valorNuevo);
        }
    }
}