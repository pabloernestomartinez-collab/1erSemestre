using Unity.Netcode;
using UnityEngine;

public class ProyectilEnemigo : NetworkBehaviour
{
    private float velocidadProyectil = 15f;
    private float tiempoDeVida = 4f;

    private int danioProyectil;

    // recibe el daño y el collider del enemigo que lo lanza
    public void ConfigurarProyectil(int danio, Collider colliderAtacante)
    {
        danioProyectil = danio;

        // Desactivamos físicamente que el proyectil choque con el propio enemigo que lo creó
        if (colliderAtacante != null && TryGetComponent<Collider>(out Collider miCollider))
        {
            Physics.IgnoreCollision(miCollider, colliderAtacante);
        }
    }

    void Update()
    {
        transform.Translate(Vector3.forward * velocidadProyectil * Time.deltaTime);

        if (!IsServer) return;

        tiempoDeVida -= Time.deltaTime;

        if (tiempoDeVida <= 0)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        GameObject objetoChocado = collision.gameObject;

        // Si choca con el Jugador, hace daño y se destruye
        if (objetoChocado.CompareTag("Player"))
        {
            if (objetoChocado.TryGetComponent<PlayerStats>(out PlayerStats stats))
            {
                stats.RecibirDanio(danioProyectil);
            }

            GetComponent<NetworkObject>().Despawn();
            return;
        }

        if (objetoChocado.CompareTag("solido"))
        {
            GetComponent<NetworkObject>().Despawn();
            return;
        }

    }
}