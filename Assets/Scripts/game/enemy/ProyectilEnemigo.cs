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

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        GameObject objetoChocado = collision.gameObject;

        if (objetoChocado.CompareTag("Player"))
        {
            if (objetoChocado.TryGetComponent<PlayerStats>(out PlayerStats stats))
            {
                stats.RecibirDanio(danioProyectil);
            }

            GetComponent<NetworkObject>().Despawn();
            return;
        }

        if (!objetoChocado.CompareTag("Enemy"))        // Ignoramos si choca con el propio enemigo que lo disparó

        {
            Debug.Log($"💥 Proyectil destruido físicamente por chocar contra: {objetoChocado.name}");
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
