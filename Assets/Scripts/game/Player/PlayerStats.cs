using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerStats : NetworkBehaviour
{
    // Usamos NetworkVariable para que Netcode sincronice los datos automáticamente
    public NetworkVariable<int> hierro = new NetworkVariable<int>(0);
    public NetworkVariable<int> madera = new NetworkVariable<int>(0);
    public NetworkVariable<int> fuego = new NetworkVariable<int>(0);
    public NetworkVariable<int> agua = new NetworkVariable<int>(0);
    public NetworkVariable<int> piedra = new NetworkVariable<int>(0);

    // Evento para avisarle a la UI que un valor cambió sin usar el Update
    public Action OnStatsChanged;

    public override void OnNetworkSpawn()
    {
        // Nos suscribimos a los cambios de red de cada variable
        hierro.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        madera.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        fuego.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        agua.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        piedra.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
    }

    // Métodos públicos que SOLO el servidor puede ejecutar para sumar de forma segura
    public void SumarHierro() { if (IsServer) hierro.Value++; }
    public void SumarMadera() { if (IsServer) madera.Value++; }
    public void SumarFuego() { if (IsServer) fuego.Value++; }
    public void SumarAgua() { if (IsServer) piedra.Value++; }
    public void SumarPiedra() { if (IsServer) piedra.Value++; }
}
