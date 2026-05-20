using Unity.Netcode;
using UnityEngine;

public class MultiplayerCamera : NetworkBehaviour
{
    [HideInInspector] public float mouseX = 0f;

    void LateUpdate()
    {
        if (!IsOwner) return;

    }


}