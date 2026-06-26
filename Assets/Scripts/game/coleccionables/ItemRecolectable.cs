using Unity.Netcode;
using UnityEngine;

public class ItemRecolectable : NetworkBehaviour
{
    public enum TipoItem { Espada, Escudo, Coleccionable222, Coleccionable224 }

    [Header("Configuración del Item")]
    [SerializeField] private TipoItem tipoDeItem;

    // 💥 CAMBIO CLAVE: Cambiamos a OnCollisionEnter para colisiones sólidas
    private void OnCollisionEnter(Collision collision)
    {
        // REGLA DE ORO: Solo el servidor procesa la recolección
        if (!IsServer) return;

        // Comprobamos si lo que nos chocó es un jugador usando collision.gameObject
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats stats = collision.gameObject.GetComponent<PlayerStats>();

            if (stats != null)
            {
                // Según el tipo de ítem asignado en el Inspector, llamamos a su función
                switch (tipoDeItem)
                {
                    case TipoItem.Espada: stats.SumarEspada(); break;
                    case TipoItem.Escudo: stats.SumarEscudo(); break;
                    case TipoItem.Coleccionable222: stats.Sumar222(); break;
                    case TipoItem.Coleccionable224: stats.Sumar224(); break;
                }

                // Despawn de red: desaparece para todos de forma sincronizada
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}