using UnityEngine;
/// <summary>
/// Indicador de dirección hacia el planeta objetivo.
/// Puede usarse como objeto del mundo (Transform) o como flecha UI (RectTransform/Image).
/// </summary>
public class CompassIndicator : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Transform del cohete.")]
    [SerializeField] private Transform rocket;
    [Tooltip("Transform del planeta objetivo/final.")]
    [SerializeField] private Transform targetPlanet;

    [Header("Modo de indicador")]
    [SerializeField] private bool usarUI = true;
    [Tooltip("Si usarUI=true, asigna aquí el RectTransform de la flecha UI.")]
    [SerializeField] private RectTransform flechaUI;
    [Tooltip("Si usarUI=false, asigna aquí el objeto flecha en el mundo.")]
    [SerializeField] private Transform flechaMundo;

    [Header("Ajustes")]
    [Tooltip("Offset de rotación para alinear el sprite/modelo de la flecha.")]
    [SerializeField] private float offsetRotacionZ = 0f;

    void Update()
    {
        if (rocket == null || targetPlanet == null)
            return;

        Vector3 direccion = targetPlanet.position - rocket.position;
        // Solo se usa plano XY para un juego 2D.
        direccion.z = 0f;

        if (direccion.sqrMagnitude <= 0.0001f)
            return;

        float anguloZ = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg + offsetRotacionZ;
        Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, anguloZ);

        if (usarUI)
        {
    // 🔥 usar el mismo objeto (más seguro)
    transform.localRotation = rotacionObjetivo;
        }
        else
        {
            if (flechaMundo == null)
                return;

            flechaMundo.rotation = rotacionObjetivo;
        }
    }

    /// <summary>Permite asignar referencias por código en runtime si lo necesitas.</summary>
    public void Configurar(Transform rocketTransform, Transform objetivoTransform)
    {
        rocket = rocketTransform;
        targetPlanet = objetivoTransform;
    }
}
