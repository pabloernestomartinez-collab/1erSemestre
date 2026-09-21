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
        ProcesarCompra("Espada", 10, 15); // Nombre, Precio, Daño Extra
    }

    public void ComprarDaga()
    {
        ProcesarCompra("Daga", 5, 8); // Nombre, Precio, Daño Extra
    }

    private void ProcesarCompra(string nombreArma, int precioOro, int danioExtra)
    {
        var netObj = NetworkManager.Singleton?.LocalClient?.PlayerObject;

        if (netObj != null && netObj.TryGetComponent<PlayerStats>(out PlayerStats stats))
        {
            if (stats.oro.Value >= precioOro)
            {
                // Enviamos la compra al servidor para actualizar oro, daño, booleano y contador
                stats.ComprarArmaEspecificaServerRpc(nombreArma, precioOro, danioExtra);

                // Notificamos al inventario local
                if (netObj.TryGetComponent<InventarioPlayer>(out var inventario))
                {
                    itemsMenu nuevaArma = ScriptableObject.CreateInstance<itemsMenu>();
                    nuevaArma.nombreArma = nombreArma;
                    nuevaArma.danioExtra = danioExtra;

                    inventario.AgregarItemLocal(nuevaArma);
                }
            }
            //else
            //{
            //    Debug.LogWarning("[KIOSCO] No tienes suficiente oro.");
            //}
        }
    }
}