using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    // Delegado para notificar cambios a la UI (GameHUDManager)
    public Action OnStatsChanged;

    [Header("Estadísticas del Jugador (Sincronizadas)")]
    public NetworkVariable<int> puntosVida = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> puntosVidaMax = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> oro = new NetworkVariable<int>(50, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> hierba = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> sabiduria = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> danioMeleeJugador = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Posesión de Armas (Booleanos)")]
    public NetworkVariable<bool> tieneEspada = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> tieneDaga = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Cantidad de Armas Compradas")]
    public NetworkVariable<int> cantidadEspadas = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> cantidadDagas = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // --- Propiedades de Compatibilidad Directa con GameHUDManager ---
    public NetworkVariable<int> vidaActual => puntosVida;
    public NetworkVariable<int> puntos => sabiduria;

    public override void OnNetworkSpawn()
    {
        puntosVida.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        puntosVidaMax.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        oro.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        hierba.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        sabiduria.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        danioMeleeJugador.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        tieneEspada.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        tieneDaga.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        puntosVida.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        puntosVidaMax.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        oro.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        hierba.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        sabiduria.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        danioMeleeJugador.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        tieneEspada.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        tieneDaga.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
    }

    // --- Métodos Getters para lectura desde UI ---
    public int GetVidaActual() => puntosVida.Value;
    public int GetVidaMaxima() => puntosVidaMax.Value;
    public int GetOro() => oro.Value;
    public int GetHierba() => hierba.Value;
    public int GetSabiduria() => sabiduria.Value;
    public int GetDanioMelee() => danioMeleeJugador.Value;

    // --- Métodos Requeridos por Enemigos y Proyectiles ---
    public void RecibirDanio(int danio)
    {
        if (!IsServer) return;

        puntosVida.Value = Mathf.Max(0, puntosVida.Value - danio);
        //Debug.Log($"[JUGADOR {OwnerClientId}] Recibió {danio} de daño. Vida restante: {puntosVida.Value}");
    }

    public void SumarPuntos(int cantidad)
    {
        if (!IsServer) return;
        sabiduria.Value += cantidad;
    }

    // --- Métodos de Modificación en Servidor ---
    public void SumarOro(int cantidad)
    {
        if (!IsServer) return;
        oro.Value += cantidad;
    }

    public void SumarHierba(int cantidad)
    {
        if (!IsServer) return;
        hierba.Value += cantidad;
    }

    public void SumarSabiduria(int cantidad)
    {
        if (!IsServer) return;
        sabiduria.Value += cantidad;
    }

    // --- RPCs de Compra y Crafteo ---
    [ServerRpc]
    public void CraftearPocionServerRpc(int costoHierba, int costoSabiduria, int curacionHP)
    {
        if (hierba.Value >= costoHierba && sabiduria.Value >= costoSabiduria)
        {
            hierba.Value -= costoHierba;
            sabiduria.Value -= costoSabiduria;
            puntosVida.Value = Mathf.Min(puntosVidaMax.Value, puntosVida.Value + curacionHP);

            //Debug.Log($"[SERVIDOR] Jugador {OwnerClientId} crafteó poción (-{costoHierba} Hierba, -{costoSabiduria} Sabiduría, +{curacionHP} HP).");
        }
    }

    [ServerRpc]
    public void ComprarArmaServerRpc(int precioOro, int danioExtra)
    {
        if (oro.Value >= precioOro)
        {
            oro.Value -= precioOro;
            danioMeleeJugador.Value += danioExtra;
        }
    }

    [ServerRpc]
    public void ComprarArmaEspecificaServerRpc(string tipoArma, int precioOro, int danioExtra)
    {
        if (oro.Value >= precioOro)
        {
            oro.Value -= precioOro;
            danioMeleeJugador.Value += danioExtra;

            if (tipoArma == "Espada")
            {
                tieneEspada.Value = true;
                cantidadEspadas.Value++;
            }
            else if (tipoArma == "Daga")
            {
                tieneDaga.Value = true;
                cantidadDagas.Value++;
            }

            //Debug.Log($"[SERVIDOR] Jugador {OwnerClientId} compró {tipoArma}. Total {tipoArma}s: {(tipoArma == "Espada" ? cantidadEspadas.Value : cantidadDagas.Value)}");
        }
    }
}