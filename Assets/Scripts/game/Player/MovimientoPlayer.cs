using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MovimientoPlayer : NetworkBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 150f;
    [SerializeField] private float gravedad = -9.81f;

    private CharacterController controller;
    private Vector2 inputMovimiento;
    private float inputRotacion;
    private float velocidadY = 0f;

    public override void OnNetworkSpawn()
    {
        controller = GetComponent<CharacterController>();

        // Si existe un Rigidbody en el Prefab por error, lo desactivamos para evitar interferencias
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        // 1. CAPTURA DE INPUTS
        CapturarEntradas();

        // 2. MOVIMIENTO Y ROTACIÓN (Todo en Update con Time.deltaTime para fluidez perfecta)
        RotarJugador();
        MoverJugador();
    }

    private void CapturarEntradas()
    {
        float moveX = 0f;
        float moveZ = 0f;
        inputRotacion = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveZ = 1f;
            if (Keyboard.current.sKey.isPressed) moveZ = -1f;

            if (Keyboard.current.aKey.isPressed) inputRotacion -= 1f;
            if (Keyboard.current.dKey.isPressed) inputRotacion += 1f;
        }

        // Soporte secundario para Gamepad
        if (Gamepad.current != null && moveX == 0f && moveZ == 0f)
        {
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            moveX = leftStick.x;
            moveZ = leftStick.y;
        }

        inputMovimiento = new Vector2(moveX, moveZ).normalized;
    }

    private void MoverJugador()
    {
        // Aplicar gravedad manual para mantener el personaje pegado al suelo
        if (controller.isGrounded && velocidadY < 0)
        {
            velocidadY = -2f; // Pequeño empuje constante hacia abajo
        }

        velocidadY += gravedad * Time.deltaTime;

        // Calcular dirección de movimiento según la orientación del jugador
        Vector3 moveDirection = (transform.forward * inputMovimiento.y + transform.right * inputMovimiento.x);
        Vector3 desplazamiento = (moveDirection * speed) + (Vector3.up * velocidadY);

        // Mover con CharacterController
        controller.Move(desplazamiento * Time.deltaTime);
    }

    private void RotarJugador()
    {
        if (Mathf.Abs(inputRotacion) > 0.01f)
        {
            transform.Rotate(Vector3.up, inputRotacion * rotationSpeed * Time.deltaTime);
        }
    }
}