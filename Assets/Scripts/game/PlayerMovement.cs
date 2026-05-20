using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;
    public float rotationSpeed = 15f;

    private Rigidbody rb;
    private Transform mainCameraTransform;
    private MultiplayerCamera cameraScript; // Referencia al script de tu cámara

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            rb = GetComponent<Rigidbody>();

            if (UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;
                // Buscamos el script de la cámara que está en el juego
                cameraScript = GameObject.FindAnyObjectByType<MultiplayerCamera>();
            }
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        // --- CORRECCIÓN DE CÁMARA MULTIJUGADOR ---
        // En lugar de buscar en toda la escena, nos aseguramos de agarrar la cámara local de ESTA pantalla
        if (mainCameraTransform == null && UnityEngine.Camera.main != null)
        {
            mainCameraTransform = UnityEngine.Camera.main.transform;

            // Buscamos el script de la cámara dentro de esa cámara principal específica
            cameraScript = UnityEngine.Camera.main.GetComponent<MultiplayerCamera>();

            // Si tu script de cámara no está en el mismo objeto que la cámara principal,
            // sino en un objeto hijo de tu propio jugador, usa esta línea en su lugar:
            // cameraScript = GetComponentInChildren<MultiplayerCamera>();
        }
        // -----------------------------------------

        // --- 1. LEER INPUTS ---
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

        // --- 2. ROTACIÓN Y MOVIMIENTO LOCAL ---
        if (rb != null)
        {
            // Si logramos conectar con el script de la cámara local
            if (cameraScript != null)
            {
                // El cuerpo del personaje copia el ángulo horizontal de SU cámara
                Quaternion targetRotation = Quaternion.Euler(0f, cameraScript.mouseX, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Calculamos la dirección usando el frente de nuestro propio cuerpo (que ya mira a nuestra cámara)
            Vector3 moveDirection = (transform.forward * moveZ + transform.right * moveX).normalized;

            // Aplicamos la velocidad física
#if UNITY_2023_1_OR_NEWER
            rb.linearVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed);
#else
        rb.velocity = new Vector3(moveDirection.x * speed, rb.velocity.y, moveDirection.z * speed);
#endif
        }
    }
}