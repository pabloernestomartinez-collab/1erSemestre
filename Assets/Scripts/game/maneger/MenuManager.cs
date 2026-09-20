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
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            ToggleCanvas(canvasCrafteo);
        }
    }

    public void AbrirMochila() => ToggleCanvas(canvasMochila);
    public void AbrirCrafteo() => ToggleCanvas(canvasCrafteo);
    public void AbrirKiosco() => ToggleCanvas(canvasKiosco);

    public void CerrarTodo()
    {
        if (canvasMochila) canvasMochila.SetActive(false);
        if (canvasCrafteo) canvasCrafteo.SetActive(false);
        if (canvasKiosco) canvasKiosco.SetActive(false);
    }

    private void ToggleCanvas(GameObject targetCanvas)
    {
        if (targetCanvas == null) return;
        bool estadoActual = targetCanvas.activeSelf;

        CerrarTodo();

        targetCanvas.SetActive(!estadoActual);
    }
}