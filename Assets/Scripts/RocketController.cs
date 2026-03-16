using UnityEngine;
using UnityEngine.InputSystem;

public class RocketController : MonoBehaviour
{
    [Header("Fuerza")]
    public float forceMultiplier = 0.01f;
    public float minForce = 1f;
    public float maxForce = 50f;
    public float maxDragDistance = 200f;

    [Header("Línea de dirección")]
    public float lineMaxLength = 3f;      // longitud máxima en world units
    public Color lineColorMin = Color.green;
    public Color lineColorMax = Color.red;

    private Vector2 startPosition;
    private bool isDragging = false;
    private Rigidbody2D rb;
    private Camera mainCamera;
    private BoxCollider2D boxCollider;
    private LineRenderer lineRenderer;

    void Awake()
    {
        rb           = GetComponent<Rigidbody2D>();
        boxCollider  = GetComponent<BoxCollider2D>();
        mainCamera   = Camera.main;
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer == null)
            Debug.LogError("[RocketController] ❌ No se encontró LineRenderer en " + gameObject.name);

        HideLine();
    }

    // ─── Input ────────────────────────────────────────────────────────────────

    public void OnClickPressed(InputValue value)
    {
        if (!isLanded)
        {
            Debug.Log("[Press] No está aterrizado — ignorado.");
            return;
        }

        // ✅ Guardar donde clickeó el mouse, no donde está el cohete
        startPosition = Mouse.current.position.ReadValue();
        isDragging    = true;
        Debug.Log("[Press] Drag iniciado en: " + startPosition);
    }

    public void OnClickReleased(InputValue value)
    {
        if (!isDragging) return;

        Vector2 endPosition = Mouse.current.position.ReadValue();
        Vector2 finalForce  = CalculateForce(startPosition, endPosition);

        transform.SetParent(null);

        rb.isKinematic = false;
        isLanded       = false;
        landedPlanet   = null;
        lastLandTime   = Time.time;  // ✅ Cooldown arranca desde el lanzamiento también

        rb.AddForce(finalForce, ForceMode2D.Impulse);

        isDragging = false;
        HideLine();
    }

    public void OnPointer(InputValue value)
    {
        if (!isDragging) return;

        Vector2 currentScreenPos = value.Get<Vector2>();
        UpdateLine(startPosition, currentScreenPos);
    }

    void Update()
    {
        if (!isDragging) return;

        Vector2 currentScreenPos = Mouse.current.position.ReadValue();
        // ✅ Recalcular desde la posición actual del objeto, no startPosition guardado
        Vector2 objectScreenPos  = mainCamera.WorldToScreenPoint(transform.position);
        Vector2 rawDelta         = objectScreenPos - currentScreenPos;
        Vector2 clampedDelta     = Vector2.ClampMagnitude(rawDelta, maxDragDistance);
        float   t                = clampedDelta.magnitude / maxDragDistance;

        Debug.DrawLine(transform.position, transform.position + (Vector3)(clampedDelta.normalized * t * lineMaxLength), Color.red);
        Debug.DrawLine(Vector3.zero, transform.position, Color.yellow);
    }

    // ─── Fuerza ───────────────────────────────────────────────────────────────

    Vector2 CalculateForce(Vector2 screenStart, Vector2 screenEnd)
    {
        Vector2 rawDelta      = screenStart - screenEnd;
        Vector2 clampedDelta  = Vector2.ClampMagnitude(rawDelta, maxDragDistance);
        float   t             = clampedDelta.magnitude / maxDragDistance;
        float   forceMagnitude = Mathf.Lerp(minForce, maxForce, t);

        return clampedDelta.normalized * forceMagnitude;
    }

    // ─── LineRenderer ─────────────────────────────────────────────────────────

    void UpdateLine(Vector2 screenStart, Vector2 screenEnd)
    {
        Vector2 rawDelta     = screenStart - screenEnd;
        Vector2 clampedDelta = Vector2.ClampMagnitude(rawDelta, maxDragDistance);
        float   t            = clampedDelta.magnitude / maxDragDistance;

        // Convertir solo el DELTA a world space usando dos puntos relativos al centro de pantalla
        float objectZ = transform.position.z - mainCamera.transform.position.z;
        Vector3 zero  = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, objectZ));
        Vector3 delta = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f + clampedDelta.x, Screen.height / 2f + clampedDelta.y, objectZ));

        Vector3 directionWorld = (delta - zero).normalized;

        // ✅ Siempre empieza en el objeto
        Vector3 pointA = new Vector3(transform.position.x, transform.position.y, 0f);
        Vector3 pointB = new Vector3(pointA.x + directionWorld.x * lineMaxLength * t,
                                    pointA.y + directionWorld.y * lineMaxLength * t,
                                    0f);

        lineRenderer.SetPosition(0, pointA);
        lineRenderer.SetPosition(1, pointB);

        lineRenderer.startColor = Color.Lerp(lineColorMin, lineColorMax, t);
        lineRenderer.endColor   = Color.Lerp(lineColorMin, lineColorMax, t);
        lineRenderer.enabled    = true;

        Debug.Log($"[UpdateLine] t: {t:F2} | pointA: {pointA} | pointB: {pointB} | enabled: {lineRenderer.enabled}");
    }

    void HideLine()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    [Header("Aterrizaje")]
    public bool isLanded = false;
    public float landCooldown = 0.5f;  // ← ajustable desde Inspector
    private Transform landedPlanet = null;
    private float lastLandTime  = -999f;    // timestamp del último aterrizaje

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Planet")) return;

        // ✅ Ignorar colisión si el cooldown no ha pasado
        if (Time.time - lastLandTime < landCooldown)
        {
            Debug.Log($"[Colisión] Ignorada — cooldown activo ({Time.time - lastLandTime:F2}s / {landCooldown}s)");
            return;
        }

        Debug.Log("[Colisión] Chocó con planeta: " + collision.gameObject.name);

        ContactPoint2D contact = collision.GetContact(0);
        Vector2 surfaceNormal  = contact.normal;

        LandOnPlanet(collision.transform, surfaceNormal);
    }

    void LandOnPlanet(Transform planet, Vector2 surfaceNormal)
    {
        rb.linearVelocity  = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic     = true;

        float angle = Mathf.Atan2(surfaceNormal.y, surfaceNormal.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        transform.position = transform.position + (Vector3)(surfaceNormal * 0.1f);

        transform.SetParent(planet);

        landedPlanet  = planet;
        isLanded      = true;
        lastLandTime  = Time.time;  // ✅ Registrar el momento del aterrizaje

        Debug.Log($"[Aterrizaje] Aterrizado en {planet.name} | Ángulo: {angle:F1}°");
    }
}

