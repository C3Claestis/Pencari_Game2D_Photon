using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Resurection : MonoBehaviourPun
{
    [SerializeField] Image ImageBar;
    [SerializeField] PlayerMovement parentPlayer;
    Resurection Playerresurection;
    private PlayerMovement targetPlayerMovement;  // Referensi PlayerMovement untuk player yang knock
    private int MaxBar = 100;
    private float currentBarValue = 0;
    private bool canTriggerAttack = false;
    private bool isFillingBar = false;
    public void SetValue(int value) => this.currentBarValue = value;
    // Start is called before the first frame update
    void Start()
    {
        ImageBar.fillAmount = 0;  // Set ImageBar awal menjadi kosong
        ImageBar.gameObject.SetActive(false);  // Sembunyikan ImageBar saat awal
    }

    void Update()
    {
        if (canTriggerAttack && Input.GetKeyDown(KeyCode.R))
        {
            parentPlayer.SetResu(true);

            // Eksekusi animasi SetBool Attack untuk semua pemain (sinkronisasi via RPC)
            photonView.RPC("SyncImageBar", RpcTarget.AllBuffered, true); 

            Playerresurection.ImageBar.gameObject.SetActive(true);
            isFillingBar = true;
        }

        if (canTriggerAttack && Input.GetKeyUp(KeyCode.R))
        {
            parentPlayer.SetResu(false);

            // Hentikan animasi Attack untuk semua pemain (sinkronisasi via RPC)
            photonView.RPC("SyncImageBar", RpcTarget.AllBuffered, false);

            Playerresurection.ImageBar.gameObject.SetActive(false);
            isFillingBar = false;
        }

        if (isFillingBar && Playerresurection != null)
        {
            FillImageBarForOtherPlayer();
        }
    }

    // Fungsi untuk mendeteksi saat trigger ter-activate
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Jika object yang masuk ke trigger memiliki tag Player dan bukan parentPlayer
        if (other.CompareTag("Player") && other.gameObject != parentPlayer)
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();

            if (player != null && player.GetKnock())
            {
                Playerresurection = other.transform.GetChild(0).GetComponent<Transform>().GetChild(1).GetComponent<Resurection>();
                targetPlayerMovement = player;  // Simpan referensi ke PlayerMovement dari player yang knock
                // Set flag bahwa attack bisa di-trigger
                canTriggerAttack = true;
            }
        }
    }

    // Fungsi untuk mendeteksi saat object keluar dari trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        // Jika object yang keluar memiliki tag Player dan bukan parentPlayer
        if (other.CompareTag("Player") && other.gameObject != parentPlayer)
        {
            // Set flag bahwa attack tidak bisa di-trigger
            canTriggerAttack = false;
            isFillingBar = false;  // Hentikan pengisian bar
        }
    }
    // Fungsi untuk mengisi ImageBar player lain
    private void FillImageBarForOtherPlayer()
    {
        if (Playerresurection.currentBarValue < MaxBar)
        {
            Playerresurection.currentBarValue += MaxBar * Time.deltaTime / 10f;  // Isi bar secara bertahap setiap 1 detik (sesuaikan waktu jika perlu)
            Playerresurection.ImageBar.fillAmount = Playerresurection.currentBarValue / MaxBar;  // Update tampilan bar
        }
        else
        {
            targetPlayerMovement.SetKnock(false);
            isFillingBar = false;  // Stop pengisian saat bar penuh
            Playerresurection.ImageBar.gameObject.SetActive(false);
        }
    }
    // Tambahkan RPC baru untuk sinkronisasi ImageBar
    [PunRPC]
    void SyncImageBar(bool isActive)
    {
        Playerresurection.ImageBar.gameObject.SetActive(isActive);
    }
}
