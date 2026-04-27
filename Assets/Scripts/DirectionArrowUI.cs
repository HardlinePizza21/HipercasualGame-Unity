using UnityEngine;

public class DirectionArrowUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform rocket;
    [SerializeField] private Transform targetPlanet;

    [Header("Ajustes")]
    [SerializeField] private float distanciaPixeles = 80f;
    [Tooltip("Offset si el sprite no apunta naturalmente hacia la derecha")]
    [SerializeField] private float offsetRotacionZ = 0f;

    private RectTransform rectTransform;
    private Camera mainCamera;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        if (rocket == null || targetPlanet == null) return;

        // 1️⃣ Dirección en espacio mundo (2D)
        Vector3 direccion = targetPlanet.position - rocket.position;
        direccion.z = 0f;

        if (direccion.sqrMagnitude < 0.001f) return;

        direccion.Normalize();

        // 2️⃣ Posicionar: convertir posición del cohete a espacio Canvas,
        //    luego desplazar en la dirección del objetivo
        Vector2 screenPosCohete = RectTransformUtility.WorldToScreenPoint(mainCamera, rocket.position);

        Vector2 posicionUI = screenPosCohete + (Vector2)direccion * distanciaPixeles;

        // Convertir esa posición de pantalla a espacio local del Canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            posicionUI,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out Vector2 localPoint
        );

        rectTransform.localPosition = localPoint;

        // 3️⃣ Rotar para que apunte hacia el objetivo
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.Euler(0f, 0f, angulo + offsetRotacionZ);
    }
}