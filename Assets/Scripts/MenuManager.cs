using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public void JugarJuego()
    {
        Time.timeScale = 1f;
        StartCoroutine(IniciarJuego());
    }

    IEnumerator IniciarJuego()
    {
        // Si no existe LevelManager, lo creamos
        if (LevelManager.Instance == null)
        {
            GameObject levelManagerGO = new GameObject("LevelManager");
            levelManagerGO.AddComponent<LevelManager>();
            
            // Esperar a que se inicialice
            yield return null;
            
            Debug.Log($"[MenuManager] ✅ LevelManager creado: {LevelManager.Instance}");
        }
        else
        {
            Debug.Log("[MenuManager] LevelManager ya existe");
        }

        // Cargar el primer nivel
        Debug.Log("[MenuManager] 🚀 Cargando planet_level1...");
        SceneManager.LoadScene("planet_level1");
    }

    public void CerrarJuego()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

