using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class UICrafteo : MonoBehaviour
{
    [Header("UI del Crafteo")]
    [SerializeField] private GameObject panelCrafteo;

    [Header("Costos de Craftear Poción")]
    [SerializeField] private int costoHierba = 2;
    [SerializeField] private int costoSabiduria = 5;

    private void Start()
    {
        // Al iniciar el juego, aseguramos que el panel empiece cerrado
        if (panelCrafteo != null)
        {
            panelCrafteo.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Abrir/Cerrar panel de crafteo con la tecla 'C'
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            TogglePanel();
        }
    }

    public void TogglePanel()
    {
        if (panelCrafteo != null)
        {
            bool estaActivo = panelCrafteo.activeSelf;
            panelCrafteo.SetActive(!estaActivo);
        }
    }

    public void AbrirPanel()
    {
        if (panelCrafteo != null) panelCrafteo.SetActive(true);
    }

    public void CerrarPanel()
    {
        if (panelCrafteo != null) panelCrafteo.SetActive(false);
    }

    // Método asignado al evento On Click () del Botón 'Craftear' en la UI
    public void CraftearPocion()
    {
        ProcesarCrafteoPocion();
    }

    private void ProcesarCrafteoPocion()
    {
        var netObj = NetworkManager.Singleton?.LocalClient?.PlayerObject;

        if (netObj != null && netObj.TryGetComponent<PocionManager>(out PocionManager pocionMgr))
        {
            // Ejecutar la petición de crafteo directamente a través del PocionManager del jugador local
            pocionMgr.IntentarCraftearPocion();
        }

    }
}