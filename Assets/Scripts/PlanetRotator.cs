using UnityEngine;

public class PlanetRotator : MonoBehaviour
{
    [Header("Rotación")]
    public float rotationSpeed = 30f; // grados por segundo, negativo = sentido contrario

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}