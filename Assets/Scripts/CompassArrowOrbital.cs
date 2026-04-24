using UnityEngine;

/// <summary>
/// Flecha UI que orbita alrededor del cohete apuntando hacia el objetivo final.
/// </summary>
public class CompassArrowOrbital : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform rocket;
    [SerializeField] private Transform targetPlanet;

    [Header("Ajustes")]
    [SerializeField] private float distanciaAlCohete = 80f; // En píxeles
    [SerializeField] private float velocidadRotacion = 1f; // Vueltas por segundo
    [SerializeField] private Canvas canvas;

    private float anguloOrbita = 0f;
    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        
        if (canvas != null)
            canvasRectTransform = canvas.GetComponent<RectTransform>();
    }

    void Update()
    {
        if (rocket == null || targetPlanet == null || rectTransform == null || canvasRectTransform == null)
            return;

        // 🔄 PASO 1: Calcular dirección hacia el objetivo (mundo)
        Vector3 direccionAlObjetivo = targetPlanet.position - rocket.position;
        direccionAlObjetivo.z = 0f;

        if (direccionAlObjetivo.sqrMagnitude <= 0.0001f)
            return;

        // 🎯 PASO 2: Calcular ángulo hacia el objetivo
        float anguloAlObjetivo = Mathf.Atan2(direccionAlObjetivo.y, direccionAlObjetivo.x) * Mathf.Rad2Deg;

        // 🔄 PASO 3: Orbitar alrededor del cohete
        anguloOrbita += velocidadRotacion * 360f * Time.deltaTime;
        float anguloTotal = anguloOrbita + anguloAlObjetivo;
        
        float radianes = anguloTotal * Mathf.Deg2Rad;
        Vector2 offsetOrbita = new Vector2(
            Mathf.Cos(radianes) * distanciaAlCohete,
            Mathf.Sin(radianes) * distanciaAlCohete
        );

        // Asignar posición relativa en el Canvas (anchoredPosition)
        rectTransform.anchoredPosition = offsetOrbita;

        // 🎯 PASO 4: Rotar la flecha para que apunte al objetivo
        rectTransform.rotation = Quaternion.Euler(0f, 0f, anguloAlObjetivo);
    }
}


