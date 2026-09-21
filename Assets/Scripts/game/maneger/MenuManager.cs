using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Paneles")]
    [SerializeField] private GameObject canvasKiosco;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AbrirKiosco()
    {
        if (canvasKiosco != null)
        {
            canvasKiosco.SetActive(true);

            if (canvasKiosco.TryGetComponent<pulperia>(out var scriptPulperia))
            {
                scriptPulperia.AbrirTienda();
            }
            else
            {
                var pulperiaHijo = canvasKiosco.GetComponentInChildren<pulperia>();
                if (pulperiaHijo != null) pulperiaHijo.AbrirTienda();
            }
        }
    }

    public void CerrarTodo()
    {
        if (canvasKiosco != null) canvasKiosco.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}