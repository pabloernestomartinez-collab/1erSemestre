using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerStats : NetworkBehaviour
{
    // Usamos NetworkVariable para que Netcode sincronice los datos automáticamente
    public NetworkVariable<int> espadas = new NetworkVariable<int>(0);
    public NetworkVariable<int> escudos = new NetworkVariable<int>(0);
    public NetworkVariable<int> coleccionable222 = new NetworkVariable<int>(0);
    public NetworkVariable<int> coleccionable224 = new NetworkVariable<int>(0);

    // Evento para avisarle a la UI que un valor cambió sin usar el Update
    public Action OnStatsChanged;

    public override void OnNetworkSpawn()
    {
        // Nos suscribimos a los cambios de red de cada variable
        espadas.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        escudos.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        coleccionable222.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        coleccionable224.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
    }

    // Métodos públicos que SOLO el servidor puede ejecutar para sumar de forma segura
    public void SumarEspada() { if (IsServer) espadas.Value++; }
    public void SumarEscudo() { if (IsServer) escudos.Value++; }
    public void Sumar222() { if (IsServer) coleccionable222.Value++; }
    public void Sumar224() { if (IsServer) coleccionable224.Value++; }
}
