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
    private MultiplayerCamera cameraScript; // Guarda el acceso al script de rotación horizontal de la cámara

    
    public override void OnNetworkSpawn()// Este método nativo de Netcode se ejecuta automáticamente cuando el objeto "nace" en la red
    {
        if (IsOwner)
        {
            rb = GetComponent<Rigidbody>(); // Vincula el componente físico para aplicar velocidades más adelante

            if (UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;

                cameraScript = GameObject.FindAnyObjectByType<MultiplayerCamera>();// google????
            }
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        if (mainCameraTransform == null && UnityEngine.Camera.main != null) // Al saltar del Lobby a la escena de juego, las cámaras viejas se destruyen
        {
            mainCameraTransform = UnityEngine.Camera.main.transform;

            cameraScript = UnityEngine.Camera.main.GetComponent<MultiplayerCamera>();            // Busca el script pegado directamente en la cámara principal del jugador local (Evita interferencias con la cámara del rival)

        }

        float moveX = 0f;
        float moveZ = 0f;

        if (Gamepad.current != null)        // google: Si hay un joystick/mando conectado, leemos sus valores analógicos (Stick izquierdo)

        {
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            moveX = leftStick.x;
            moveZ = leftStick.y;
        }
        else // Si no hay mando, recurrimos al teclado tradicional como respaldo técnico
        {
            if (Keyboard.current.wKey.isPressed) moveZ = 1f;
            if (Keyboard.current.sKey.isPressed) moveZ = -1f;
            if (Keyboard.current.aKey.isPressed) moveX = -1f;
            if (Keyboard.current.dKey.isPressed) moveX = 1f;
        }

        
        if (rb != null)// analizar la correccion de google... ROTACIÓN Y MOVIMIENTO FÍSICO LOCAL ---
        {
            if (cameraScript != null)
            {
                Quaternion targetRotation = Quaternion.Euler(0f, cameraScript.mouseX, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            Vector3 moveDirection = (transform.forward * moveZ + transform.right * moveX).normalized;
            rb.linearVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed);
        }
    }
}