using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PocionManager : NetworkBehaviour
{
    [Header("Costos de Crafteo")]
    [SerializeField] private int costoHierba = 2;
    [SerializeField] private int costoSabiduria = 5;

    [Header("Búsqueda Automática de UI en Escena")]
    [SerializeField] private string nombreImagenPocionUI = "ImagenPocion";
    [SerializeField] private string nombreTextoPocionUI = "TextoPociones";

    // Cantidad de pociones sincronizada en red
    public NetworkVariable<int> cantidadPociones = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Image imagenPocionUI;
    private TextMeshProUGUI textoPocionesUI;
    private PlayerStats misStats;

    public override void OnNetworkSpawn()
    {
        misStats = GetComponent<PlayerStats>();
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (IsOwner)
        {
            BuscarReferenciasUI();
        }

        cantidadPociones.OnValueChanged += OnPocionesChanged;
        ActualizarUI(cantidadPociones.Value);
    }

    public override void OnNetworkDespawn()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        cantidadPociones.OnValueChanged -= OnPocionesChanged;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsOwner)
        {
            StartCoroutine(VincularUIConRetraso());
        }
    }

    private IEnumerator VincularUIConRetraso()
    {
        yield return new WaitForSeconds(0.2f);
        BuscarReferenciasUI();
        ActualizarUI(cantidadPociones.Value);
    }

    private void BuscarReferenciasUI()
    {
        // Buscar Imagen de Poción en la escena (incluso si está inactiva)
        Image[] imagenes = Resources.FindObjectsOfTypeAll<Image>();
        foreach (Image img in imagenes)
        {
            if (img.gameObject.scene.name == null) continue;
            if (img.gameObject.name == nombreImagenPocionUI)
            {
                imagenPocionUI = img;
                break;
            }
        }

        // Buscar Texto de Poción en la escena
        TextMeshProUGUI[] textos = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (TextMeshProUGUI txt in textos)
        {
            if (txt.gameObject.scene.name == null) continue;
            if (txt.gameObject.name == nombreTextoPocionUI)
            {
                textoPocionesUI = txt;
                break;
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Usar Poción con la tecla 'P'
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (cantidadPociones.Value > 0 && misStats != null && misStats.GetVidaActual() < misStats.GetVidaMaxima())
            {
                UsarPocionServerRpc();
            }
            
        }
    }

    // --- MÉTODOS PÚBLICOS DE CRAFTEO (Vincular al Botón del Canvas) ---
    public void IntentarCraftearPocion()
    {
        if (misStats != null)
        {
            if (misStats.GetHierba() >= costoHierba && misStats.GetSabiduria() >= costoSabiduria)
            {
                CraftearPocionServerRpc(costoHierba, costoSabiduria);
            }
            
        }
    }

    [ServerRpc]
    private void CraftearPocionServerRpc(int costoH, int costoS)
    {
        if (misStats != null && misStats.hierba.Value >= costoH && misStats.sabiduria.Value >= costoS)
        {
            // Descontar recursos
            misStats.hierba.Value -= costoH;
            misStats.sabiduria.Value -= costoS;

            // Incrementar pociones
            cantidadPociones.Value++;
        }
    }

    [ServerRpc]
    private void UsarPocionServerRpc()
    {
        if (cantidadPociones.Value > 0 && misStats != null)
        {
            // Descontar 1 poción
            cantidadPociones.Value--;

            // Restaurar vida al 100% (puntosVidaMax)
            misStats.puntosVida.Value = misStats.puntosVidaMax.Value;

        }
    }

    private void OnPocionesChanged(int valorAnterior, int valorNuevo)
    {
        ActualizarUI(valorNuevo);
    }

    private void ActualizarUI(int pociones)
    {
        // Si hay pociones (> 0), se activa la imagen en el inventario. Si es 0, se oculta.
        if (imagenPocionUI != null)
        {
            imagenPocionUI.gameObject.SetActive(pociones > 0);
        }

        if (textoPocionesUI != null)
        {
            textoPocionesUI.text = pociones > 0 ? $"x{pociones}" : "";
        }
    }
}