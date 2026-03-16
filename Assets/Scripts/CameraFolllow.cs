using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;        // arrastra el cohete aquí en el Inspector

    [Header("Configuración")]
    public float smoothSpeed = 5f;  // qué tan suave sigue al cohete
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Z debe mantenerse en -10 en 2D

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}