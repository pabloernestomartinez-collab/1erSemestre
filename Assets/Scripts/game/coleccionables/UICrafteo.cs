using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICrafteo : MonoBehaviour
{
    [Header("Receta Poción")]
    [SerializeField] private int costoHierbaPocion = 2;
    [SerializeField] private int costoSabiduriaPocion = 1;

    [Header("UI")]
    [SerializeField] private Button botonCraquearPocion;
    [SerializeField] private TextMeshProUGUI textoRequisitos;

    private void OnEnable()
    {
        if (textoRequisitos != null)
        {
            textoRequisitos.text = $"Poción de Vida\nRequiere: {costoHierbaPocion} Hierbas, {costoSabiduriaPocion} Sabiduría";
        }
    }

    // Método asignado al botón "Craquear Poción"
    public void IntentarCraquearPocion()
    {
        var netObj = Unity.Netcode.NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (netObj != null && netObj.TryGetComponent<PlayerStats>(out PlayerStats stats))
        {
            // Validamos que tenga recursos
            if (stats.hierba.Value >= costoHierbaPocion && stats.sabiduria.Value >= costoSabiduriaPocion)
            {
                // Petición al servidor (ServerRpc) para procesar el crafteo seguro
                stats.CraquearPocionServerRpc(costoHierbaPocion, costoSabiduriaPocion);
            }
            else
            {
                Debug.Log("❌ No tienes suficientes materiales para la poción.");
            }
        }
    }
}