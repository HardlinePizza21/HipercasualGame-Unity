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
        SumarPuntos(puntosPorAterrizaje);
    }

    /// <summary>
    /// Suma puntos genéricos (por ejemplo monedas) sin afectar el sistema actual.
    /// </summary>
    public void SumarPuntos(int cantidad)
    {
        _total += cantidad;
        if (_total < 0)
            _total = 0;

        // Notificar al LevelManager para acumular entre niveles
        if (LevelManager.Instance != null)
            LevelManager.Instance.AgregarPuntos(cantidad);

        ActualizarTexto();
    }

    void ActualizarTexto()
    {
        if (textoPuntos != null)
            textoPuntos.text = string.Format(formato, _total);
    }
}
