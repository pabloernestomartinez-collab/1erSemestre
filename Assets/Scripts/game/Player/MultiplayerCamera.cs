using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MultiplayerCamera : NetworkBehaviour
{
    [Header("Configuración de Seguimiento")]
    private Transform camaraPrincipal;
    public Vector3 offset = new Vector3(0f, 1.5f, -4f); // offset.y es la altura del pivote (pecho/hombros)
    public float suavizado = 12f; // Aumentado ligeramente para mayor respuesta en combate

    [Header("Sensibilidad de Entrada")]
    public float sensibilidadMouse = 0.15f;
    public float sensibilidadJoystick = 150f;

    [Header("Límites Verticales (Pitch)")]
    public float minAnguloVertical = -25f; // Cuánto puedes mirar hacia arriba
    public float maxAnguloVertical = 60f;  // Cuánto puedes mirar hacia abajo

    [HideInInspector] public float mouseX = 0f; // Mantiene compatibilidad directa con MovimientoPlayer.cs
    
    private float rotacionY = 0f; // Yaw (Giro Horizontal)
    private float rotacionX = 15f; // Pitch (Giro Vertical inicializado mirando levemente hacia abajo)


    private void Start()
    {
        if (!IsOwner) return;

        BuscarCamaraActual();
        EvaluarEstadoDelCursor(); // <-- Evaluamos el ratón al nacer
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }


    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (!IsOwner) return;

        BuscarCamaraActual();
        EvaluarEstadoDelCursor(); // <-- Volvemos a evaluar cada vez que Unity cambie de mapa
    }

    private void BuscarCamaraActual()
    {
        if (Camera.main != null)
        {
            camaraPrincipal = Camera.main.transform;
            rotacionY = transform.eulerAngles.y;
        }
    }

    private void EvaluarEstadoDelCursor()
    {
        // Si la escena activa se llama exactamente "game" (o el nombre que tenga tu mapa de juego)
        if (SceneManager.GetActiveScene().name == "game")
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // En el Lobby, pantalla de victoria/derrota, o cualquier otro menú: LIBERAR RATÓN.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;

        if (camaraPrincipal == null)
        {
            BuscarCamaraActual();
            return; 
        }

        // 1. LEER INPUTS HÍBRIDOS (Soporta Mouse y Mando de forma simultánea)
        float inputHorizontal = 0f;
        float inputVertical = 0f;

        // Lectura de Mouse
        if (Mouse.current != null)
        {
            Vector2 deltaMouse = Mouse.current.delta.ReadValue();
            inputHorizontal += deltaMouse.x * sensibilidadMouse;
            inputVertical -= deltaMouse.y * sensibilidadMouse; // Invertido para sensación natural de Look
        }

        // Lectura de Gamepad
        if (Gamepad.current != null)
        {
            Vector2 deltaStick = Gamepad.current.rightStick.ReadValue();
            if (deltaStick.magnitude > 0.1f)
            {
                inputHorizontal += deltaStick.x * sensibilidadJoystick * Time.deltaTime;
                inputVertical -= deltaStick.y * sensibilidadJoystick * Time.deltaTime;
            }
        }

        // 2. ACUMULAR Y CLAMPAR ÁNGULOS
        rotacionY += inputHorizontal;
        rotacionX = Mathf.Clamp(rotacionX + inputVertical, minAnguloVertical, maxAnguloVertical);

        // Le pasamos el ángulo absoluto de la cámara (rotacionY) en lugar del delta por frame.
        // Ahora tu personaje siempre rotará fluidamente hacia donde mire la cámara.
        mouseX = rotacionY;

        // 3. CÁLCULO DE ÓRBITA ESFÉRICA (Estilo Action-RPG)
        Quaternion rotacionDeseada = Quaternion.Euler(rotacionX, rotacionY, 0f);

        // El pivote se coloca a la altura del pecho/hombros usando offset.y (evita orbitar los pies)
        Vector3 puntoPivote = transform.position + Vector3.up * offset.y;

        // Calculamos la posición final empujando la cámara hacia atrás (offset.z) y lateralmente (offset.x)
        Vector3 posicionDeseada = puntoPivote + (rotacionDeseada * new Vector3(offset.x, 0f, offset.z));

        // 4. APLICAR INTERPOLACIÓN SUAVE (Lerp de posición + Slerp de rotación)
        camaraPrincipal.position = Vector3.Lerp(camaraPrincipal.position, posicionDeseada, suavizado * Time.deltaTime);
        camaraPrincipal.rotation = Quaternion.Slerp(camaraPrincipal.rotation, rotacionDeseada, suavizado * Time.deltaTime);
    }
}