using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor global de niveles. Maneja la progresión entre niveles.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private string[] levelScenes = { "planet_level1", "planet_level2" };
    private int currentLevelIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log($"[LevelManager] Creado - Nivel inicial: {currentLevelIndex}");
    }

    /// <summary>
    /// Completa el nivel actual y carga el siguiente
    /// </summary>
    public void CompletarNivel()
    {
        Debug.Log($"[LevelManager] CompletarNivel() llamado - Nivel actual: {currentLevelIndex}");
        
        currentLevelIndex++;

        if (currentLevelIndex < levelScenes.Length)
        {
            Debug.Log($"[LevelManager] Cargando nivel: {levelScenes[currentLevelIndex]} (índice {currentLevelIndex})");
            SceneManager.LoadScene(levelScenes[currentLevelIndex]);
        }
        else
        {
            Debug.Log("[LevelManager] ¡🎉 TODOS LOS NIVELES COMPLETADOS!");
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// Reinicia el nivel actual
    /// </summary>
    public void ReiniciarNivel()
    {
        SceneManager.LoadScene(levelScenes[currentLevelIndex]);
    }

    /// <summary>
    /// Vuelve al menú y reinicia el progreso
    /// </summary>
    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        currentLevelIndex = 0;
        Destroy(gameObject);
        SceneManager.LoadScene("menu");
    }

    /// <summary>
    /// Obtiene el nivel actual (1-indexed para mostrar al usuario)
    /// </summary>
    public int GetNivelActual()
    {
        return currentLevelIndex + 1;
    }

    /// <summary>
    /// Obtiene el total de niveles
    /// </summary>
    public int GetTotalNiveles()
    {
        return levelScenes.Length;
    }
}
