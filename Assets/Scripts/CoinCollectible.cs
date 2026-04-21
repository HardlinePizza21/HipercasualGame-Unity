using UnityEngine;

/// <summary>
/// Moneda coleccionable con flotación suave.
/// Requiere un Collider marcado como "Is Trigger".
/// </summary>
public class CoinCollectible : MonoBehaviour
{
    [Header("Puntos")]
    [Tooltip("Arrastra aquí el objeto que tiene el script ScoreCounter.")]
    [SerializeField] private ScoreCounter scoreCounter;
    [SerializeField] private int puntosPorMoneda = 10;

    [Header("Flotación")]
    [SerializeField] private float amplitud = 0.25f;
    [SerializeField] private float velocidad = 1.5f;

    private Vector3 posicionBase;

    void Awake()
    {
        posicionBase = transform.position;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * velocidad) * amplitud;
        transform.position = posicionBase + new Vector3(0f, offsetY, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Rocket") && other.GetComponent<RocketController>() == null)
            return;

        if (scoreCounter != null)
            scoreCounter.SumarPuntos(puntosPorMoneda);

        Destroy(gameObject);
    }
}
