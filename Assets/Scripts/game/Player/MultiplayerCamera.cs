using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Requerido para detectar el cambio de escenas

public class MultiplayerCamera : NetworkBehaviour
{
    [Header("Configuración de Seguimiento")]
    private Transform camaraPrincipal;
    public Vector3 offset = new Vector3(0f, 2f, -4f);
    public float suavizado = 5f;

    [Header("Configuración de Rotación (Joystick Mando)")]
    public float sensibilidadJoystick = 150f;

    [HideInInspector] public float mouseX = 0f;
    private float rotacionY = 0f;

    private void Start()
    {
        if (!IsOwner) return;

        BuscarCamaraActual();
        Cursor.visible = false;
    }

    // NUEVO: Nos suscribimos a los eventos de Unity para saber cuándo se carga una escena nueva
    private void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    // NUEVO: Este método se ejecuta automáticamente CADA VEZ que Unity cambia de escena
    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (!IsOwner) return;

        // Forzamos a buscar la nueva cámara de la escena "game"
        BuscarCamaraActual();
    }

    // NUEVO: Método centralizado para encontrar la cámara activa
    private void BuscarCamaraActual()
    {
        if (Camera.main != null)
        {
            camaraPrincipal = Camera.main.transform;
            rotacionY = transform.eulerAngles.y;
            //Debug.Log($"[Cámara] Vinculada con éxito en la escena: {SceneManager.GetActiveScene().name}");
        }
        else
        {
            //Debug.LogWarning("[Cámara] No se encontró 'MainCamera'. Si estás en el Lobby transicionando, es normal por un frame.");
        }
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;

        // Si por alguna razón la cámara se perdió (cambio de escena brusco), intentamos buscarla en caliente
        if (camaraPrincipal == null)
        {
            BuscarCamaraActual();
            return; // Esperamos al siguiente frame para no tirar error
        }

        // Resetear el valor por frame
        mouseX = 0f;

        // LEER JOYSTICK DERECHO
        if (Gamepad.current != null)
        {
            float joystickX = Gamepad.current.rightStick.x.ReadValue();

            if (Mathf.Abs(joystickX) > 0.1f)
            {
                mouseX = joystickX * sensibilidadJoystick * Time.deltaTime;
            }
        }

        // Aplicamos la rotación
        rotacionY += mouseX;
        transform.rotation = Quaternion.Euler(0f, rotacionY, 0f);

        // SEGUIMIENTO SUAVE DE POSICIÓN
        Vector3 posicionDeseada = transform.position + (transform.rotation * offset);
        Vector3 posicionSuave = Vector3.Lerp(camaraPrincipal.position, posicionDeseada, suavizado * Time.deltaTime);

        camaraPrincipal.position = posicionSuave;
        camaraPrincipal.LookAt(transform.position + Vector3.up * (offset.y * 0.5f));
    }
}