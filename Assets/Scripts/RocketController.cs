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
    public bool isLanded = false;
    public float landCooldown = 0.5f;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverUI;

    [Header("Vacío")]
    [SerializeField] private bool usarLimiteY = true;
    [SerializeField] private float limiteYVacio = -10f;
    [SerializeField] private string tagZonaMuerte = "DeathZone";

    [Header("Puntuación")]
    [SerializeField] private GameObject scoreCounter;

    [Header("🔥 Partículas de fuego")]
    [SerializeField] private ParticleSystem fireParticles;

    [Header("💥 Explosión")]
    [SerializeField] private GameObject explosionPrefab;

    [Header("🎵 Sonidos")]
    [Tooltip("Asigna un AudioSource con Loop activado para el sonido de vuelo")]
    [SerializeField] private AudioSource audioSourceVuelo;
    [Tooltip("Clip de sonido que suena al explotar o caer")]
    [SerializeField] private AudioClip clipExplosion;

    private bool gameEnded = false;

    private Vector2 startPosition;
    private bool isDragging = false;
    private Rigidbody rb;
    private Camera mainCamera;
    private LineRenderer lineRenderer;

    private Transform landedPlanet = null;
    private float lastLandTime = -999f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        lineRenderer = GetComponent<LineRenderer>();

        rb.constraints = RigidbodyConstraints.FreezePositionZ
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY;

        HideLine();

        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        if (fireParticles != null)
            fireParticles.Stop();
    }

    void Update()
    {
        if (gameEnded) return;

        // 🔥 fuego y sonido mientras vuela
        if (!isLanded && rb.linearVelocity.magnitude > 0.1f)
        {
            if (fireParticles != null && !fireParticles.isPlaying)
                fireParticles.Play();

            if (audioSourceVuelo != null && !audioSourceVuelo.isPlaying)
                audioSourceVuelo.Play();
        }
        else
        {
            if (fireParticles != null && fireParticles.isPlaying)
                fireParticles.Stop();

            if (audioSourceVuelo != null && audioSourceVuelo.isPlaying)
                audioSourceVuelo.Stop();
        }

        // ☠️ caída al vacío
        if (usarLimiteY && transform.position.y < limiteYVacio)
            ActivarDerrota();
    }

    // ─── INPUT ─────────────────────────────

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

    // ─── FUERZA ─────────────────────────────

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

    // ─── LINE RENDERER ─────────────────────

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

    // ─── COLISIONES ────────────────────────

    void OnCollisionEnter(Collision collision)
    {
        // 🏁 victoria
        FinalPlanetController finalPlanet = collision.gameObject.GetComponent<FinalPlanetController>();
        if (finalPlanet != null)
        {
            if (gameEnded) return;

            gameEnded = true;
            isDragging = false;
            HideLine();

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (fireParticles != null)
                fireParticles.Stop();

            finalPlanet.MostrarFelicitaciones();
            return;
        }

        // 💥 roca
        if (collision.gameObject.CompareTag("Rock"))
        {
            ActivarDerrota();
            return;
        }

        // 🌍 planeta
        if (!collision.gameObject.CompareTag("Planet")) return;

        ContactPoint contact = collision.GetContact(0);
        Vector3 surfaceNormal = contact.normal;
        surfaceNormal.z = 0f;

        LandOnPlanet(collision.transform, surfaceNormal.normalized);
    }

    void OnTriggerEnter(Collider other)
    {
        if (gameEnded) return;

        if (!string.IsNullOrEmpty(tagZonaMuerte) && other.CompareTag(tagZonaMuerte))
            ActivarDerrota();
    }

    // ─── DERROTA ───────────────────────────

    public void ActivarDerrota()
    {
        if (gameEnded) return;

        gameEnded = true;
        isDragging = false;
        isLanded = false;
        HideLine();

        transform.SetParent(null);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // 💥 EXPLOSIÓN
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(
                explosionPrefab,
                transform.position,
                Quaternion.identity
            );

            Destroy(explosion, 2f);
        }

        // 🔊 SONIDO DE EXPLOSIÓN Y DETENCIÓN DE VUELO
        if (clipExplosion != null)
        {
            // Usar PlayClipAtPoint para que el sonido no se corte cuando el gameObject se desactive
            AudioSource.PlayClipAtPoint(clipExplosion, transform.position);
        }
        if (audioSourceVuelo != null && audioSourceVuelo.isPlaying)
        {
            audioSourceVuelo.Stop();
        }

        // 🔥 apagar fuego
        if (fireParticles != null)
            fireParticles.Stop();

        // 🚀 ocultar cohete
        gameObject.SetActive(false);

        if (gameOverUI != null)
            gameOverUI.SetActive(true);
    }

    // ─── ATERRIZAJE ────────────────────────

    void LandOnPlanet(Transform planet, Vector3 surfaceNormal)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // 🔄 Alinea el "up" del cohete con la normal de la superficie
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
        transform.rotation = targetRotation;

        Vector3 newPos = transform.position + surfaceNormal * 0.1f;
        newPos.z = 0f;
        transform.position = newPos;

        transform.SetParent(planet);

        landedPlanet = planet;
        isLanded = true;

        if (fireParticles != null)
            fireParticles.Stop();

        if (scoreCounter != null)
        {
            ScoreCounter sc = scoreCounter.GetComponent<ScoreCounter>();
            if (sc != null)
                sc.RegistrarAterrizajeEnPlaneta();
        }
    }
}