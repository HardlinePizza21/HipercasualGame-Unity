using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class FinalPlanetController : MonoBehaviour
{
    [Header("UI Felicitaciones")]
    [SerializeField] private GameObject panelFelicitaciones;
    [SerializeField] private bool pausarJuego = true;
    [SerializeField] private float delayCargaNivelSiguiente = 2f;

    [Header("Barra de Carga")]
    [SerializeField] private Image barraProgreso;
    [SerializeField] private TextMeshProUGUI textoProgreso;

    void Awake()
    {
        if (panelFelicitaciones != null)
            panelFelicitaciones.SetActive(false);

        if (barraProgreso != null)
            barraProgreso.fillAmount = 0f;
    }

    public void MostrarFelicitaciones()
    {
        if (panelFelicitaciones != null)
            panelFelicitaciones.SetActive(true);

        if (pausarJuego)
            Time.timeScale = 0f;

        // 🎉 Cargar el siguiente nivel después de X segundos
        StartCoroutine(CargarNivelSiguiente());
    }

    IEnumerator CargarNivelSiguiente()
    {
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < delayCargaNivelSiguiente)
        {
            // 🔑 IMPORTANTE: Usar unscaledDeltaTime para que funcione aunque Time.timeScale = 0
            tiempoTranscurrido += Time.unscaledDeltaTime;

            // Actualizar barra de progreso
            float progreso = tiempoTranscurrido / delayCargaNivelSiguiente;
            if (barraProgreso != null)
                barraProgreso.fillAmount = Mathf.Clamp01(progreso);

            if (textoProgreso != null)
                textoProgreso.text = $"Cargando... {(progreso * 100f):F0}%";

            yield return null;
        }

        // 🔄 IMPORTANTE: Restaurar Time.timeScale ANTES de cambiar de escena
        Time.timeScale = 1f;

        Debug.Log($"[FinalPlanetController] LevelManager.Instance: {LevelManager.Instance}");
        Debug.Log("[FinalPlanetController] Intentando cargar siguiente nivel...");

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CompletarNivel();
        }
        else
        {
            Debug.LogError("[FinalPlanetController] ❌ LevelManager no encontrado!");
        }
    }
}