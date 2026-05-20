using Unity.Netcode;
using UnityEngine;

public class MultiplayerCamera : NetworkBehaviour
{
    // ... tus variables de distancia, sensibilidad, etc.

    // CAMBIO CLAVE: Hacemos que mouseX sea pública para que el script de movimiento la lea
    [HideInInspector] public float mouseX = 0f;
    //private float mouseY = 0f;

    // ... tu método Update que calcula la posición de la cámara usando mouseX y mouseY


    void LateUpdate()
    {
        if (!IsOwner) return;

    }


}