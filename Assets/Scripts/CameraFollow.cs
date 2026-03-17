using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Configuración")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Z negativo para cámara ortográfica 3D

    void LateUpdate()
    {
        if (target == null) return;

        // ✅ Seguir X e Y del target, mantener Z fija del offset
        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            offset.z   // Z nunca cambia — la cámara siempre mira desde el mismo plano
        );

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}