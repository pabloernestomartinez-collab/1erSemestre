using Unity.Netcode;
using UnityEngine;

public class Management : MonoBehaviour
{
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host (Jugador 1)")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client (Jugador 2)")) NetworkManager.Singleton.StartClient();
        }
        GUILayout.EndArea();
    }
}