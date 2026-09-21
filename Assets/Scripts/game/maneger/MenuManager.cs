using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Referencias de Canvas / Paneles Intermitentes")]
    [SerializeField] private GameObject canvasCrafteo;
    [SerializeField] private GameObject canvasKiosco;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Abrir / Cerrar Crafteo con la tecla C
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            ToggleCanvas(canvasCrafteo);
        }
    }

    // Métodos públicos para botones de la UI o Triggers
    public void AbrirCrafteo() => AbrirUnico(canvasCrafteo);
    public void AbrirKiosco() => AbrirUnico(canvasKiosco);

    /// <summary>
    /// Cierra todos los menús emergentes (Tiendas/Crafteo)
    /// </summary>
    public void CerrarTodo()
    {
        if (canvasCrafteo != null) canvasCrafteo.SetActive(false);
        if (canvasKiosco != null) canvasKiosco.SetActive(false);
    }

    private void ToggleCanvas(GameObject targetCanvas)
    {
        if (targetCanvas == null) return;

        bool estabaActivo = targetCanvas.activeSelf;

        // Cerramos cualquier otro menú abierto
        CerrarTodo();

        // Si estaba cerrado lo abrimos, si estaba abierto se queda cerrado por CerrarTodo()
        targetCanvas.SetActive(!estabaActivo);
    }

    private void AbrirUnico(GameObject targetCanvas)
    {
        if (targetCanvas == null) return;

        CerrarTodo();
        targetCanvas.SetActive(true);
    }
}