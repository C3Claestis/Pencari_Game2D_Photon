using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab; // Player prefab
    [SerializeField] Transform[] pointSpawn;  // Array of spawn points
    // [SerializeField] GameObject PanelDefeat;
    // [SerializeField] GameObject PanelWinner;

    // [HideInInspector] public bool isDefeat = false;
    // [HideInInspector] public bool isWinner = false;
    void Start()
    {
        // Pastikan hanya instantiate player saat terhubung dengan Photon
        if (PhotonNetwork.IsConnectedAndReady)
        {
            int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // Dapatkan index player berdasarkan ActorNumber

            // Jika playerIndex lebih kecil dari jumlah pointSpawn yang tersedia
            if (playerIndex < pointSpawn.Length)
            {
                Vector3 spawnPosition = pointSpawn[playerIndex].position; // Posisi sesuai urutan player

                // Instantiate player pada posisi yang sesuai
                GameObject newPlayer = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);

                // Set referensi ke objek player untuk LocalPlayer
                PhotonNetwork.LocalPlayer.TagObject = newPlayer.transform;
            }
            else
            {
                Debug.LogWarning("Tidak ada pointSpawn yang tersedia untuk player index ini.");
            }
        }
    }

    void Update()
    {
        // PanelDefeat.SetActive(isDefeat);
        // PanelWinner.SetActive(isWinner);
    }

    public void LeaveRoom()
    {
        StartCoroutine(LeaveRoomAndReturnToMainMenu());
    }

     // Coroutine untuk memastikan keluar dari room dan pindah ke scene 0
    IEnumerator LeaveRoomAndReturnToMainMenu()
    {
        // Keluar dari room
        PhotonNetwork.LeaveRoom();

        // Tunggu hingga proses keluar dari room selesai
        while (PhotonNetwork.InRoom)
        {
            yield return null;
        }

        // Setelah berhasil keluar, load scene 0
        SceneManager.LoadScene(0);
    }
}
