using Unity.Netcode;
using UnityEngine;

public class pulperia : MonoBehaviour
{
    public void AbrirTienda()
    {
        gameObject.SetActive(true);
    }

    public void CerrarTienda()
    {
        gameObject.SetActive(false);
    }


    public void ComprarEspada()
    {
        ProcesarCompra("Espada", 10, 15); // Tipo, Precio Oro, Daño Extra
    }

    public void ComprarDaga()
    {
        ProcesarCompra("Daga", 5, 8); // Tipo, Precio Oro, Daño Extra
    }

    private void ProcesarCompra(string tipoArma, int precioOro, int danioExtra)
    {
        var netObj = NetworkManager.Singleton?.LocalClient?.PlayerObject;

        if (netObj != null && netObj.TryGetComponent<PlayerStats>(out PlayerStats stats))
        {
            if (stats.oro.Value >= precioOro)
            {
                // Envía la compra al servidor especificado SOLO por tipoArma ("Espada" o "Daga")
                stats.ComprarArmaEspecificaServerRpc(tipoArma, precioOro, danioExtra);

                //Agrega SOLO UN ítem al inventario local
                if (netObj.TryGetComponent<InventarioPlayer>(out var inventario))
                {
                    itemsMenu nuevaArma = ScriptableObject.CreateInstance<itemsMenu>();
                    nuevaArma.nombreArma = tipoArma;
                    nuevaArma.danioExtra = danioExtra;

                    inventario.AgregarItemLocal(nuevaArma);
                    Debug.Log($"[KIOSCO] ¡{tipoArma} comprada con éxito!");
                }
            }
            //else
            //{
            //    Debug.LogWarning($"[KIOSCO] No tienes suficiente oro para comprar {tipoArma}.");
            //}
        }
    }
}