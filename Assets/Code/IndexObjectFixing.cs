using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class IndexObjectFixing : MonoBehaviourPun
{
    [SerializeField] GameObject portal;
    [SerializeField] Transform[] posPortal;
    int indexPortal = 5; // Jumlah portal yang ingin di-spawn
    TextMeshProUGUI textMeshProUGUI;
    int indexCount;
    private bool spawningCompleted = false; // Menandakan apakah spawning sudah selesai
    private List<int> selectedSpawnIndices = new List<int>(); // List of selected spawn indices

    public void SetIndeCount(int decrement)
    {
        indexCount -= decrement; // Kurangi nilai indexCount
        UpdateText(); // Perbarui tampilan teks setelah dikurangi

        // Jika indexCount sudah 0 dan spawning belum selesai, mulai proses spawn
        if (indexCount == 0 && !spawningCompleted && PhotonNetwork.IsMasterClient)
        {
            SpawnPortal();
        }
    }

    void Start()
    {
        indexCount = 1; // Set default value
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        UpdateText(); // Menampilkan teks di awal
    }

    void UpdateText()
    {
        textMeshProUGUI.text = indexCount.ToString(); // Update teks di UI
    }

    // Fungsi ini hanya dilakukan oleh MasterClient untuk memilih posisi spawn portal
    void SpawnPortal()
    {
        List<int> spawnIndices = new List<int>();

        // MasterClient memilih spawn points
        if (PhotonNetwork.IsMasterClient)
        {
            List<Transform> availablePoints = new List<Transform>(posPortal); // Salin array posPortal ke list

            // Shuffle posPortal dan pilih indexPortal pertama (misalnya 5 pertama)
            for (int i = 0; i < indexPortal; i++)
            {
                if (availablePoints.Count == 0) break;

                int randomIndex = Random.Range(0, availablePoints.Count);
                spawnIndices.Add(randomIndex); // Simpan indeks yang dipilih
                availablePoints.RemoveAt(randomIndex); // Hapus dari list setelah dipilih
            }

            // Kirimkan spawn indices ke semua klien untuk sinkronisasi
            photonView.RPC("SyncSpawnPoints", RpcTarget.AllBuffered, spawnIndices.ToArray());
        }
    }

    // Sinkronisasi spawn points ke semua klien
    [PunRPC]
    void SyncSpawnPoints(int[] spawnIndices)
    {
        // Update selectedSpawnIndices untuk semua klien
        selectedSpawnIndices.Clear();
        selectedSpawnIndices.AddRange(spawnIndices);

        // MasterClient akan spawn portal
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnPortalsForAllClients());
        }
    }

    IEnumerator SpawnPortalsForAllClients()
    {
        for (int i = 0; i < selectedSpawnIndices.Count; i++)
        {
            Transform spawnPoint = posPortal[selectedSpawnIndices[i]]; // Dapatkan posisi spawn point

            // Hanya MasterClient yang instansiasi portal
            GameObject newObject = PhotonNetwork.Instantiate(portal.name, spawnPoint.position, Quaternion.identity);
            
            // Set parent setelah spawn untuk memastikan portal di parent ke posPortal
            photonView.RPC("SetParentForPortal", RpcTarget.AllBuffered, newObject.GetComponent<PhotonView>().ViewID, i);

            yield return new WaitForSeconds(1); // Tunggu 1 detik sebelum spawn berikutnya
        }

        spawningCompleted = true; // Tandai bahwa spawning sudah selesai
        Debug.Log("Sudah spawn semua portal. Spawning dihentikan.");
    }

    [PunRPC]
    void SetParentForPortal(int portalViewID, int spawnIndex)
    {
        // Cari game object berdasarkan PhotonView ID
        GameObject portalObject = PhotonView.Find(portalViewID).gameObject;

        // Cari spawn point berdasarkan indeks dan set sebagai parent
        Transform spawnPoint = posPortal[selectedSpawnIndices[spawnIndex]];
        portalObject.transform.SetParent(spawnPoint);
    }
}
