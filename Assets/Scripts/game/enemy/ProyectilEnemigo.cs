using Unity.Netcode;
using UnityEngine;

public class ProyectilEnemigo : NetworkBehaviour
{
    [SerializeField] private float velocidadProyectil = 15f;
    [SerializeField] private float tiempoDeVida = 4f; // Se destruye solo si no le pega a nada

    private int danioProyectil;

    public void ConfigurarProyectil(int danio)
    {
        danioProyectil = danio; // El script del enemigo le inyecta el daño aquí al nacer

    }

    void Update()
    {
        transform.Translate(Vector3.forward * velocidadProyectil * Time.deltaTime); // El movimiento lineal lo calcula tanto el servidor como el cliente


        if (!IsServer) return;// El servidor controla el reloj para borrarlo

        tiempoDeVida -= Time.deltaTime;  

        if (tiempoDeVida <= 0)
        {
            GetComponent<NetworkObject>().Despawn(); // Borrado limpio en red
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;


        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerStats>(out PlayerStats stats))  // if le pegamos al jugador, buscamos sus Stats de vida

            {
                stats.RecibirDanio(danioProyectil);
            }

            GetComponent<NetworkObject>().Despawn();            // Una vez que impacta al jugador, el proyectil desaparece de la red

        }
    }
}
