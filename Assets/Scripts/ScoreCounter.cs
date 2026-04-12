using UnityEngine;
using UnityEngine.UI;

public class ScoreCounter : MonoBehaviour
{
    [Header("Puntuación")]
    [SerializeField] int puntosPorAterrizaje = 10;
    [SerializeField] Text textoPuntos;
    [Tooltip("Usa {0} donde debe ir el número. Ejemplo: \"Puntos: {0}\"")]
    [SerializeField] string formato = "Puntos: {0}";

    int _total;

    public int PuntuacionTotal => _total;

    void Start()
    {
        ActualizarTexto();
    }

    /// <summary>Llama esto cada vez que el cohete aterriza correctamente en un planeta.</summary>
    public void RegistrarAterrizajeEnPlaneta()
    {
        _total += puntosPorAterrizaje;
        ActualizarTexto();
    }

    void ActualizarTexto()
    {
        if (textoPuntos != null)
            textoPuntos.text = string.Format(formato, _total);
    }
}
