using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovimientoPlayer : NetworkBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 150f;

    private Rigidbody rb;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (!IsOwner) return;

        // Rotación con Q y E
        RotarConTeclas();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        MoverJugador();
    }

    private void MoverJugador()
    {
        float moveX = 0f;
        float moveZ = 0f;

        // Lectura de teclado
        if (Keyboard.current.wKey.isPressed) moveZ = 1f;
        if (Keyboard.current.sKey.isPressed) moveZ = -1f;
        if (Keyboard.current.aKey.isPressed) moveX = -1f;
        if (Keyboard.current.dKey.isPressed) moveX = 1f;

        // Soporte secundario para Gamepad
        if (Gamepad.current != null && moveX == 0f && moveZ == 0f)
        {
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            moveX = leftStick.x;
            moveZ = leftStick.y;
        }

        Vector3 moveDirection = (transform.forward * moveZ + transform.right * moveX).normalized;

        // Aplicar velocidad al Rigidbody
        rb.linearVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed);
    }

    private void RotarConTeclas()
    {
        float rotacion = 0f;

        if (Keyboard.current.qKey.isPressed) rotacion -= 1f; // Girar a la izquierda
        if (Keyboard.current.eKey.isPressed) rotacion += 1f; // Girar a la derecha

        if (rotacion != 0f)
        {
            transform.Rotate(Vector3.up, rotacion * rotationSpeed * Time.deltaTime);
        }
    }
}