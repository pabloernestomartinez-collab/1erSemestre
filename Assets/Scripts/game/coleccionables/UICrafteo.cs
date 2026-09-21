using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class UICrafteo : MonoBehaviour
{
    [Header("UI del Crafteo")]
    [Tooltip("Arrastra aquí el Panel de Crafteo (el cuadro visible que se abrirá/cerrará)")]
    [SerializeField] private GameObject panelCrafteo;

    private void Start()
    {
        if (panelCrafteo != null)        // Al iniciar el juego, aseguramos que el panel empiece cerrado

        {
            panelCrafteo.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;        // Verificamos si existe un teclado activo


        if (Keyboard.current.cKey.wasPressedThisFrame)        // Detectar si se presionó la tecla C

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
            Debug.Log($"[CRAFTEO] Panel crafteo conmutado a: {!estaActivo}");
        }
        else
        {
            Debug.LogWarning("[CRAFTEO] No se asignó la referencia 'panelCrafteo' en el Inspector.");
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

    public void CraftearPocion()    // Método asignado al evento On Click () del Botón 'Craftear'

    {
        ProcesarCrafteoPocion(10, 5, 25); // Costo Hierba, Costo Sabiduría, Curación HP
    }

    private void ProcesarCrafteoPocion(int costoHierba, int costoSabiduria, int curacionHP)
    {
        var netObj = NetworkManager.Singleton?.LocalClient?.PlayerObject;

        if (netObj != null && netObj.TryGetComponent<PlayerStats>(out PlayerStats stats))
        {
            if (stats.hierba.Value >= costoHierba && stats.sabiduria.Value >= costoSabiduria)
            {
                stats.CraftearPocionServerRpc(costoHierba, costoSabiduria, curacionHP);
                Debug.Log($"[CRAFTEO] ¡Poción creada! Se usaron {costoHierba} de hierba y {costoSabiduria} de sabiduría.");
            }
            else
            {
                Debug.LogWarning($"[CRAFTEO] Recursos insuficientes. Necesitas {costoHierba} Hierba y {costoSabiduria} Sabiduría.");
            }
        }
    }
}