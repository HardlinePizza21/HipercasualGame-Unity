using UnityEngine;

/// <summary>
/// Flecha que siempre apunta hacia donde está el objetivo.
/// Se posiciona en la dirección del objetivo y apunta radialmente hacia afuera.
/// </summary>
public class DirectionArrow : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform rocket;
    [SerializeField] private Transform targetPlanet;

    [Header("Ajustes")]
    [SerializeField] private float distanciaAlObjetivo = 2f;

    void Update()
    {
        if (rocket == null || targetPlanet == null)
            return;

        // 📍 Calcular dirección hacia el objetivo
        Vector3 direccion = (targetPlanet.position - rocket.position).normalized;
        direccion.z = 0f;

        if (direccion.sqrMagnitude < 0.001f)
            return;

        // 🎯 Ángulo hacia el objetivo (en grados)
        float anguloAlObjetivo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        // 📍 Posicionar la flecha en esa dirección a cierta distancia
        Vector3 posicion = rocket.position + direccion * distanciaAlObjetivo;
        posicion.z = -5f;
        transform.position = posicion;

        // 🎯 Rotar la flecha para que apunte radialmente hacia afuera
        transform.rotation = Quaternion.Euler(0f, 0f, anguloAlObjetivo);
    }
}



