using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectFixed : MonoBehaviour
{
    [SerializeField] GameObject bar;
    [SerializeField] Image imageBar;
    int MaxBar = 1000;
    float currentBar;
    float barDecreaseAmount = 10f; // nilai pengurangan bar setiap trigger
    int playerCount = 0; // penghitung jumlah player dalam collider
    bool canBerkurang = false;
    private Animator parentAnim;
    IndexObjectFixing IndexTreasure;
    bool isFixed = false; // Flag untuk mencegah pengurangan berulang

    void Start()
    {
        IndexTreasure = FindAnyObjectByType<IndexObjectFixing>();
        currentBar = MaxBar;
        imageBar.fillAmount = currentBar / MaxBar;
        parentAnim = GetComponentInParent<Animator>();
        bar.SetActive(false); // Awalnya bar disembunyikan
    }

    void Update()
    {
        // Bar hanya aktif jika ada player dalam collider
        bar.SetActive(playerCount > 0);

        if (canBerkurang && !isFixed) // Cek apakah objek belum diperbaiki
        {
            currentBar -= barDecreaseAmount;

            // memastikan nilai currentBar tidak kurang dari 0
            if (currentBar <= 0 && !isFixed) // Cek apakah currentBar sudah 0 dan objek belum diperbaiki
            {
                currentBar = 0; // Set currentBar ke 0 jika kurang dari 0
                parentAnim.SetBool("Open", true);
                FixObject(); // Panggil fungsi untuk mengurangi index hanya satu kali
                canBerkurang = false; // Stop pengurangan bar
            }

            // memperbarui tampilan bar
            imageBar.fillAmount = currentBar / MaxBar;
        }
    }

    // Fungsi ini memastikan pengurangan indexCount hanya terjadi sekali
    void FixObject()
    {
        if (!isFixed) // Pastikan hanya dipanggil sekali
        {
            IndexTreasure.SetIndeCount(1); // Kurangi jumlah objek dengan 1
            isFixed = true; // Tandai objek sebagai sudah diperbaiki
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isFixed) // Cek jika objek belum diperbaiki
        {
            if (currentBar > 0)
            {
                playerCount++; // Tambah jumlah player di dalam collider
                canBerkurang = true; // Mulai mengurangi bar
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isFixed) // Cek jika objek belum diperbaiki
        {
            if (currentBar > 0)
            {
                playerCount--; // Kurangi jumlah player di dalam collider

                // Jika tidak ada lagi player di dalam area, stop pengurangan bar
                if (playerCount <= 0)
                {
                    canBerkurang = false;
                    playerCount = 0; // Jaga agar playerCount tidak negatif
                }
            }
        }
    }
}
