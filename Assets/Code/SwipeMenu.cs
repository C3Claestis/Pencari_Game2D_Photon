using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeMenu : MonoBehaviour
{
    [SerializeField] GameObject scrollBar;
    [SerializeField] float autoSlideInterval = 10f; // Interval auto-slide (detik)
    [SerializeField] float slideSpeed = 5f;         // Kecepatan animasi slide

    float scrollPos = 0;
    float[] pos;
    float idleTimer = 0f;
    bool isSliding = false; // Status apakah sedang auto-slide
    int targetIndex = 0; // Indeks slide tujuan untuk auto-slide

    void Start()
    {
        pos = new float[transform.childCount];
        float distance = 1f / (pos.Length - 1f);

        // Inisialisasi posisi tiap konten
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            scrollPos = scrollBar.GetComponent<Scrollbar>().value; // Update posisi scroll saat input
            idleTimer = 0f; // Reset timer jika ada input
            isSliding = false; // Hentikan auto-slide saat ada input
        }
        else
        {
            idleTimer += Time.deltaTime;

            if (!isSliding)
            {
                // Snapping ke posisi terdekat
                float closestDistance = Mathf.Infinity;
                for (int i = 0; i < pos.Length; i++)
                {
                    float distance = Mathf.Abs(scrollBar.GetComponent<Scrollbar>().value - pos[i]);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        targetIndex = i; // Set target index ke slide terdekat
                    }
                }
                scrollBar.GetComponent<Scrollbar>().value = Mathf.Lerp(
                    scrollBar.GetComponent<Scrollbar>().value,
                    pos[targetIndex],
                    Time.deltaTime * 10f
                );
                scrollPos = scrollBar.GetComponent<Scrollbar>().value; // Update posisi scroll
            }

            // Auto-slide jika idle selama `autoSlideInterval`
            if (idleTimer >= autoSlideInterval)
            {
                idleTimer = 0f; // Reset idle timer
                SlideToNext();  // Pindahkan ke slide berikutnya
            }

            // Animasi auto-slide
            if (isSliding)
            {
                scrollBar.GetComponent<Scrollbar>().value = Mathf.Lerp(
                    scrollBar.GetComponent<Scrollbar>().value,
                    pos[targetIndex],
                    Time.deltaTime * slideSpeed
                );

                // Akhiri sliding jika sudah dekat ke posisi target
                if (Mathf.Abs(scrollBar.GetComponent<Scrollbar>().value - pos[targetIndex]) < 0.001f)
                {
                    isSliding = false; // Selesai sliding
                }
            }
        }
    }

    void SlideToNext()
    {
        isSliding = true; // Aktifkan animasi sliding
        targetIndex = (targetIndex + 1) % pos.Length; // Pindah ke slide berikutnya, reset jika di akhir
    }
}
