using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    // Delegado para notificar cambios a la UI (GameHUDManager y ArmasHUDManager)
    public Action OnStatsChanged;

    [Header("Estadísticas del Jugador (Sincronizadas)")]
    public NetworkVariable<int> puntosVida = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> puntosVidaMax = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> oro = new NetworkVariable<int>(50, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> hierba = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> sabiduria = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> puntuacion = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> danioMeleeJugador = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Posesión de Armas (Booleanos)")]
    public NetworkVariable<bool> tieneEspada = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> tieneDaga = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> tieneHueso = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Cantidad de Armas Compradas / Poseídas")]
    public NetworkVariable<int> cantidadEspadas = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> cantidadDagas = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> cantidadHuesos = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Temporizador de puntos por segundo
    private float temporizadorPuntos = 0f;

    // --- Propiedades de Compatibilidad Directa con GameHUDManager ---
    public NetworkVariable<int> vidaActual => puntosVida;
    public NetworkVariable<int> puntos => puntuacion;

    public override void OnNetworkSpawn()
    {
        // Suscripción a eventos de estadísticas generales (¡Mantenemos puntosVida aquí!)
        puntosVida.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        puntosVidaMax.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        oro.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        hierba.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        sabiduria.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        puntuacion.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        danioMeleeJugador.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();

        // Suscripción a eventos de armas
        tieneEspada.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        tieneDaga.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        tieneHueso.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        cantidadEspadas.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        cantidadDagas.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        cantidadHuesos.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();

        if (IsServer)
        {
            tieneHueso.Value = true;
            cantidadHuesos.Value = 1;
        }

        if (IsOwner && TryGetComponent<InventarioPlayer>(out var inventario))
        {
            itemsMenu huesoInicial = ScriptableObject.CreateInstance<itemsMenu>();
            huesoInicial.nombreArma = "Hueso";
            huesoInicial.danioExtra = 0;
            inventario.AgregarItemLocal(huesoInicial);
        }
    }

    public override void OnNetworkDespawn()
    {
        puntosVida.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        puntosVidaMax.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        oro.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        hierba.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        sabiduria.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        puntuacion.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        danioMeleeJugador.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();

        tieneEspada.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        tieneDaga.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        tieneHueso.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        cantidadEspadas.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        cantidadDagas.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
        cantidadHuesos.OnValueChanged -= (oldVal, newVal) => OnStatsChanged?.Invoke();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (puntosVida.Value > 0)
        {
            temporizadorPuntos += Time.deltaTime;
            if (temporizadorPuntos >= 1f)
            {
                puntuacion.Value += 1;
                temporizadorPuntos = 0f;
            }
        }
    }

    // --- Métodos Getters ---
    public int GetVidaActual() => puntosVida.Value;
    public int GetVidaMaxima() => puntosVidaMax.Value;
    public int GetOro() => oro.Value;
    public int GetHierba() => hierba.Value;
    public int GetSabiduria() => sabiduria.Value;
    public int GetPuntuacion() => puntuacion.Value;
    public int GetDanioMelee() => danioMeleeJugador.Value;

    // --- Métodos de Daño y Puntos ---
    public void RecibirDanio(int danio)
    {
        if (!IsServer) return;
        puntosVida.Value = Mathf.Max(0, puntosVida.Value - danio);
    }

    public void SumarPuntos(int cantidad)
    {
        if (!IsServer) return;
        puntuacion.Value += cantidad;
    }

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

    // --- RPCs ---
    [ServerRpc]
    public void CraftearPocionServerRpc(int costoHierba, int costoSabiduria, int curacionHP)
    {
        if (hierba.Value >= costoHierba && sabiduria.Value >= costoSabiduria)
        {
            hierba.Value -= costoHierba;
            sabiduria.Value -= costoSabiduria;
            puntosVida.Value = Mathf.Min(puntosVidaMax.Value, puntosVida.Value + curacionHP);
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
        if (oro.Value < precioOro) return;

        if (tipoArma == "Daga")
        {
            if (tieneHueso.Value && !tieneDaga.Value)
            {
                oro.Value -= precioOro;
                tieneHueso.Value = false;
                cantidadHuesos.Value = 0;
                tieneDaga.Value = true;
                cantidadDagas.Value = 1;
                danioMeleeJugador.Value += danioExtra;
            }
        }
        else if (tipoArma == "Espada")
        {
            if (tieneDaga.Value && !tieneEspada.Value)
            {
                oro.Value -= precioOro;
                tieneDaga.Value = false;
                cantidadDagas.Value = 0;
                tieneEspada.Value = true;
                cantidadEspadas.Value = 1;
                danioMeleeJugador.Value += danioExtra;
            }
        }
    }
}