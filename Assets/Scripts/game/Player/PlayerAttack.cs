using System.Collections; // 🔥 Necesario para usar Corrutinas (IEnumerator)
using Unity.Netcode;
using UnityEngine;

public class PlayerAttack : NetworkBehaviour
{
    [Header("Configuración del Ataque")]
    [SerializeField] private float rangoAtaque = 2.5f;     // Qué tan lejos llega el golpe
    [SerializeField] private float cooldownAtaque = 0.6f;   // Tiempo entre ataques
    [SerializeField] private Transform puntoAtaque;         // Objeto vacío al frente del jugador

    [Header("Señal Visual de Ataque")]
    // 🔥 ARRASTRA AQUÍ EL OBJETO HIJO (puedes usar un cubo translúcido, efecto de espada, etc.)
    [SerializeField] private GameObject senalVisualGolpe;
    [SerializeField] private float duracionSenalVisual = 0.15f; // Cuánto tiempo se queda prendido en pantalla

    private float tiempoSiguienteAtaque = 0f;
    private PlayerStats misStats;

    public override void OnNetworkSpawn()
    {
        misStats = GetComponent<PlayerStats>();

        // Aseguramos que la señal empiece apagada en todas las pantallas
        if (senalVisualGolpe != null)
        {
            senalVisualGolpe.SetActive(false);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        // Mouse.current.leftButton.wasPressedThisFrame detecta el clic izquierdo del nuevo sistema
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame && Time.time >= tiempoSiguienteAtaque)
        {
            tiempoSiguienteAtaque = Time.time + cooldownAtaque;

            int danioAInfligir = misStats != null ? misStats.GetDanioMelee() : 10;

            SolicitarAtaqueServerRpc(puntoAtaque.position, danioAInfligir);
        }
    }

    // El Cliente le envía al Servidor los datos exactos del golpe
    [ServerRpc]
    private void SolicitarAtaqueServerRpc(Vector3 posicionDelGolpe, int danio)
    {
        // 🔥 LE AVISAMOS A TODOS LOS CLIENTES QUE ENCIENDAN LA SEÑAL VISUAL DE ESTE JUGADOR
        ControlarVisualAtaqueClientRpc(true);

        Collider[] enemigosGolpeados = Physics.OverlapSphere(posicionDelGolpe, rangoAtaque);

        foreach (Collider col in enemigosGolpeados)
        {
            if (col.CompareTag("Enemy"))
            {
                enemy scriptEnemigo = col.GetComponentInParent<enemy>();

                if (scriptEnemigo != null)
                {
                    // 🔥 CONTROLADO Y ACTUALIZADO: Pasamos el daño Y ADEMÁS el GameObject de este jugador (gameObject)
                    scriptEnemigo.RecibirDanio(danio, gameObject);
                }
            }
        }

        // 🔥 Iniciamos el temporizador en el servidor para apagar el efecto
        StartCoroutine(ApagarSenalVisualDespuesDeTiempo());
    }

    // Corrutina en el servidor para esperar y enviar la orden de apagado
    private IEnumerator ApagarSenalVisualDespuesDeTiempo()
    {
        yield return new WaitForSeconds(duracionSenalVisual);
        ControlarVisualAtaqueClientRpc(false); // Le avisa a todos que se apague
    }

    // 🔥 RPC DE CLIENTE: El servidor fuerza a todas las pantallas a prender/apagar el objeto visual
    [ClientRpc]
    private void ControlarVisualAtaqueClientRpc(bool activar)
    {
        if (senalVisualGolpe != null)
        {
            senalVisualGolpe.SetActive(activar);
        }
    }

    // Dibuja una esfera roja en el editor para que puedas calibrar el rango del golpe a ojo
    private void OnDrawGizmosSelected()
    {
        if (puntoAtaque == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoAtaque.position, rangoAtaque);
    }
}