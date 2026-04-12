using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Header("Rotación suave")]
    [SerializeField] private Vector3 rotacionPorSegundo = new Vector3(10f, 15f, 20f);

    [Header("Movimiento suave (máx ±1 por eje)")]
    [SerializeField] [Range(0f, 1f)] private float amplitud = 0.5f;
    [SerializeField] private float frecuenciaX = 0.5f;
    [SerializeField] private float frecuenciaY = 0.4f;
    [SerializeField] private float frecuenciaZ = 0.3f;

    private Transform[] rocas;
    private Vector3[] posicionesIniciales;
    private float[] offsets;

    void Start()
    {
        int count = transform.childCount;

        rocas = new Transform[count];
        posicionesIniciales = new Vector3[count];
        offsets = new float[count];

        for (int i = 0; i < count; i++)
        {
            rocas[i] = transform.GetChild(i);
            posicionesIniciales[i] = rocas[i].position;

            // 🔥 offset para que no se muevan igual
            offsets[i] = Random.Range(0f, 100f);
        }
    }

    void Update()
    {
        float t = Time.time;

        for (int i = 0; i < rocas.Length; i++)
        {
            Transform roca = rocas[i];

            // 🔁 ROTACIÓN INDIVIDUAL
            roca.Rotate(rotacionPorSegundo * Time.deltaTime, Space.Self);

            // 🌊 MOVIMIENTO SUAVE INDIVIDUAL
            float x = Mathf.Sin((t + offsets[i]) * frecuenciaX) * amplitud;
            float y = Mathf.Cos((t + offsets[i]) * frecuenciaY) * amplitud;
            float z = Mathf.Sin((t + offsets[i]) * frecuenciaZ) * amplitud;

            roca.position = posicionesIniciales[i] + new Vector3(x, y, z);
        }
    }
}