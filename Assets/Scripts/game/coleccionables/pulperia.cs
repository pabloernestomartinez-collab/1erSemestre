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

    public void ComprarDaga()
    {
        ProcesarCompra("Daga", 5, 8); // Tipo, Precio Oro, Daño Extra
    }

    public void ComprarEspada()
    {
        ProcesarCompra("Espada", 10, 15); // Tipo, Precio Oro, Daño Extra
    }

    private void ProcesarCompra(string tipoArma, int precioOro, int danioExtra)
    {
        var netObj = NetworkManager.Singleton?.LocalClient?.PlayerObject;

        if (netObj != null && netObj.TryGetComponent<PlayerStats>(out PlayerStats stats))
        {
            // Validar la secuencia de progresión (Hueso -> Daga -> Espada)
            if (tipoArma == "Daga" && !stats.tieneHueso.Value)
            {
                return;
            }

            if (tipoArma == "Espada" && !stats.tieneDaga.Value)
            {
                return;
            }

            // Validar Fondos
            if (stats.oro.Value >= precioOro)
            {
                // Enviar la petición al servidor para cambiar las NetworkVariables
                stats.ComprarArmaEspecificaServerRpc(tipoArma, precioOro, danioExtra);

                // Reemplazar el ítem en el inventario local para conservar 1 sola arma equipada
                if (netObj.TryGetComponent<InventarioPlayer>(out var inventario))
                {
                    inventario.listaDeItems.Clear(); // Eliminamos el arma anterior

                    itemsMenu nuevaArma = ScriptableObject.CreateInstance<itemsMenu>();
                    nuevaArma.nombreArma = tipoArma;
                    nuevaArma.danioExtra = danioExtra;

                    inventario.AgregarItemLocal(nuevaArma);
                }
            }
            //else
            //{
            //}
        }
    }
}