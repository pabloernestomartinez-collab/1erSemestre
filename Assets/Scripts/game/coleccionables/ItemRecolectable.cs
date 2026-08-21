using Unity.Netcode;
using UnityEngine;

public class ItemRecolectable : NetworkBehaviour
{
    public enum TipoItem { Hierro, Madera, Fuego, Agua, Piedra }//espada=hierro+fuego+agua escudo=madera+piedra

    [Header("Configuración del Item")]
    [SerializeField] private TipoItem tipoDeItem;

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
                    case TipoItem.Hierro: stats.SumarHierro(); break;
                    case TipoItem.Madera: stats.SumarMadera(); break;
                    case TipoItem.Fuego: stats.SumarFuego(); break;
                    case TipoItem.Agua: stats.SumarAgua(); break;
                    case TipoItem.Piedra: stats.SumarPiedra(); break;
                }

                // Despawn de red: desaparece para todos de forma sincronizada
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}