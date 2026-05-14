using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem; // Necesario para el nuevo sistema

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;
    private Vector2 moveInput;

    void Update()
    {
        if (!IsOwner) return;

        if (Keyboard.current != null)
        {
            float moveX = 0;
            float moveY = 0;

            if (Keyboard.current.wKey.isPressed) moveY = 1;
            if (Keyboard.current.sKey.isPressed) moveY = -1;
            if (Keyboard.current.aKey.isPressed) moveX = -1;
            if (Keyboard.current.dKey.isPressed) moveX = 1;

            Vector3 move = new Vector3(moveX, 0, moveY).normalized;
            transform.position += move * speed * Time.deltaTime;
        }
    }
}