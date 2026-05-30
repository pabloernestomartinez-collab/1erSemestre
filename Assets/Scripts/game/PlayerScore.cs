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
        puntos.OnValueChanged += AlCambiarPuntos;// Escuchamos el cambio de puntos
        if (GameUIManager.Instance != null)// Forzamos actualización inicial en la UI
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
        if (GameUIManager.Instance != null)// En cuanto la red avisa que cambiaron los puntos, actualizamos el administrador global
        {
            GameUIManager.Instance.ActualizarMarcador(OwnerClientId, valorNuevo);
        }
    }
}