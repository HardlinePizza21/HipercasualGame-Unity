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
    public float lineMaxLength = 15f;
    public float lineMinLength = 5f;
    public Color lineColorMin = Color.green;
    public Color lineColorMax = Color.red;

    [Header("Aterrizaje")]
    public bool  isLanded     = false;
    public float landCooldown = 0.5f;

    private Vector2   startPosition;
    private bool      isDragging         = false;
    private Rigidbody rb;
    private Camera    mainCamera;
    private CapsuleCollider capsuleCollider;
    private LineRenderer    lineRenderer;

    private Transform landedPlanet       = null;
    private int       lastLandedPlanetID = -1;
    private float     lastLandTime       = -999f;  // solo aplica al planeta de despegue

    void Awake()
    {
        rb              = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        mainCamera      = Camera.main;
        lineRenderer    = GetComponent<LineRenderer>();

        if (rb == null)
            Debug.LogError("[RocketController] ❌ No se encontró Rigidbody en " + gameObject.name);
        if (lineRenderer == null)
            Debug.LogError("[RocketController] ❌ No se encontró LineRenderer en " + gameObject.name);

        rb.constraints = RigidbodyConstraints.FreezePositionZ
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY;

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

        startPosition = Mouse.current.position.ReadValue();
        isDragging    = true;
        Debug.Log("[Press] Drag iniciado en: " + startPosition);
    }

    public void OnClickReleased(InputValue value)
    {
        if (!isDragging) return;

        Vector2 endPosition = Mouse.current.position.ReadValue();
        Vector3 finalForce  = CalculateForce(startPosition, endPosition);

        transform.SetParent(null);

        rb.isKinematic = false;
        isLanded       = false;
        lastLandTime   = Time.time;  // cooldown arranca al despegar
        landedPlanet   = null;
        // ✅ lastLandedPlanetID se mantiene para el cooldown

        rb.AddForce(finalForce, ForceMode.Impulse);

        isDragging = false;
        HideLine();

        Debug.Log($"[Release] Fuerza: {finalForce} | Magnitud: {finalForce.magnitude:F1}");
    }

    public void OnPointer(InputValue value)
    {
        if (!isDragging) return;
        UpdateLine(startPosition, value.Get<Vector2>());
    }

    void Update()
    {
        if (!isDragging) return;

        Vector2 currentScreenPos = Mouse.current.position.ReadValue();
        Vector2 objectScreenPos  = mainCamera.WorldToScreenPoint(transform.position);
        Vector2 rawDelta         = objectScreenPos - currentScreenPos;
        Vector2 clampedDelta     = Vector2.ClampMagnitude(rawDelta, maxDragDistance);
        float   t                = clampedDelta.magnitude / maxDragDistance;

        Debug.DrawLine(transform.position, transform.position + (Vector3)(clampedDelta.normalized * t * lineMaxLength), Color.red);
        Debug.DrawLine(Vector3.zero, transform.position, Color.yellow);
    }

    // ─── Fuerza ───────────────────────────────────────────────────────────────

    Vector3 CalculateForce(Vector2 screenStart, Vector2 screenEnd)
    {
        Vector2 rawDelta       = screenStart - screenEnd;
        Vector2 clampedDelta   = Vector2.ClampMagnitude(rawDelta, maxDragDistance);
        float   t              = clampedDelta.magnitude / maxDragDistance;
        float   forceMagnitude = Mathf.Lerp(minForce, maxForce, t);

        float   objectZ        = transform.position.z - mainCamera.transform.position.z;
        Vector3 zero           = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, objectZ));
        Vector3 delta          = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f + clampedDelta.x, Screen.height / 2f + clampedDelta.y, objectZ));
        Vector3 directionWorld = (delta - zero).normalized;
        directionWorld.z       = 0f;

        return directionWorld.normalized * forceMagnitude;
    }

    // ─── LineRenderer ─────────────────────────────────────────────────────────

    void UpdateLine(Vector2 screenStart, Vector2 screenEnd)
    {
        Vector2 rawDelta     = screenStart - screenEnd;
        Vector2 clampedDelta = Vector2.ClampMagnitude(rawDelta, maxDragDistance);
        float   t            = clampedDelta.magnitude / maxDragDistance;

        float   objectZ        = transform.position.z - mainCamera.transform.position.z;
        Vector3 zero           = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, objectZ));
        Vector3 delta          = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f + clampedDelta.x, Screen.height / 2f + clampedDelta.y, objectZ));
        Vector3 directionWorld = (delta - zero).normalized;
        directionWorld.z       = 0f;

        Vector3 pointA = new Vector3(transform.position.x, transform.position.y, 0f);
        float lineLength = Mathf.Lerp(lineMinLength, lineMaxLength, t);
        Vector3 pointB   = pointA + directionWorld.normalized * lineLength;

        lineRenderer.SetPosition(0, pointA);
        lineRenderer.SetPosition(1, pointB);

        lineRenderer.startColor = Color.Lerp(lineColorMin, lineColorMax, t);
        lineRenderer.endColor   = Color.Lerp(lineColorMin, lineColorMax, t);
        lineRenderer.enabled    = true;
    }

    void HideLine()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    // ─── Aterrizaje ───────────────────────────────────────────────────────────

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Planet")) return;

        int incomingID = collision.gameObject.GetInstanceID();

        // ✅ Solo aplicar cooldown si es el mismo planeta de despegue
        bool isSamePlanet = incomingID == lastLandedPlanetID;
        if (isSamePlanet && Time.time - lastLandTime < landCooldown)
        {
            Debug.Log($"[Colisión] Ignorada — cooldown activo en planeta de origen ({Time.time - lastLandTime:F2}s / {landCooldown}s)");
            return;
        }

        ContactPoint contact  = collision.GetContact(0);
        Vector3 surfaceNormal = contact.normal;
        surfaceNormal.z       = 0f;

        Debug.Log($"[Colisión] Planeta: {collision.gameObject.name} | MismoPlaneta: {isSamePlanet}");

        LandOnPlanet(collision.transform, surfaceNormal.normalized);
    }

    void LandOnPlanet(Transform planet, Vector3 surfaceNormal)
    {
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic     = true;

        float angle = Mathf.Atan2(surfaceNormal.y, surfaceNormal.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Vector3 newPos = transform.position + surfaceNormal * 0.1f;
        newPos.z       = 0f;
        transform.position = newPos;

        transform.SetParent(planet);

        landedPlanet       = planet;
        lastLandedPlanetID = planet.gameObject.GetInstanceID();
        isLanded           = true;

        Debug.Log($"[Aterrizaje] En {planet.name} | ID: {lastLandedPlanetID} | Ángulo: {angle:F1}°");
    }
}