using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerStats : NetworkBehaviour
{
    [Header("Configuración de Vida")]
    [SerializeField] private int vidaMaxima = 100;

    [Header("Configuración de Combate")]
    [SerializeField] private int danioMeleeJugador = 25; // Daño que inflige el jugador

    public int GetDanioMelee() => danioMeleeJugador;    // Método público (Getter) para que el script de ataque pueda leer este daño

    public NetworkVariable<int> vidaActual = new NetworkVariable<int>(100);

    [Header("Recursos Sincronizados")]
    public NetworkVariable<int> hierro = new NetworkVariable<int>(0);
    public NetworkVariable<int> madera = new NetworkVariable<int>(0);
    public NetworkVariable<int> fuego = new NetworkVariable<int>(0);
    public NetworkVariable<int> agua = new NetworkVariable<int>(0);
    public NetworkVariable<int> piedra = new NetworkVariable<int>(0);
    public NetworkVariable<int> puntos = new NetworkVariable<int>(0);

    public Action OnStatsChanged;    // Evento para avisarle a la UI que un valor cambió sin usar el Update

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            vidaActual.Value = vidaMaxima;
        }

        // Nos suscribimos a los cambios de red de cada variable (incluyendo la vida y los nuevos puntos)
        vidaActual.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        hierro.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        madera.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        fuego.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        agua.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        piedra.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke();
        puntos.OnValueChanged += (oldVal, newVal) => OnStatsChanged?.Invoke(); // 🔥 Registramos los puntos en la UI

        OnStatsChanged?.Invoke();        // Disparamos un aviso inicial para que la UI dibuje los valores correctos al nacer
    }

    public void RecibirDanio(int cantidadDanio)
    {
        if (!IsServer) return;

        vidaActual.Value -= cantidadDanio;        // Restamos vida de forma segura en el servidor

        Debug.Log($"[SERVIDOR] Jugador {OwnerClientId} recibió {cantidadDanio} de daño. Vida restante: {vidaActual.Value}");

        // Comprobamos si el jugador murió
        if (vidaActual.Value <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        vidaActual.Value = vidaMaxima;        //  revivir con la vida al máximo hasta saber que tengo que hacer con la programación
    }

    public void SumarPuntos(int cantidad)
    {
        if (IsServer)
        {
            puntos.Value += cantidad;
            Debug.Log($"⭐ [SERVIDOR] Jugador {OwnerClientId} ganó {cantidad} puntos por velocidad. Total: {puntos.Value}");
        }
    }

    public void SumarHierro() { if (IsServer) hierro.Value++; }
    public void SumarMadera() { if (IsServer) madera.Value++; }
    public void SumarFuego() { if (IsServer) fuego.Value++; }
    public void SumarAgua() { if (IsServer) agua.Value++; }
    public void SumarPiedra() { if (IsServer) piedra.Value++; }
    public int GetVidaMaxima() => vidaMaxima;
}