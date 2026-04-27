using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private string[] levelScenes = { "planet_level1", "planet_level2", "planet_level3", "planet_level4", "planet_level5" };
    private int currentLevelIndex = 0;

    // ─── PUNTUACIÓN ────────────────────────
    private int puntosAcumulados = 0;
    private int record = 0;

    public int PuntosAcumulados => puntosAcumulados;
    public int Record => record;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cargar record guardado
        record = PlayerPrefs.GetInt("Record", 0);
    }

    /// <summary>Llamado por ScoreCounter al sumar puntos</summary>
    public void AgregarPuntos(int cantidad)
    {
        puntosAcumulados += cantidad;
    }

    /// <summary>Reinicia puntos de la run actual (al morir)</summary>
    public void ReiniciarPuntos()
    {
        puntosAcumulados = 0;
    }

    /// <summary>Guarda record si los puntos actuales lo superan</summary>
    private void IntentarGuardarRecord()
    {
        if (puntosAcumulados > record)
        {
            record = puntosAcumulados;
            PlayerPrefs.SetInt("Record", record);
            PlayerPrefs.Save();
        }
    }

    public void CompletarNivel()
    {
        currentLevelIndex++;

        if (currentLevelIndex < levelScenes.Length)
        {
            SceneManager.LoadScene(levelScenes[currentLevelIndex]);
        }
        else
        {
            // Todos los niveles completados — guardar record
            IntentarGuardarRecord();
            Debug.Log("[LevelManager] ¡🎉 TODOS LOS NIVELES COMPLETADOS!");
            Time.timeScale = 0f;
        }
    }
    public void VolverAlMenuVictoriaConDelay(float delay)
    {
        StartCoroutine(RoutineVictoria(delay));
    }

    private IEnumerator RoutineVictoria(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // ← ignora timeScale
        IntentarGuardarRecord();
        ReiniciarPuntos();
        Time.timeScale = 1f;
        currentLevelIndex = 0;
        Destroy(gameObject);
        SceneManager.LoadScene("menu");
    }


    public void ReiniciarNivel()
    {
        SceneManager.LoadScene(levelScenes[currentLevelIndex]);
    }

    public void VolverAlMenu()
    {
        // Al morir se llama esto — reiniciar puntos y volver
        ReiniciarPuntos();
        Time.timeScale = 1f;
        currentLevelIndex = 0;
        Destroy(gameObject);
        SceneManager.LoadScene("menu");
    }

    public void VolverAlMenuConDelay(float delay)
    {
        StartCoroutine(RoutineVolverAlMenu(delay));
    }

    private IEnumerator RoutineVolverAlMenu(float delay)
    {
        yield return new WaitForSeconds(delay);
        VolverAlMenu();
    }

    public int GetNivelActual() => currentLevelIndex + 1;
    public int GetTotalNiveles() => levelScenes.Length;
}