using Unity.Netcode;
using UnityEngine;
using TMPro; 

public class PlayerScore : NetworkBehaviour
{
    // Una variable de red que guarda los puntos sincronizados automáticamente
    public readonly NetworkVariable<int> puntos = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Opcional: Arrastra un texto aquí dentro del prefab si quieres ver tus puntos individuales
    [SerializeField] private TextMeshProUGUI textoPuntajeLocal;

    public override void OnNetworkSpawn()
    {
        // Nos suscribimos para actualizar el texto si los puntos cambian
        puntos.OnValueChanged += AlCambiarPuntos;
        ActualizarUI(puntos.Value);
    }

    public override void OnNetworkDespawn()
    {
        puntos.OnValueChanged -= AlCambiarPuntos;
    }

    // Este método solo lo puede llamar el Servidor (como pasa en el script de la zona de entrega)
    public void AddPoints(int cantidad)
    {
        if (!IsServer) return;
        puntos.Value += cantidad;
    }

    private void AlCambiarPuntos(int valorViejo, int valorNuevo)
    {
        ActualizarUI(valorNuevo);
    }

    private void ActualizarUI(int puntosActuales)
    {
        // Solo actualizamos el texto si eres el dueño de este personaje (tu propia pantalla)
        if (IsOwner && textoPuntajeLocal != null)
        {
            textoPuntajeLocal.text = "Puntos: " + puntosActuales;
        }
    }
}
