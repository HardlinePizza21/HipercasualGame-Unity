using UnityEngine;

public class RockDetector : MonoBehaviour
{
    private RocketController rocket;

    void Awake()
    {
        rocket = GetComponentInParent<RocketController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rock"))
            rocket.ActivarDerrota();
    }
}