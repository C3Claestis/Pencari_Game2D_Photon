using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ObjectFixed : MonoBehaviourPun
{
    [SerializeField] GameObject bar;
    [SerializeField] Image imageBar;
    [SerializeField] GameObject tanda;
    int MaxBar = 10;
    float currentBar;
    float barDecreaseAmount = 0.1f; // nilai pengurangan bar setiap trigger
    float defaultBarDecreasement;
    int playerCount = 0; // penghitung jumlah player dalam collider
    bool canBerkurang = false;
    private Animator parentAnim;
    IndexObjectFixing IndexTreasure;
    bool isFixed = false; // Flag untuk mencegah pengurangan berulang
    public void SetCanBerkurang(bool canBerkurang) => this.canBerkurang = canBerkurang;
    void Start()
    {
        defaultBarDecreasement = barDecreaseAmount;
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
        tanda.SetActive(canBerkurang);
        if (canBerkurang && !isFixed) // Cek apakah objek belum diperbaiki
        {
            switch (playerCount)
            {
                case 0:
                    barDecreaseAmount = defaultBarDecreasement;
                    break;
                case 2:
                    barDecreaseAmount += 0.1f;
                    break;
                case 3:
                    barDecreaseAmount += 0.2f;
                    break;
                case 4:
                    barDecreaseAmount += 0.4f;
                    break;
            }
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
            // Panggil RPC untuk mengurangi indexCount di semua klien
            IndexTreasure.SetIndeCount(1);
            photonView.RPC("RPC_FixedIndex", RpcTarget.AllBuffered, true, 1);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isFixed) // Cek jika objek belum diperbaiki
        {
            if (currentBar > 0)
            {
                PlayerMovement player = other.GetComponent<PlayerMovement>();
                player.SetCanFixing(true);
                playerCount++; // Tambah jumlah player di dalam collider
            }
        }
        if (other.CompareTag("AttackPointPlayer") && !isFixed)
        {
            canBerkurang = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isFixed) // Cek jika objek belum diperbaiki
        {
            if (currentBar > 0)
            {
                PlayerMovement player = other.GetComponent<PlayerMovement>();
                player.SetCanFixing(false);
                playerCount--; // Kurangi jumlah player di dalam collider

                // Jika tidak ada lagi player di dalam area, stop pengurangan bar
                if (playerCount <= 0)
                {
                    canBerkurang = false;
                    playerCount = 0; // Jaga agar playerCount tidak negatif
                }
            }
        }
        if (other.CompareTag("AttackPointPlayer") && !isFixed)
        {
            canBerkurang = false;
        }
    }
    
    [PunRPC]
    void RPC_FixedIndex(bool fixedex, int index)
    {
        isFixed = fixedex; // Tandai objek sebagai sudah diperbaiki
                           // Hanya ubah indexCount jika objek belum diperbaiki
        if (!isFixed)
        {
            IndexTreasure.SetIndeCount(index); // Kurangi jumlah objek dengan 1
        }
    }
}
