using UnityEngine;
using TMPro;

/// <summary>
/// Muestra el nivel actual en la pantalla.
/// Asigna este script a un TextMeshPro en tu UI.
/// </summary>
public class LevelDisplay : MonoBehaviour
{
    private TextMeshProUGUI levelText;

    void Start()
    {
        levelText = GetComponent<TextMeshProUGUI>();
        ActualizarTexto();
    }

    void ActualizarTexto()
    {
        if (LevelManager.Instance != null)
        {
            int nivelActual = LevelManager.Instance.GetNivelActual();
            int totalNiveles = LevelManager.Instance.GetTotalNiveles();
            levelText.text = $"Nivel {nivelActual}/{totalNiveles}";
        }
    }
}
