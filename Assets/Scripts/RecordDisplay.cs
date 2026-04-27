using UnityEngine;
using TMPro;

public class RecordDisplay : MonoBehaviour
{
    private TextMeshProUGUI texto;

    void Start()
    {
        texto = GetComponent<TextMeshProUGUI>();
        int record = PlayerPrefs.GetInt("Record", 0);
        texto.text = record > 0 ? $"Récord: {record}" : "Sin récord aún";
    }
}