using Unity.Netcode;
using UnityEngine;

public class MultiplayerCamera : NetworkBehaviour
{
    [Header("Configuración de Seguimiento")]
    private Transform camaraPrincipal; // Referencia a la cámara física de la escena
    public Vector3 offset = new Vector3(0f, 2f, -4f); // Distancia detrás y arriba del jugador
    public float suavizado = 5f; // Qué tan suave sigue la cámara al jugador

    [Header("Configuración de Rotación (Mouse)")]
    public float sensibilidadMouse = 100f;
    [HideInInspector] public float mouseX = 0f;
    private float rotacionY = 0f;

    private void Start()
    {
        // 🔍 CANDADO SUPREMO MULTI-JUGADOR:
        // Si este clon del jugador NO es el que yo controlo con mi teclado/mouse,
        // desactivamos este script por completo. Solo el jugador local mueve la cámara.
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }

        // Buscamos la cámara principal que ya existe en la escena de Unity
        if (Camera.main != null)
        {
            camaraPrincipal = Camera.main.transform;
        }
        else
        {
            Debug.LogError("🚨 [Cámara] ¡No se encontró ninguna cámara con el Tag 'MainCamera' en la escena!");
        }

        // Opcional: Bloquea el cursor en el centro de la pantalla para que no se salga al girar
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Usamos LateUpdate para el seguimiento de cámaras. 
    // Corre justo después de que el jugador se movió en el Update, evitando tirones (jittering).
    private void LateUpdate()
    {
        // Doble seguridad por si la cámara no se vinculó correctamente
        if (camaraPrincipal == null) return;

        // 1. ROTACIÓN CON EL MOUSE
        // Leemos el movimiento horizontal del mouse
        mouseX = Input.GetAxis("Mouse X") * sensibilidadMouse * Time.deltaTime;
        rotacionY += mouseX;

        // Rotamos el cuerpo del jugador sobre el eje Y (izquierda/derecha)
        transform.rotation = Quaternion.Euler(0f, rotacionY, 0f);


        // 2. SEGUIMIENTO SUAVE DE POSICIÓN
        // Calculamos la posición ideal en base a dónde mira el jugador aplicando el offset
        Vector3 posicionDeseada = transform.position + (transform.rotation * offset);

        // Interpolamos (Lerp) de la posición actual de la cámara a la deseada para que sea fluido
        Vector3 posicionSuave = Vector3.Lerp(camaraPrincipal.position, posicionDeseada, suavizado * Time.deltaTime);

        // Aplicamos la posición final a la cámara
        camaraPrincipal.position = posicionSuave;

        // Hacemos que la cámara mire fijamente hacia el centro del jugador
        camaraPrincipal.LookAt(transform.position + Vector3.up * (offset.y * 0.5f));
    }
}