using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject monedaPrefab;

    private float tiempoPorSpawn = 2.0f;
    private float cronometroSpawn = 0f;

    // Seteamos los 600 segundos por código
    private float tiempoRestantePartida = 600f;
    private bool tiempoAgotado = false;

    public override void OnNetworkSpawn()
    {
        cronometroSpawn = 0f;
        tiempoAgotado = false;

        // obligando a Unity a ignorar cualquier número viejo del Inspector.
        tiempoRestantePartida = 600f;
    }

    void Update()
    {
        if (!IsServer) return;
        if (tiempoAgotado) return;

        tiempoRestantePartida -= Time.deltaTime;

        if (tiempoRestantePartida <= 0f)
        {
            tiempoRestantePartida = 0f;
            tiempoAgotado = true;

            string resultadoFinal = "Empate";
            int puntosHost = 0;
            int puntosCliente = 0;

            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(0, out var hostNet))
            {
                if (hostNet.PlayerObject.TryGetComponent<PlayerScore>(out var pScoreHost)) puntosHost = pScoreHost.puntos.Value;
            }
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(1, out var clientNet))
            {
                if (clientNet.PlayerObject.TryGetComponent<PlayerScore>(out var pScoreClient)) puntosCliente = pScoreClient.puntos.Value;
            }

            if (puntosHost > puntosCliente)
            {
                resultadoFinal = $"¡GANÓ EL HOST! ({puntosHost} vs {puntosCliente})";
            }
            else if (puntosCliente > puntosHost)
            {
                resultadoFinal = $"¡GANÓ EL CLIENTE! ({puntosCliente} vs {puntosHost})";
            }
            else
            {
                resultadoFinal = $"¡EMPATE! ({puntosHost} a {puntosHost})";
            }

            if (GameUIManager.Instance != null)
            {
                // NOTA: Asegúrate de que este método en tu GameUIManager termine con el sufijo 'Rpc'
                GameUIManager.Instance.MostrarBotonesFinPartidaRpc(resultadoFinal);
            }
            return;
        }

        cronometroSpawn += Time.deltaTime;
        if (cronometroSpawn >= tiempoPorSpawn)
        {
            GenerarMonedaAleatoria();
            cronometroSpawn = 0f;
        }
    }

    private void GenerarMonedaAleatoria()
    {
        if (monedaPrefab == null) return;

        float xAleatorio = Random.Range(-400f, 400f);
        float zAleatorio = Random.Range(-400f, 400f);
        float yPosicion = 10f;

        Vector3 posicionAleatoria = new Vector3(xAleatorio, yPosicion, zAleatorio);

        GameObject nuevaMoneda = Instantiate(monedaPrefab, posicionAleatoria, Quaternion.identity);
        nuevaMoneda.GetComponent<NetworkObject>().Spawn();
    }
}