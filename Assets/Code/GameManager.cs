using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPun
{
    #region ATRIBUT
    [Header("=========== ATRIBUT PLAYER SPAWN ============")]
    [SerializeField] GameObject playerPrefab; // Player prefab
    [SerializeField] GameObject impostorPrefab; // Impostor prefab
    [SerializeField] GameObject treasureUI;
    [SerializeField] Transform[] pointSpawn;  // Array of spawn points

    [Header("=========== ATRIBUT OBJECT FIX SPAWN ============")]
    [SerializeField] GameObject objectPrefab; // Objek fixing prefab
    [SerializeField] Transform[] spawnObjek; // Array of all possible spawn points

    [Header("=========== ATRIBUT UI CREW ============")]
    [SerializeField] Text crew1;
    [SerializeField] Text crew2;
    [SerializeField] Text crew3;
    [SerializeField] Text crew4;

    [Header("=========== PANEL IMPOSTOR/CREW ============")]
    [SerializeField] GameObject panelCrew;
    [SerializeField] GameObject panelImpostor;
    #endregion
    private GameObject[] playersIndex;
    private List<string> crewNames = new List<string>();  // List to hold crew names
    private List<Transform> selectedSpawnPoints = new List<Transform>(); // List of selected spawn points
    private int maxObjectsToSpawn = 10; // Max number of objects to spawn
    private int impostorIndex = -1; // Index untuk impostor
    private int objectsSpawned = 0; // Counter untuk objek yang telah di-spawn
    private int playerCrewCount = 0;  // Counter untuk crew yang telah ditambahkan

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

    //SETPLAYER UI PANEL CREWMET MANAGER
    public void SetPlayerCrew(int index)
    {
        playerCrewCount += index;  // Tambahkan nilai index ke counter

        // Kirim RPC untuk update playerCrewCount di semua klien
        photonView.RPC("RPC_SetPlayerCrewCount", RpcTarget.AllBuffered, index);

        // Pastikan playersIndex tidak null dan periksa panjangnya
        if (playersIndex == null)
        {
            photonView.RPC("RPC_ShowCrewPanel", RpcTarget.AllBuffered, true);
        }

        // Jika playerCrewCount lebih dari 0, cek status knock
        if (playerCrewCount > 0)
        {
            // Dapatkan semua pemain dengan tag "Player"
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            bool allPlayersKnocked = true; // Asumsikan semua pemain knock

            // Loop melalui setiap pemain dan periksa apakah status knock mereka true
            foreach (GameObject player in players)
            {
                PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
                if (playerMovement != null && !playerMovement.GetKnock())
                {
                    allPlayersKnocked = false; // Jika ada satu yang tidak knock, set menjadi false
                    break; // Berhenti loop jika ditemukan satu yang tidak knock
                }
            }

            // Setelah loop selesai, jika semua pemain knock, tampilkan panel crew
            if (allPlayersKnocked)
            {
                photonView.RPC("RPC_ShowCrewPanel", RpcTarget.AllBuffered, true);
            }
        }
    }

    //SET PLAYER IMPOSTOR PANEL MANAGER
    public void SetCountPlayerKnock()
    {
        // Dapatkan semua pemain dengan tag "Player"
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        bool allPlayersKnocked = true; // Asumsikan semua pemain knock

        foreach (GameObject player in players)
        {
            // Dapatkan script PlayerMovement pada player
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

            // Jika ada satu pemain yang tidak knock, ubah nilai allPlayersKnocked menjadi false
            if (playerMovement != null && !playerMovement.GetKnock())
            {
                allPlayersKnocked = false;
                break; // Tidak perlu cek lebih lanjut, cukup satu player tidak knock
            }
        }

        // Jika semua pemain knock, kirim RPC untuk menampilkan panel impostor
        if (allPlayersKnocked && playerCrewCount == 0)
        {
            photonView.RPC("RPC_ShowImpostorPanel", RpcTarget.AllBuffered, true);
        }
    }

    public void LeaveRoom()
    {
        StartCoroutine(LeaveRoomAndReturnToMainMenu());
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

    public void AddPlayerToCrew(string playerName)
    {
        // Cek apakah nama pemain sudah ada dan batas maksimum
        if (!crewNames.Contains(playerName) && crewNames.Count < 4)
        {
            crewNames.Add(playerName);

            // Kirim RPC untuk menambahkan satu nama ke semua klien
            photonView.RPC("AddCrewNameRPC", RpcTarget.AllBuffered, playerName);
        }
    }

    // Fungsi untuk memperbarui UI berdasarkan daftar crewNames
    private void UpdateCrewUI()
    {
        // Pastikan UI diisi sesuai urutan crewNames
        if (crewNames.Count > 0) crew1.text = crewNames[0];
        if (crewNames.Count > 1) crew2.text = crewNames[1];
        if (crewNames.Count > 2) crew3.text = crewNames[2];
        if (crewNames.Count > 3) crew4.text = crewNames[3];

        // Jika kurang dari 4 crew, sisanya kosong
        if (crewNames.Count < 4) crew4.text = "-";
        if (crewNames.Count < 3) crew3.text = "-";
        if (crewNames.Count < 2) crew2.text = "-";
        if (crewNames.Count < 1) crew1.text = "-";

        Debug.Log("Crew UI Updated: " + string.Join(", ", crewNames));
    }

    #region COROUTINE FUNCTION
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

        //Setelah semua pemain di-spawn, ambil semua pemain
        playersIndex = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log("Players found: " + playersIndex.Length);

        // Panggil RPC untuk mendapatkan semua pemain
        photonView.RPC("RPC_GetAllPlayers", RpcTarget.AllBuffered);
    }
    IEnumerator WaitForAllPlayersReady()
    {
        while (playersIndex.Length < PhotonNetwork.CurrentRoom.PlayerCount) // Ganti dengan jumlah pemain yang diinginkan
        {
            // Find all GameObjects with the tag "Player"
            playersIndex = GameObject.FindGameObjectsWithTag("Player");
            yield return null; // Tunggu hingga ada cukup pemain
        }
    }
    public IEnumerator WaitForPlayerCrewCountIncrease(int index)
    {
        // Simpan nilai sebelumnya dari playerCrewCount
        int previousCount = playerCrewCount;

        // Tambahkan ke playerCrewCount
        SetPlayerCrew(index);

        // Tunggu hingga playerCrewCount bertambah
        while (playerCrewCount <= previousCount)
        {
            yield return null; // Tunggu frame berikutnya
        }

        // Lakukan logika tambahan jika diperlukan setelah playerCrewCount bertambah
        Debug.Log("Player crew count increased to: " + playerCrewCount);
    }
    IEnumerator ShowCrewPanelWithDelay(bool show)
    {
        if (show)
        {
            // Berikan jeda 2 detik sebelum menampilkan panel impostor
            yield return new WaitForSeconds(2);
        }

        // Aktifkan atau nonaktifkan panel impostor berdasarkan parameter
        panelCrew.SetActive(show);
    }
    IEnumerator ShowImpostorPanelWithDelay(bool show)
    {
        if (show)
        {
            // Berikan jeda 2 detik sebelum menampilkan panel impostor
            yield return new WaitForSeconds(2);
        }

        // Aktifkan atau nonaktifkan panel impostor berdasarkan parameter
        panelImpostor.SetActive(show);
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
    #endregion

    #region RPC AREA
    // RPC untuk spawn treasure pertama kali ke semua player
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

    //RPC untuk menambahkan playerCrewCount ke semua player, untuk pengecekan panel crew
    [PunRPC]
    public void RPC_SetPlayerCrewCount(int count)
    {
        playerCrewCount += count; // Tambahkan count ke playerCrewCount
        Debug.Log("Player crew count updated to: " + playerCrewCount);
    }

    // RPC untuk menampilkan panel impostor keseluruh player
    [PunRPC]
    void RPC_ShowImpostorPanel(bool show)
    {
        // Jalankan coroutine untuk menampilkan/menyembunyikan panel dengan delay
        StartCoroutine(ShowImpostorPanelWithDelay(show));
    }

    // RPC untuk menampilkan panel crewmet keseluruh player
    [PunRPC]
    void RPC_ShowCrewPanel(bool show)
    {
        // Jalankan coroutine untuk menampilkan/menyembunyikan panel dengan delay
        StartCoroutine(ShowCrewPanelWithDelay(show));
    }

    // RPC untuk mengambil coroutine tunggu sampai seluruh player ready
    [PunRPC]
    void RPC_GetAllPlayers()
    {
        StartCoroutine(WaitForAllPlayersReady());
    }

    // RPC untuk menambahkan index impostor ketika start
    [PunRPC]
    void SetImpostorIndex(int index)
    {
        impostorIndex = index; // Set impostor index
        Debug.Log("Impostor index set to: " + index);
    }

    // RPC untuk menambahkan satu nama ke semua klien dan update UI
    [PunRPC]
    private void AddCrewNameRPC(string newCrewName)
    {
        // Tambahkan nama baru ke list jika belum ada
        if (!crewNames.Contains(newCrewName))
        {
            crewNames.Add(newCrewName);
        }

        // Update UI text sesuai dengan jumlah crewNames
        UpdateCrewUI();
    }
    #endregion
}