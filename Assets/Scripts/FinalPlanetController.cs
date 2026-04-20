using UnityEngine;

public class FinalPlanetController : MonoBehaviour
{
    [Header("UI Felicitaciones")]
    [SerializeField] private GameObject panelFelicitaciones;
    [SerializeField] private bool pausarJuego = true;

    void Awake()
    {
        if (panelFelicitaciones != null)
            panelFelicitaciones.SetActive(false);
    }

    public void MostrarFelicitaciones()
    {
        if (panelFelicitaciones != null)
            panelFelicitaciones.SetActive(true);

        if (pausarJuego)
            Time.timeScale = 0f;
    }
}