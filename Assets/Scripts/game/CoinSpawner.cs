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

        if (tiempoRestantePartida <= 0f)
        {
            tiempoRestantePartida = 0f;
            tiempoAgotado = true;

            // El servidor le ordena al UIManager (en todas las pantallas) que muestre los botones
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.MostrarBotonesFinPartidaRpc();
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