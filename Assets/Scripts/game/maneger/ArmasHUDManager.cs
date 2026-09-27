using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ArmasHUDManager : MonoBehaviour
{
    [Header("UI del Arma Equipada")]
    [SerializeField] private Image iconoArmaEquipada;
    [SerializeField] private TextMeshProUGUI nombreArmaText;
    [SerializeField] private TextMeshProUGUI danioArmaText; // Texto para el daño

    [Header("Sprites de Armas")]
    [SerializeField] private Sprite spriteHueso;
    [SerializeField] private Sprite spriteDaga;
    [SerializeField] private Sprite spriteEspada;

    private PlayerStats jugadorLocalStats;

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
                if (jugadorObj != null && jugadorObj.TryGetComponent<PlayerStats>(out var stats))
                {
                    jugadorLocalStats = stats;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        // Suscribir a los booleanos de armas
        jugadorLocalStats.tieneHueso.OnValueChanged += OnArmaEquipadaChanged;
        jugadorLocalStats.tieneDaga.OnValueChanged += OnArmaEquipadaChanged;
        jugadorLocalStats.tieneEspada.OnValueChanged += OnArmaEquipadaChanged;

        // Suscripción de respaldo
        jugadorLocalStats.OnStatsChanged += ActualizarArmaEnHUD;

        ActualizarArmaEnHUD();
    }

    private void OnArmaEquipadaChanged(bool valorAnterior, bool valorNuevo)
    {
        ActualizarArmaEnHUD();
    }

    public void ActualizarArmaEnHUD()
    {
        if (jugadorLocalStats == null) return;

        // 1. Si la espada está equipada
        if (jugadorLocalStats.tieneEspada.Value)
        {
            MostrarArma("Espada", spriteEspada, 10);
        }
        // 2. Si la daga está equipada
        else if (jugadorLocalStats.tieneDaga.Value)
        {
            MostrarArma("Daga", spriteDaga, 5);
        }
        // 3. Por defecto / Hueso
        else if (jugadorLocalStats.tieneHueso.Value)
        {
            MostrarArma("Hueso", spriteHueso, 1);
        }
        else
        {
            if (iconoArmaEquipada != null) iconoArmaEquipada.gameObject.SetActive(false);
            if (nombreArmaText != null) nombreArmaText.text = "Sin Arma";
            if (danioArmaText != null) danioArmaText.text = "";
        }
    }

    private void MostrarArma(string nombre, Sprite sprite, int danio)
    {
        if (nombreArmaText != null)
        {
            nombreArmaText.text = nombre;
        }

        if (danioArmaText != null)
        {
            danioArmaText.text = $"DAÑO {danio}";
        }

        if (iconoArmaEquipada != null)
        {
            if (sprite != null)
            {
                iconoArmaEquipada.sprite = sprite;
                iconoArmaEquipada.gameObject.SetActive(true);
            }
            else
            {
                iconoArmaEquipada.gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        if (jugadorLocalStats != null)
        {
            jugadorLocalStats.tieneHueso.OnValueChanged -= OnArmaEquipadaChanged;
            jugadorLocalStats.tieneDaga.OnValueChanged -= OnArmaEquipadaChanged;
            jugadorLocalStats.tieneEspada.OnValueChanged -= OnArmaEquipadaChanged;

            jugadorLocalStats.OnStatsChanged -= ActualizarArmaEnHUD;
        }
    }
}