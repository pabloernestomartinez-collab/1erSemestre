using System.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ArmasHUDManager : MonoBehaviour
{
    [Header("Conteo de Armas (UI)")]
    [SerializeField] private TextMeshProUGUI espadasText;
    [SerializeField] private TextMeshProUGUI dagasText;
    [SerializeField] private TextMeshProUGUI huesosText;

    private PlayerStats jugadorLocalStats;
    private InventarioPlayer jugadorLocalInventario;

    private void Start()
    {
        StartCoroutine(EsperarYVincularJugador());
    }

    private IEnumerator EsperarYVincularJugador()
    {
        while (SceneManager.GetActiveScene().name.ToLower() == "lobby")
        {
            yield return new WaitForSeconds(0.1f);
        }

        while (jugadorLocalStats == null)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
            {
                var jugadorObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
                if (jugadorObj != null)
                {
                    if (jugadorObj.TryGetComponent<PlayerStats>(out var stats))
                    {
                        jugadorLocalStats = stats;
                    }

                    if (jugadorObj.TryGetComponent<InventarioPlayer>(out var inv))
                    {
                        jugadorLocalInventario = inv;
                        jugadorLocalInventario.OnInventarioCambiado += ActualizarConteoArmas;
                    }
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        // Suscribir a los eventos de las NetworkVariables directas
        jugadorLocalStats.cantidadEspadas.OnValueChanged += OnArmasChanged;
        jugadorLocalStats.cantidadDagas.OnValueChanged += OnArmasChanged;

        // Si tienes la variable en PlayerStats, la escuchamos directamente
        if (jugadorLocalStats.cantidadHuesos != null)
        {
            jugadorLocalStats.cantidadHuesos.OnValueChanged += OnArmasChanged;
        }

        ActualizarConteoArmas();
    }

    private void OnArmasChanged(int valorAnterior, int valorNuevo)
    {
        ActualizarConteoArmas();
    }

    public void ActualizarConteoArmas()
    {
        int totalEspadas = 0;
        int totalDagas = 0;
        int totalHuesos = 0;

        // Opción A: Leer de PlayerStats
        if (jugadorLocalStats != null)
        {
            totalEspadas = jugadorLocalStats.cantidadEspadas.Value;
            totalDagas = jugadorLocalStats.cantidadDagas.Value;
            if (jugadorLocalStats.cantidadHuesos != null)
            {
                totalHuesos = jugadorLocalStats.cantidadHuesos.Value;
            }
        }

        // Opción B: Si no se han seteado variables sincronizadas, verificar el inventario local, recomendacion de google
        if (jugadorLocalInventario != null)
        {
            int eCount = 0, dCount = 0, hCount = 0;

            foreach (itemsMenu item in jugadorLocalInventario.listaDeItems)
            {
                if (item == null) continue;
                string nombre = item.nombreArma.ToLower();
                if (nombre.Contains("espada")) eCount++;
                else if (nombre.Contains("daga")) dCount++;
                else if (nombre.Contains("hueso")) hCount++;
            }

            // Usar el valor mayor entre Stats e Inventario
            totalEspadas = Mathf.Max(totalEspadas, eCount);
            totalDagas = Mathf.Max(totalDagas, dCount);
            totalHuesos = Mathf.Max(totalHuesos, hCount);
        }

        if (espadasText != null) espadasText.text = "Espadas: " + totalEspadas;
        if (dagasText != null) dagasText.text = "Dagas: " + totalDagas;
        if (huesosText != null) huesosText.text = "Huesos: " + totalHuesos;
    }

    private void OnDestroy()
    {
        if (jugadorLocalStats != null)
        {
            jugadorLocalStats.cantidadEspadas.OnValueChanged -= OnArmasChanged;
            jugadorLocalStats.cantidadDagas.OnValueChanged -= OnArmasChanged;

            if (jugadorLocalStats.cantidadHuesos != null)
            {
                jugadorLocalStats.cantidadHuesos.OnValueChanged -= OnArmasChanged;
            }
        }

        if (jugadorLocalInventario != null)
        {
            jugadorLocalInventario.OnInventarioCambiado -= ActualizarConteoArmas;
        }
    }
}