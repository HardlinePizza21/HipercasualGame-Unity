using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuButton : MonoBehaviour
{
    public void VolverAlMenu()
    {
        Time.timeScale = 1f; // Reanuda el juego antes de cambiar de escena

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.VolverAlMenu();
        }
        else
        {
            SceneManager.LoadScene("menu");
        }
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ReiniciarNivel();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}