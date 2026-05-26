using Unity.Netcode; // Requerido para heredar de NetworkBehaviour e identificar al dueño del objeto en red
using UnityEngine;
using UnityEngine.InputSystem; // Requerido para el nuevo sistema de Inputs de Unity (Gamepad/Keyboard)

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;
    public float rotationSpeed = 15f;

    private Rigidbody rb;
    private Transform mainCameraTransform;
    private MultiplayerCamera cameraScript; // Guarda el acceso al script de rotación horizontal de la cámara

    // Este método nativo de Netcode se ejecuta automáticamente cuando el objeto "nace" en la red
    public override void OnNetworkSpawn()
    {
        // ESCUDO DE RED MÁS IMPORTANTE: El código interno solo se ejecutará en la computadora del jugador que maneja este personaje.
        // Evita que el Host inicialice componentes o busque cámaras basándose en el personaje del Cliente, y viceversa.
        if (IsOwner)
        {
            rb = GetComponent<Rigidbody>(); // Vincula el componente físico para aplicar velocidades más adelante

            if (UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;

                // Intento inicial de buscar el script de la cámara (Útil si el objeto ya existía en la escena desde el inicio)
                cameraScript = GameObject.FindAnyObjectByType<MultiplayerCamera>();
            }
        }
    }

    void Update()
    {
        // Si este clon del personaje le pertenece a otro jugador en la red, detenemos el Update de inmediato.
        // Sin esta línea, cuando tú muevas tu stick, TODOS los personajes de la pantalla se moverían al mismo tiempo.
        if (!IsOwner) return;

        // --- PARCHE CLAVE PARA CAMBIO DE ESCENA MULTIJUGADOR ---
        // Al saltar del Lobby a la escena de juego, las cámaras viejas se destruyen. Si este personaje se quedó sin cámara,
        // este bloque detecta la nueva cámara principal de la escena y vuelve a enlazar su componente 'MultiplayerCamera'.
        if (mainCameraTransform == null && UnityEngine.Camera.main != null)
        {
            mainCameraTransform = UnityEngine.Camera.main.transform;

            // Busca el script pegado directamente en la cámara principal del jugador local (Evita interferencias con la cámara del rival)
            cameraScript = UnityEngine.Camera.main.GetComponent<MultiplayerCamera>();
        }

        // --- 1. SANEAMIENTO DE INPUTS (Soporte Híbrido) ---
        float moveX = 0f;
        float moveZ = 0f;

        // Si hay un joystick/mando conectado, leemos sus valores analógicos (Stick izquierdo)
        if (Gamepad.current != null)
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

        // --- 2. ROTACIÓN Y MOVIMIENTO FÍSICO LOCAL ---
        if (rb != null)
        {
            // Si el enlace con la cámara local es exitoso, alineamos el cuerpo del personaje
            if (cameraScript != null)
            {
                // Extraemos el ángulo horizontal (Y) que el mouse/stick derecho aplicó a la cámara
                Quaternion targetRotation = Quaternion.Euler(0f, cameraScript.mouseX, 0f);

                // Rotamos el cuerpo del personaje suavemente (Slerp) para que su espalda siempre mire hacia nuestra pantalla
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // CÁLCULO DE DIRECCIÓN RELATIVA: Multiplicamos los inputs por los vectores de dirección propios del cuerpo (forward y right).
            // Esto asegura que al presionar "W", el personaje corra hacia donde apunta su propio pecho (que ya está mirando a su cámara).
            Vector3 moveDirection = (transform.forward * moveZ + transform.right * moveX).normalized;

            // DIRECTIVA DE COMPILACIÓN: Adapta el código según la versión de Unity que compiles.
            // Protege la gravedad manteniendo intacta la velocidad actual en el eje Y ('linearVelocity.y' o 'velocity.y').
            rb.linearVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed);
        }
    }
}