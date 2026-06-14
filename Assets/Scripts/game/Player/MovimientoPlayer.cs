using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoPlayer : NetworkBehaviour
{
    [Header("Configuración de Movimiento")]
    public float speed = 10f;
    public float rotationSpeed = 15f;

    private Rigidbody rb;
    private Transform mainCameraTransform;
    private MultiplayerCamera cameraScript;

    public override void OnNetworkSpawn()
    {
        // Vinculamos el Rigidbody para que funcione tanto en el Servidor como en los Clientes
        rb = GetComponent<Rigidbody>();

        if (IsOwner)
        {
            // Solo el dueño busca su cámara local y el script de rotación
            if (UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;
                cameraScript = GameObject.FindAnyObjectByType<MultiplayerCamera>();
            }
        }
    }

    void Update()
    {
        // Candado de Red: Solo el jugador dueño de este personaje puede controlarlo
        if (!IsOwner) return;

        // Protección por si la cámara cambia o se destruye al cambiar de escena
        if (mainCameraTransform == null && UnityEngine.Camera.main != null)
        {
            mainCameraTransform = UnityEngine.Camera.main.transform;
            cameraScript = UnityEngine.Camera.main.GetComponent<MultiplayerCamera>();
        }

        // 1. LEER INPUTS (Controles)
        float moveX = 0f;
        float moveZ = 0f;

        if (Gamepad.current != null)
        {
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            moveX = leftStick.x;
            moveZ = leftStick.y;
        }
        else
        {
            if (Keyboard.current.wKey.isPressed) moveZ = 1f;
            if (Keyboard.current.sKey.isPressed) moveZ = -1f;
            if (Keyboard.current.aKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed) moveX = 1f;
        }

        // 2. APLICAR ROTACIÓN Y MOVIMIENTO FÍSICO
        if (rb != null)
        {
            // Rotar el cuerpo del jugador hacia donde mira la cámara horizontalmente
            if (cameraScript != null)
            {
                Quaternion targetRotation = Quaternion.Euler(0f, cameraScript.mouseX, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Calcular dirección y aplicar velocidad manteniendo la gravedad física del Rigidbody (Y)
            Vector3 moveDirection = (transform.forward * moveZ + transform.right * moveX).normalized;
            rb.linearVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed);
        }
    }
}