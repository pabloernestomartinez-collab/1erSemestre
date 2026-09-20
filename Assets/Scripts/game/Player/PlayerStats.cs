using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerStats : NetworkBehaviour
{
    [Header("Configuración de Vida")]
    [SerializeField] private int vidaMaxima = 100;
    public NetworkVariable<int> vidaActual = new NetworkVariable<int>(100);

    [Header("Configuración de Combate")]
    [SerializeField] private int danioMeleeJugador = 25;
    public int GetDanioMelee() => danioMeleeJugador;

    [Header("Recursos Sincronizados")]
    public NetworkVariable<int> oro = new NetworkVariable<int>(0);
    public NetworkVariable<int> hierba = new NetworkVariable<int>(0);
    public NetworkVariable<int> sabiduria = new NetworkVariable<int>(0);
    public NetworkVariable<int> puntos = new NetworkVariable<int>(0);

    public Action OnStatsChanged;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            vidaActual.Value = vidaMaxima;
        }

        vidaActual.OnValueChanged += (o, n) => OnStatsChanged?.Invoke();
        oro.OnValueChanged += (o, n) => OnStatsChanged?.Invoke();
        hierba.OnValueChanged += (o, n) => OnStatsChanged?.Invoke();
        sabiduria.OnValueChanged += (o, n) => OnStatsChanged?.Invoke();
        puntos.OnValueChanged += (o, n) => OnStatsChanged?.Invoke();

        OnStatsChanged?.Invoke();
    }

    public void RecibirDanio(int cantidadDanio)
    {
        if (!IsServer) return;

        vidaActual.Value -= cantidadDanio;
        Debug.Log($"[SERVIDOR] Jugador {OwnerClientId} recibió {cantidadDanio} de daño. Vida restante: {vidaActual.Value}");

        if (vidaActual.Value <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        vidaActual.Value = vidaMaxima; // Respawn temporal
    }

    // 🔥 SERVER RPC PARA CRAFTEO DE POCIÓN (Solicitado por UICrafteo)
    [ServerRpc]
    public void CraquearPocionServerRpc(int reqHierba, int reqSabiduria)
    {
        if (hierba.Value >= reqHierba && sabiduria.Value >= reqSabiduria)
        {
            hierba.Value -= reqHierba;
            sabiduria.Value -= reqSabiduria;

            // Restaura 50 de vida al jugador asegurando no exceder el máximo
            vidaActual.Value = Mathf.Clamp(vidaActual.Value + 50, 0, vidaMaxima);
            Debug.Log($"🟢 [SERVIDOR] Jugador {OwnerClientId} crafteó una poción con éxito.");
        }
        else
        {
            Debug.LogWarning($"⚠️ [SERVIDOR] Jugador {OwnerClientId} intentó craftear sin suficientes recursos.");
        }
    }

    public void SumarOro(int cantidad = 1) { if (IsServer) oro.Value += cantidad; }
    public void SumarHierba(int cantidad = 1) { if (IsServer) hierba.Value += cantidad; }
    public void SumarSabiduria(int cantidad = 1) { if (IsServer) sabiduria.Value += cantidad; }
    public void SumarPuntos(int cantidad) { if (IsServer) puntos.Value += cantidad; }

    public int GetVidaMaxima() => vidaMaxima;
}