using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class MovimientoPlayer : NetworkBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float rotationSpeed = 150f;
    [SerializeField] private float gravedad = -9.81f;

    private CharacterController controller;
    private Animator animator;

    private float inputAvance;  // W / S
    private float inputRotacion; // A / D
    private float velocidadY = 0f;

    public override void OnNetworkSpawn()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        CapturarEntradas();
        RotarJugador();
        MoverJugador();
        ActualizarAnimaciones();
    }

    private void CapturarEntradas()
    {
        inputAvance = 0f;
        inputRotacion = 0f;

        if (Keyboard.current != null)
        {
            // Avance hacia adelante y atrás (W / S)
            if (Keyboard.current.wKey.isPressed) inputAvance += 1f;
            if (Keyboard.current.sKey.isPressed) inputAvance -= 1f;

            // Rotación izquierda y derecha (A / D)
            if (Keyboard.current.aKey.isPressed) inputRotacion -= 1f;
            if (Keyboard.current.dKey.isPressed) inputRotacion += 1f;
        }
    }

    private void MoverJugador()
    {
        if (controller.isGrounded && velocidadY < 0)
        {
            velocidadY = -2f; // Mantener pegado al suelo
        }

        velocidadY += gravedad * Time.deltaTime;

        // Avanzar/Retroceder en la dirección hacia la que mira el jugador
        Vector3 moveDirection = transform.forward * inputAvance;
        Vector3 desplazamiento = (moveDirection * speed) + (Vector3.up * velocidadY);

        controller.Move(desplazamiento * Time.deltaTime);
    }

    private void RotarJugador()
    {
        if (Mathf.Abs(inputRotacion) > 0.01f)
        {
            transform.Rotate(Vector3.up, inputRotacion * rotationSpeed * Time.deltaTime);
        }
    }

    private void ActualizarAnimaciones()
    {
        if (animator == null) return;

        // Se considera en movimiento si avanza con W/S o gira con A/D
        bool enMovimiento = Mathf.Abs(inputAvance) > 0.01f || Mathf.Abs(inputRotacion) > 0.01f;

        animator.SetBool("IsMoving", enMovimiento);
    }
}