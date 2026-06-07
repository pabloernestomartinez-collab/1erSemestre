using UnityEngine;
[CreateAssetMenu(fileName ="Nuevo Enemigo",menuName ="Enemigos Data")]
public class enemigosData : ScriptableObject //plantilla para cargar los datos de los enemigos
{
    [SerializeField] private string _EnemigoNombre;
    [SerializeField] private string _EnemigoDescripcion;
    [SerializeField] private string _efe3;
    [SerializeField] private string _efe4;
    [SerializeField] private int _EnemigoVelocidad;
    [SerializeField] private int _EnemigoAtaque;
    [SerializeField] private int _EnemigoDefensa;
    [SerializeField] private int _EnemmigoCantidad;
    [SerializeField] private bool _GreenFlag;
    [SerializeField] private bool _YellowFlag;
    [SerializeField] private bool _RedFlag;


    public string EnemigoNombre { get { return _EnemigoNombre; } }
    public string EnemigoDescripcion { get { return _EnemigoDescripcion; } }
    public string efe3 { get { return _efe3; } }
    public string efe4 { get { return _efe4; } }
    public int EnemigoVelocidad { get { return _EnemigoVelocidad; } }
    public int EnemigoVeAtaque { get { return _EnemigoAtaque; } }
    public int EnemigoDefensa { get { return _EnemigoDefensa; } }
    public int EnemmigoCantidad { get { return _EnemmigoCantidad; } }
    public bool GreenFlag { get { return _GreenFlag; } }
    public bool YellowFlag { get { return _YellowFlag; } }
    public bool RedFlag { get { return _RedFlag; } }



}
