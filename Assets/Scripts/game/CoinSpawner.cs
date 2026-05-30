using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject monedaPrefab;

    private float tiempoPorSpawn = 2.0f;
    private float cronometroSpawn = 0f;

    [SerializeField] private float tiempoRestantePartida = 60f; // Reduje a 10 para que lo pruebes rápido
    private bool tiempoAgotado = false;

    public override void OnNetworkSpawn()
    {
        cronometroSpawn = 0f;
        tiempoAgotado = false;
    }

    void Update()
    {
        if (!IsServer) return;
        if (tiempoAgotado) return;

        tiempoRestantePartida -= Time.deltaTime;

        // ... (dentro del Update de CoinSpawner.cs cuando el tiempo llega a cero) ...
        if (tiempoRestantePartida <= 0f)
        {
            tiempoRestantePartida = 0f;
            tiempoAgotado = true;
            //Debug.Log("⏱️ [Servidor] ¡El tiempo llegó a CERO! Calculando ganador...");

            // pARA CALCULAR EL GANADOR
            string resultadoFinal = "Empate";
            int puntosHost = 0;
            int puntosCliente = 0;

            // Buscamos los componentes PlayerScore de los jugadores conectados
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(0, out var hostNet))
            {
                if (hostNet.PlayerObject.TryGetComponent<PlayerScore>(out var pScoreHost)) puntosHost = pScoreHost.puntos.Value;
            }
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(1, out var clientNet))
            {
                if (clientNet.PlayerObject.TryGetComponent<PlayerScore>(out var pScoreClient)) puntosCliente = pScoreClient.puntos.Value;
            }

            // Comparamos los valores para armar el mensaje
            if (puntosHost > puntosCliente)
            {
                resultadoFinal = $"🏆 ¡GANÓ EL HOST! ({puntosHost} vs {puntosCliente})";
            }
            else if (puntosCliente > puntosHost)
            {
                resultadoFinal = $"🏆 ¡GANÓ EL CLIENTE! ({puntosCliente} vs {puntosHost})";
            }
            else
            {
                resultadoFinal = $"🤝 ¡EMPATE! ({puntosHost} a {puntosHost})";
            }

            // Enviamos el resultado directo al UIManager
            if (GameUIManager.Instance != null)
            {
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

        float xAleatorio = Random.Range(-4f, 4f);
        float zAleatorio = Random.Range(-4f, 4f);
        float yPosicion = 10f;

        Vector3 posicionAleatoria = new Vector3(xAleatorio, yPosicion, zAleatorio);

        GameObject nuevaMoneda = Instantiate(monedaPrefab, posicionAleatoria, Quaternion.identity);
        nuevaMoneda.GetComponent<NetworkObject>().Spawn();
    }
}