using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HabilidadEspecial : NetworkBehaviour
{
    [Header("Configuración de la Habilidad")]
    [SerializeField] private float radioAtaque = 5f;
    [SerializeField] private int puntosMaximos = 10;

    [Header("Búsqueda Automática de UI por Nombre")]
    [SerializeField] private string nombreSliderUI = "BarraHabilidad";

    private Slider barraHabilidad;

    // NetworkVariable para sincronizar la carga de la habilidad vía red
    private NetworkVariable<int> puntosHabilidad = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float temporizadorSegundos = 0f;

    public override void OnNetworkSpawn()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (IsOwner)
        {
            BuscarReferenciasUI();
        }

        puntosHabilidad.OnValueChanged += OnCargaCambiada;
        ActualizarUI(puntosHabilidad.Value);
    }

    public override void OnNetworkDespawn()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        puntosHabilidad.OnValueChanged -= OnCargaCambiada;
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
        ActualizarUI(puntosHabilidad.Value);
    }

    private void BuscarReferenciasUI()
    {
        Slider[] slidersEnEscena = Resources.FindObjectsOfTypeAll<Slider>();

        foreach (Slider s in slidersEnEscena)
        {
            if (s.gameObject.scene.name == null) continue;

            if (s.gameObject.name == nombreSliderUI)
            {
                barraHabilidad = s;
                break;
            }
        }
    }

    private void Update()
    {
        //El Servidor incrementa 1 punto por segundo
        if (IsServer)
        {
            if (puntosHabilidad.Value < puntosMaximos)
            {
                temporizadorSegundos += Time.deltaTime;
                if (temporizadorSegundos >= 1f)
                {
                    puntosHabilidad.Value = Mathf.Min(puntosMaximos, puntosHabilidad.Value + 1);
                    temporizadorSegundos = 0f;
                }
            }
        }

        // Tecla 'H' para activar la habilidad especial
        if (IsOwner)
        {
            if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
            {
                if (puntosHabilidad.Value >= puntosMaximos)
                {
                    EjecutarHabilidadServerRpc();
                }
            }
        }
    }

    [ServerRpc]
    private void EjecutarHabilidadServerRpc()
    {
        if (puntosHabilidad.Value < puntosMaximos) return;

        // Resetear la carga a cero
        puntosHabilidad.Value = 0;
        temporizadorSegundos = 0f;

        // Físicas 3D igual que en PlayerAttack.cs
        Collider[] enemigosGolpeados = Physics.OverlapSphere(transform.position, radioAtaque);

        foreach (Collider col in enemigosGolpeados)
        {
            if (col.CompareTag("Enemy"))
            {
                enemy scriptEnemigo = col.GetComponentInParent<enemy>();

                if (scriptEnemigo != null)
                {
                    // Le infligimos 9999 de daño enviándole este gameObject como atacante
                    scriptEnemigo.RecibirDanio(9999, gameObject);
                }
            }
        }
    }

    private void OnCargaCambiada(int valorAnterior, int valorNuevo)
    {
        ActualizarUI(valorNuevo);
    }

    private void ActualizarUI(int cargaActual)
    {
        if (barraHabilidad != null)
        {
            barraHabilidad.maxValue = puntosMaximos;
            barraHabilidad.value = cargaActual;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, radioAtaque);
    }
}