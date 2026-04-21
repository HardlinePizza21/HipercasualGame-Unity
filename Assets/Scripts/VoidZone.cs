using UnityEngine;

/// <summary>
/// Marca una zona de muerte para el cohete.
/// Este objeto debe tener Collider con "Is Trigger" activado.
/// </summary>
[RequireComponent(typeof(Collider))]
public class VoidZone : MonoBehaviour
{
    [Tooltip("Debe coincidir con el tagZonaMuerte del RocketController.")]
    [SerializeField] private string tagConfigurado = "DeathZone";

    void Reset()
    {
        gameObject.tag = tagConfigurado;

        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }
}
