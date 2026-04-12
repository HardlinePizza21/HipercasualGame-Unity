using UnityEngine;

/// <summary>
/// Asigna en el Inspector el panel (GameObject) con el texto "Perdiste".
/// Ese panel debe estar desactivado al inicio o este script lo oculta en Awake.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [SerializeField] GameObject panelPerdiste;
    [SerializeField] bool pausarJuego = true;

    void Awake()
    {
        if (panelPerdiste != null)
            panelPerdiste.SetActive(false);
    }

    public void MostrarPerdiste()
    {
        if (panelPerdiste != null)
            panelPerdiste.SetActive(true);

        if (pausarJuego)
            Time.timeScale = 0f;
    }
}
