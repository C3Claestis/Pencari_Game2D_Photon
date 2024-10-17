using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPun
{
    [Header("=========== ATRIBUT PLAYER SPAWN ============")]
    [SerializeField] GameObject playerPrefab; // Player prefab
    [SerializeField] GameObject impostorPrefab; // Impostor prefab
    [SerializeField] GameObject treasureUI;
    [SerializeField] Transform[] pointSpawn;  // Array of spawn points

    [Header("=========== ATRIBUT OBJECT FIX SPAWN ============")]
    [SerializeField] GameObject objectPrefab; // Objek fixing prefab
    [SerializeField] Transform[] spawnObjek; // Array of all possible spawn points

    private List<Transform> selectedSpawnPoints = new List<Transform>(); // List of selected spawn points
    private int maxObjectsToSpawn = 1; // Max number of objects to spawn
    private int impostorIndex = -1; // Index untuk impostor
    private int objectsSpawned = 0; // Counter untuk objek yang telah di-spawn
    void Start()
    {
        // Pastikan hanya instantiate player saat terhubung dengan Photon
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // Pilih impostor jika masih belum dipilih
            if (PhotonNetwork.IsMasterClient && impostorIndex == -1)
            {
                impostorIndex = Random.Range(0, PhotonNetwork.CurrentRoom.PlayerCount); // Pilih pemain acak
                photonView.RPC("SetImpostorIndex", RpcTarget.AllBuffered, impostorIndex); // Broadcast impostor index
            }

            // Mulai Coroutine untuk menunggu sampai semua pemain mendapatkan impostorIndex
            StartCoroutine(WaitForAllPlayersToReceiveImpostorIndex());
        }

         // Hanya master client yang memilih dan men-spawn objek
        if (PhotonNetwork.IsMasterClient)
        {
            // Pilih 10 titik spawn secara acak dari array spawnObjek
            SelectRandomSpawnPoints();

            // Mulai proses spawn objek secara acak
            StartCoroutine(RandomSpawn());
        }
    }

    IEnumerator WaitForAllPlayersToReceiveImpostorIndex()
    {
        // Tunggu sampai semua pemain mendapatkan impostorIndex
        while (impostorIndex == -1)
        {
            yield return null; // Tunggu frame berikutnya
        }

        Debug.Log("Semua pemain telah mendapatkan impostorIndex.");

        // Mulai spawning player setelah impostorIndex diterima oleh semua pemain
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // Dapatkan index player berdasarkan ActorNumber

        if (playerIndex < pointSpawn.Length)
        {
            Vector3 spawnPosition = pointSpawn[playerIndex].position; // Posisi sesuai urutan player

            // Cek apakah player ini adalah impostor
            if (playerIndex == impostorIndex)
            {
                // Instantiate impostor prefab
                GameObject newImpostor = PhotonNetwork.Instantiate(impostorPrefab.name, spawnPosition, Quaternion.identity);
                PhotonNetwork.LocalPlayer.TagObject = newImpostor.transform;
                treasureUI.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
            }
            else
            {
                // Instantiate player prefab
                GameObject newPlayer = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
                PhotonNetwork.LocalPlayer.TagObject = newPlayer.transform;
            }
        }
        else
        {
            Debug.LogWarning("Tidak ada pointSpawn yang tersedia untuk player index ini.");
        }
    }

    [PunRPC]
    void SetImpostorIndex(int index)
    {
        impostorIndex = index; // Set impostor index
        Debug.Log("Impostor index set to: " + index);
    }

    public void LeaveRoom()
    {
        StartCoroutine(LeaveRoomAndReturnToMainMenu());
    }

    IEnumerator LeaveRoomAndReturnToMainMenu()
    {
        PhotonNetwork.LeaveRoom();

        while (PhotonNetwork.InRoom)
        {
            yield return null;
        }

        SceneManager.LoadScene(0);
    }

   // Pilih 10 spawn point secara acak dari array spawnObjek
    void SelectRandomSpawnPoints()
    {
        List<Transform> availablePoints = new List<Transform>(spawnObjek); // Salin array spawnObjek ke list

        // Shuffle spawnObjek dan pilih 10 pertama
        for (int i = 0; i < maxObjectsToSpawn; i++)
        {
            if (availablePoints.Count == 0) break;

            int randomIndex = Random.Range(0, availablePoints.Count);
            selectedSpawnPoints.Add(availablePoints[randomIndex]);
            availablePoints.RemoveAt(randomIndex); // Hapus dari list setelah dipilih
        }
    }

    IEnumerator RandomSpawn()
    {
        while (objectsSpawned < maxObjectsToSpawn)
        {
            // Pilih spawn point yang tersedia
            Transform spawnPoint = GetRandomAvailableSpawnPoint();

            if (spawnPoint != null)
            {
                int spawnIndex = selectedSpawnPoints.IndexOf(spawnPoint); // Dapatkan index dari selectedSpawnPoints

                photonView.RPC("SpawnObjectWithValue", RpcTarget.AllBuffered, spawnIndex);
                objectsSpawned++;
            }
            yield return new WaitForSeconds(1); // Tunggu 1 detik sebelum spawn berikutnya
        }

        Debug.Log("Sudah spawn 10 objek. Berhenti spawn.");
    }

    [PunRPC]
    void SpawnObjectWithValue(int spawnIndex)
    {
        Transform parentSpawnPoint = selectedSpawnPoints[spawnIndex]; // Gunakan selectedSpawnPoints

        // Cek jika spawn point sudah ada child, hapus child sebelumnya
        if (parentSpawnPoint.childCount > 0)
        {
            Destroy(parentSpawnPoint.GetChild(0).gameObject);
        }

        // Instantiate object di posisi spawnObjek yang dipilih
        GameObject newObject = PhotonNetwork.Instantiate(objectPrefab.name, parentSpawnPoint.position, Quaternion.identity);

        // Jadikan object sebagai child dari spawnPoint yang dipilih
        newObject.transform.SetParent(parentSpawnPoint);
    }

    // Mendapatkan spawn point yang tersedia dari selectedSpawnPoints
    Transform GetRandomAvailableSpawnPoint()
    {
        List<Transform> availableSpawnPoints = new List<Transform>();

        foreach (Transform point in selectedSpawnPoints)
        {
            if (point.childCount == 0)
            {
                availableSpawnPoints.Add(point);
            }
        }

        if (availableSpawnPoints.Count > 0)
        {
            return availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
        }

        return null;
    }
}
