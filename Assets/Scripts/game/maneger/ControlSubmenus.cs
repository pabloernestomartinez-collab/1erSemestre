using UnityEngine;
using UnityEngine.InputSystem;

public class ControlSubmenus : MonoBehaviour
{
    [Header("Submenús (Arrastra los 4 objetos contenedores aquí)")]
    [SerializeField] private GameObject[] submenus;

    void Update()
    {


        if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            AlternarSubmenu(0);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
        {
            AlternarSubmenu(1);
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
        {
            AlternarSubmenu(2);
        }
        else if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame)
        {
            AlternarSubmenu(3);
        }
    }

    private void AlternarSubmenu(int indice)
    {
        if (indice < 0 || indice >= submenus.Length || submenus[indice] == null) return;

        // Invertimos el estado de activación del submenú
        bool estaActivo = submenus[indice].activeSelf;
        submenus[indice].SetActive(!estaActivo);
    }
}