using UnityEngine;
using UnityEngine.InputSystem; // 🔥 Importante para el nuevo Input System

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
        // 🔥 Uso compatible con el nuevo Input System para la tecla 'I'
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleCanvas(canvasMochila);
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