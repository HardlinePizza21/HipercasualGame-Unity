using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuButton : MonoBehaviour
{
    public void VolverAlMenu()
    {
        Time.timeScale = 1f; // Reanuda el juego antes de cambiar de escena
        SceneManager.LoadScene("menu");
    }
}