using UnityEngine;

public class Rock : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        RocketController rocket = collision.gameObject.GetComponent<RocketController>();
        if (rocket != null)
            rocket.ActivarDerrota();
    }

    // Por si el cohete es kinematic (aterrizado), usar trigger como respaldo
    void OnTriggerEnter(Collider other)
    {
        
        RocketController rocket = other.gameObject.GetComponent<RocketController>();
        if (rocket != null)
            rocket.ActivarDerrota();
    }
}