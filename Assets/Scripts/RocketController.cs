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

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverUI;

    [Header("Puntuación")]
    [SerializeField] private GameObject scoreCounter;

    private bool gameEnded = false;

    private Vector2 startPosition;
    private bool isDragging = false;
    private Rigidbody rb;
    private Camera mainCamera;
    private CapsuleCollider capsuleCollider;
    private LineRenderer lineRenderer;

    private Transform landedPlanet = null;
    private int lastLandedPlanetID = -1;
    private float lastLandTime = -999f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        mainCamera = Camera.main;
        lineRenderer = GetComponent<LineRenderer>();

        rb.constraints = RigidbodyConstraints.FreezePositionZ
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY;

        HideLine();

        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    // ─── INPUT ─────────────────────────────────────────

    public void OnClickPressed(InputValue value)
    {
        if (gameEnded || !isLanded) return;

        startPosition = Mouse.current.position.ReadValue();
        isDragging = true;
    }

    public void OnClickReleased(InputValue value)
    {
        if (gameEnded || !isDragging) return;

        Vector2 endPosition = Mouse.current.position.ReadValue();
        Vector3 finalForce = CalculateForce(startPosition, endPosition);

        transform.SetParent(null);

        rb.isKinematic = false;
        isLanded = false;
        lastLandTime = Time.time;
        landedPlanet = null;

        rb.AddForce(finalForce, ForceMode.Impulse);

        isDragging = false;
        HideLine();
    }

    public void OnPointer(InputValue value)
    {
        if (gameEnded || !isDragging) return;
        UpdateLine(startPosition, value.Get<Vector2>());
    }

    // ─── FUERZA ────────────────────────────────────────

    Vector3 CalculateForce(Vector2 screenStart, Vector2 screenEnd)
    {
        Vector2 rawDelta = screenStart - screenEnd;
        Vector2 clampedDelta = Vector2.ClampMagnitude(rawDelta, maxDragDistance);
        float t = clampedDelta.magnitude / maxDragDistance;

        float forceMagnitude = Mathf.Lerp(minForce, maxForce, t);

        float objectZ = transform.position.z - mainCamera.transform.position.z;

        Vector3 zero = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, objectZ));
        Vector3 delta = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f + clampedDelta.x, Screen.height / 2f + clampedDelta.y, objectZ));

        Vector3 directionWorld = (delta - zero).normalized;
        directionWorld.z = 0f;

        return directionWorld * forceMagnitude;
    }

    // ─── LINE RENDERER ─────────────────────────────────

    void UpdateLine(Vector2 screenStart, Vector2 screenEnd)
    {
        Vector2 rawDelta = screenStart - screenEnd;
        Vector2 clampedDelta = Vector2.ClampMagnitude(rawDelta, maxDragDistance);
        float t = clampedDelta.magnitude / maxDragDistance;

        float objectZ = transform.position.z - mainCamera.transform.position.z;

        Vector3 zero = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, objectZ));
        Vector3 delta = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f + clampedDelta.x, Screen.height / 2f + clampedDelta.y, objectZ));

        Vector3 directionWorld = (delta - zero).normalized;
        directionWorld.z = 0f;

        Vector3 pointA = transform.position;
        float lineLength = Mathf.Lerp(lineMinLength, lineMaxLength, t);
        Vector3 pointB = pointA + directionWorld * lineLength;

        lineRenderer.SetPosition(0, pointA);
        lineRenderer.SetPosition(1, pointB);

        lineRenderer.startColor = Color.Lerp(lineColorMin, lineColorMax, t);
        lineRenderer.endColor = Color.Lerp(lineColorMin, lineColorMax, t);
        lineRenderer.enabled = true;
    }

    void HideLine()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    // ─── COLISIONES ────────────────────────────────────

    void OnCollisionEnter(Collision collision)
    {
        // 🏁 PLANETA FINAL → VICTORIA
        FinalPlanetController finalPlanet = collision.gameObject.GetComponent<FinalPlanetController>();
        if (finalPlanet != null)
        {
            if (gameEnded) return;

            gameEnded = true;
            isDragging = false;
            HideLine();

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            finalPlanet.MostrarFelicitaciones();
            return;
        }
        
        // 💥 ASTEROIDES → GAME OVER
        if (collision.gameObject.CompareTag("Rock"))
        {
            if (gameEnded) return;

            gameEnded = true;
            isDragging = false;
            HideLine();

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (gameOverUI != null)
                gameOverUI.SetActive(true);

            return;
        }

        // 🌍 PLANETAS → ATERRIZAJE (TU LÓGICA ORIGINAL)
        if (!collision.gameObject.CompareTag("Planet")) return;

        int incomingID = collision.gameObject.GetInstanceID();
        bool isSamePlanet = incomingID == lastLandedPlanetID;

        if (isSamePlanet && Time.time - lastLandTime < landCooldown)
            return;

        ContactPoint contact = collision.GetContact(0);
        Vector3 surfaceNormal = contact.normal;
        surfaceNormal.z = 0f;

        LandOnPlanet(collision.transform, surfaceNormal.normalized);
    }

    void LandOnPlanet(Transform planet, Vector3 surfaceNormal)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        float angle = Mathf.Atan2(surfaceNormal.y, surfaceNormal.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Vector3 newPos = transform.position + surfaceNormal * 0.1f;
        newPos.z = 0f;
        transform.position = newPos;

        transform.SetParent(planet);

        landedPlanet = planet;
        lastLandedPlanetID = planet.gameObject.GetInstanceID();
        isLanded = true;

        // 🧮 SCORE
        if (scoreCounter != null)
        {
            ScoreCounter sc = scoreCounter.GetComponent<ScoreCounter>();
            if (sc != null)
                sc.RegistrarAterrizajeEnPlaneta();
        }
    }
}