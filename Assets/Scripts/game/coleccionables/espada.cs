using UnityEngine;

public class espada : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int cantidadEspadas = 1;
    [SerializeField] private float velocidadRotacion = 100f;

    void Update()
    {
        // Hacer que el coleccionable gire sobre su propio eje para que sea vistoso
        transform.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime);
    }

    // Esta función mágica de Unity se activa cuando algo atraviesa el Trigger
    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si el objeto que nos tocó tiene el componente de nuestro Jugador
        if (other.TryGetComponent<PlayerMovement>(out PlayerMovement jugador))
        {
            // 1. Aquí le sumamos los puntos al jugador (crearemos este método en el paso 2)
            jugador.SumarPuntos(cantidadEspadas);

            // 2. Mensaje en consola para verificar que funciona
            Debug.Log($"¡Espada recolectado! +{cantidadEspadas} puntos.");

            // 3. Destruimos el coleccionable de la escena para que no se pueda volver a agarrar
            Destroy(gameObject);
        }
    }
}