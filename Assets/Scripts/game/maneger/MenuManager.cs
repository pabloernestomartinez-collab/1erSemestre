using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Referencias de Canvas / Paneles")]
    [SerializeField] private GameObject canvasMochila;
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

        // Abrir / Cerrar Mochila con la tecla I o Tab (Opcional)
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleCanvas(canvasMochila);
        }
    }

    public void AbrirMochila() => AbrirUnico(canvasMochila);
    public void AbrirCrafteo() => AbrirUnico(canvasCrafteo);
    public void AbrirKiosco() => AbrirUnico(canvasKiosco);

    public void CerrarTodo()
    {
        if (canvasMochila != null) canvasMochila.SetActive(false);
        if (canvasCrafteo != null) canvasCrafteo.SetActive(false);
        if (canvasKiosco != null) canvasKiosco.SetActive(false);
    }

    private void ToggleCanvas(GameObject targetCanvas)
    {
        if (targetCanvas == null) return;

        bool estabaActivo = targetCanvas.activeSelf;

        // Cerramos los demás paneles
        CerrarTodo();

        // Si estaba cerrado, lo abrimos. Si estaba abierto, se queda cerrado por CerrarTodo()
        targetCanvas.SetActive(!estabaActivo);
    }

    private void AbrirUnico(GameObject targetCanvas)
    {
        if (targetCanvas == null) return;

        CerrarTodo();
        targetCanvas.SetActive(true);
    }
}