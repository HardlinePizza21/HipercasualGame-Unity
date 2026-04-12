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
    [SerializeField] private GameObject scoreCounter; // 🔥 CAMBIO AQUÍ

    private bool gameEnded = false;

    private Vector2   startPosition;
    private bool      isDragging = false;
    private Rigidbody rb;
    private Camera    mainCamera;
    private LineRenderer lineRenderer;

    private int   lastLandedPlanetID = -1;
    private float lastLandTime       = -999f;

    void Awake()
    {
        rb           = GetComponent<Rigidbody>();
        mainCamera   = Camera.main;
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
        isDragging    = true;
    }

    public void OnClickReleased(InputValue value)
    {
        if (gameEnded || !isDragging) return;

        Vector2 endPosition = Mouse.current.position.ReadValue();
        Vector3 finalForce  = CalculateForce(startPosition, endPosition);

        transform.SetParent(null);

        rb.isKinematic = false;
        isLanded       = false;
        lastLandTime   = Time.time;

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

    Vector3 CalculateForce(Vector2 start, Vector2 end)
    {
        Vector2 delta       = start - end;
        Vector2 clamped     = Vector2.ClampMagnitude(delta, maxDragDistance);
        float   t           = clamped.magnitude / maxDragDistance;
        float   magnitude   = Mathf.Lerp(minForce, maxForce, t);

        float z = transform.position.z - mainCamera.transform.position.z;

        Vector3 zero = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, z));
        Vector3 dir  = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2 + clamped.x, Screen.height / 2 + clamped.y, z));

        Vector3 direction = (dir - zero).normalized;
        direction.z = 0;

        return direction * magnitude;
    }

    // ─── LÍNEA ─────────────────────────────────────────

    void UpdateLine(Vector2 start, Vector2 end)
    {
        Vector2 delta   = start - end;
        Vector2 clamped = Vector2.ClampMagnitude(delta, maxDragDistance);
        float   t       = clamped.magnitude / maxDragDistance;

        float z = transform.position.z - mainCamera.transform.position.z;

        Vector3 zero = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, z));
        Vector3 dir  = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2 + clamped.x, Screen.height / 2 + clamped.y, z));

        Vector3 direction = (dir - zero).normalized;
        direction.z = 0;

        Vector3 A = transform.position;
        Vector3 B = A + direction * Mathf.Lerp(lineMinLength, lineMaxLength, t);

        lineRenderer.SetPosition(0, A);
        lineRenderer.SetPosition(1, B);
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
        if (collision.gameObject.CompareTag("Rock"))
        {
            if (gameEnded) return;

            gameEnded = true;
            isDragging = false;
            HideLine();

            rb.linearVelocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (gameOverUI != null)
                gameOverUI.SetActive(true);

            return;
        }

        if (!collision.gameObject.CompareTag("Planet")) return;

        int id = collision.gameObject.GetInstanceID();

        if (id == lastLandedPlanetID && Time.time - lastLandTime < landCooldown)
            return;

        LandOnPlanet(collision.transform);
    }

    void LandOnPlanet(Transform planet)
    {
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic     = true;

        transform.SetParent(planet);
        isLanded = true;

        lastLandedPlanetID = planet.gameObject.GetInstanceID();

        // 🔥 SUMAR PUNTOS (ahora sí seguro funciona)
        if (scoreCounter != null)
        {
            ScoreCounter sc = scoreCounter.GetComponent<ScoreCounter>();

            if (sc != null)
                sc.RegistrarAterrizajeEnPlaneta();
            else
                Debug.LogWarning("El objeto no tiene ScoreCounter.");
        }
        else
        {
            Debug.LogWarning("No asignaste ScoreCounter en el Inspector.");
        }
    }
}