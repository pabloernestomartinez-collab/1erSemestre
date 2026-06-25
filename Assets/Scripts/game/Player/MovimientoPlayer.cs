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

    public override void OnNetworkSpawn()
    {
        // Vinculamos el Rigidbody para que funcione en Servidor y Clientes
        rb = GetComponent<Rigidbody>();

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
        if (!IsOwner) return; // Candado de red[cite: 2]

        // Re-capturar la cámara de Unity automáticamente si cambiamos de escena
        if (mainCameraTransform == null && UnityEngine.Camera.main != null)
        {
            BuscarCamara();
        }

        // 1. LEER INPUTS[cite: 2]
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
            if (Keyboard.current.wKey.isPressed) moveZ = 1f; //[cite: 2]
            if (Keyboard.current.sKey.isPressed) moveZ = -1f; //[cite: 2]
            if (Keyboard.current.aKey.isPressed) moveX = -1f; //[cite: 2]
            if (Keyboard.current.dKey.isPressed) moveX = 1f; //[cite: 2]
        }

        if (rb == null || mainCameraTransform == null) return;

        // 2. CALCULAR VECTORES RESPECTO A LA PERSPECTIVA DE LA CÁMARA
        Vector3 camForward = mainCameraTransform.forward;
        camForward.y = 0f; // Aplanamos el vector al suelo
        camForward = camForward.normalized;

        Vector3 camRight = mainCameraTransform.right;
        camRight.y = 0f;
        camRight = camRight.normalized;

        Vector3 moveDirection = (camForward * moveZ + camRight * moveX).normalized;

        // 3. ROTACIÓN SUAVE INDEPENDIENTE 
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 4. APLICAR VELOCIDAD FÍSICA[cite: 2]
        rb.linearVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed); //[cite: 2]
    }
}