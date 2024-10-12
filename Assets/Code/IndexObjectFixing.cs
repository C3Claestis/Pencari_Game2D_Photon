using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IndexObjectFixing : MonoBehaviour
{
    TextMeshProUGUI textMeshProUGUI;
    int indexCount;

    public void SetIndeCount(int decrement)
    {
        indexCount -= decrement; // kurangi nilai indexCount
        UpdateText(); // perbarui tampilan teks setelah dikurangi
    }

    void Start()
    {
        indexCount = 10; // Set default value
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        UpdateText(); // Menampilkan teks di awal
    }

    void UpdateText()
    {
        textMeshProUGUI.text = indexCount.ToString(); // Update teks di UI
    }
}
