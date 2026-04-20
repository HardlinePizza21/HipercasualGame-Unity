using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void JugarJuego()
    {
        Time.timeScale = 1f; // Asegúrate de que el tiempo está habilitado
        SceneManager.LoadScene("planet");
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