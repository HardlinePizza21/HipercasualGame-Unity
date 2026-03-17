using UnityEngine;

public class SkyRotator : MonoBehaviour
{
    [Header("Rotación")]
    public float rotationX = 0f;
    public float rotationY = 0f;
    public float rotationZ = 10f;

    void Update()
    {
        transform.Rotate(
            rotationX * Time.deltaTime,
            rotationY * Time.deltaTime,
            rotationZ * Time.deltaTime,
            Space.World
        );
    }
}