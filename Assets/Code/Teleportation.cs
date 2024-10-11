using UnityEngine;

public class Teleportation : MonoBehaviour
{
    [SerializeField] Transform teleportTarget; // Target teleportasi pertama
    private Transform player;         // Referensi ke transformasi pemain
    private bool canTeleport = false; // Untuk mengecek apakah player berada di area trigger

    // Fungsi ketika player masuk ke trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Mengecek apakah yang masuk ke trigger adalah player
        if (collision.CompareTag("Player"))
        {
            player = collision.transform;
            canTeleport = true; // Player bisa teleport
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            playerMovement.SetTeleActive(canTeleport);
        }
    }

    // Fungsi ketika player keluar dari trigger
    private void OnTriggerExit2D(Collider2D collision)
    {
        // Mengecek apakah yang keluar dari trigger adalah player
        if (collision.CompareTag("Player"))
        {
            canTeleport = false; // Player tidak bisa teleport lagi
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            playerMovement.SetTeleActive(canTeleport);
        }
    }

    void Update()
    {
        // Mengecek apakah player bisa teleport dan menekan tombol "T"
        if (canTeleport && Input.GetKeyDown(KeyCode.T))
        {
            player.position = teleportTarget.GetChild(0).position; 

            // Setelah teleport, player tidak bisa langsung teleport lagi sampai masuk trigger kembali
            canTeleport = false;
        }
    }
}
