using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovimientoPlayer : NetworkBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float speed = 10f;

    private Rigidbody rb;
    private Transform mainCameraTransform;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        if (IsOwner)
        {
            BuscarCamara();
        }
    }

    private void BuscarCamara()
    {
        if (UnityEngine.Camera.main != null)
        {
            mainCameraTransform = UnityEngine.Camera.main.transform;
        }
    }

    void Update()
    {
        if (!IsOwner) return; // Candado de red

        // Re-capturar la cámara si cambiamos de escena
        if (mainCameraTransform == null && UnityEngine.Camera.main != null)
        {
            BuscarCamara();
        }

        if (rb == null || mainCameraTransform == null) return;

        RotarHaciaMouse();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return; // Las físicas se procesan preferentemente en FixedUpdate

        MoverJugador();
    }

    private void MoverJugador()
    {
        float moveX = 0f;
        float moveZ = 0f;

        // Soporte para Teclado (Nuevo Input System)
        if (Keyboard.current.wKey.isPressed) moveZ = 1f;
        if (Keyboard.current.sKey.isPressed) moveZ = -1f;
        if (Keyboard.current.aKey.isPressed) moveX = -1f;
        if (Keyboard.current.dKey.isPressed) moveX = 1f;

        // Soporte secundario opcional para Gamepad
        if (Gamepad.current != null && moveX == 0f && moveZ == 0f)
        {
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            moveX = leftStick.x;
            moveZ = leftStick.y;
        }

        // Calcular dirección con base en la cámara (aplanando el eje Y)
        Vector3 camForward = mainCameraTransform.forward;
        camForward.y = 0f;
        camForward = camForward.normalized;

        Vector3 camRight = mainCameraTransform.right;
        camRight.y = 0f;
        camRight = camRight.normalized;

        Vector3 moveDirection = (camForward * moveZ + camRight * moveX).normalized;

        // Aplicar la velocidad en el Rigidbody de forma limpia manteniendo la gravedad actual
        rb.linearVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed);
    }

    private void RotarHaciaMouse()
    {
        // Creamos un rayo desde la posición del mouse en pantalla hacia el mundo 3D
        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        // Creamos un plano matemático invisible a la altura de los pies del jugador (Y = posición actual)
        // Esto garantiza que el cálculo sea perfectamente plano en el suelo
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        // Lanzamos el rayo al plano para ver dónde impacta
        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            // Conseguimos el punto exacto del impacto en el mundo 3D
            Vector3 pointToLook = ray.GetPoint(rayDistance);

            // Calculamos la dirección desde el jugador hacia ese punto del mouse
            Vector3 lookDirection = pointToLook - transform.position;
            lookDirection.y = 0f; // Nos aseguramos de que no intente mirar hacia arriba o abajo

            // Si la distancia es prudente, aplicamos la rotación instantánea y firme
            if (lookDirection.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
}