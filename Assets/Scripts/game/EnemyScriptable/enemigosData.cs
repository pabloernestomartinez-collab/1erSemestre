using UnityEngine;
[CreateAssetMenu(fileName ="Nuevo Enemigo",menuName ="Enemigos Data")]
public class enemigosData : ScriptableObject //plantilla para cargar los datos de los enemigos
{
    [SerializeField] private string _EnemigoNombre;
    [SerializeField] private string _EnemigoDescripcion;
    [SerializeField] private float _EnemigoVelocidad;
    [SerializeField] private int _EnemigoAtaque;//cantidad de golpes que mata al player
    [SerializeField] private int _EnemigoDefensa;// cantida de golpes que recibe para morir
    [SerializeField] private bool _emelee;
    [SerializeField] private bool _distancia;


    public string EnemigoNombre { get { return _EnemigoNombre; } }
    public string EnemigoDescripcion { get { return _EnemigoDescripcion; } }
    public float EnemigoVelocidad { get { return _EnemigoVelocidad; } }
    public int EnemigoAtaque { get { return _EnemigoAtaque; } }
    public int EnemigoDefensa { get { return _EnemigoDefensa; } }
    public bool Emelee { get { return _emelee; } }
    public bool Distancia { get { return _distancia; } }
}
