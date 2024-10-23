using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Connection Status")]
    public Text statusKoneksi;

    [Header("Login UI Panel")]
    public InputField NamaPlayer;
    public GameObject PanelLogin;

    [Header("Room Panel")]
    public GameObject RoomPanel;

    [Header("Membuat Room Panel")]
    public GameObject BuatRoomPanel;
    public InputField NamaRoomInputField;
    public Slider maxPlayerSlider;
    public Text maxPlayerSliderValueText;

    [Header("Join Room")]
    public GameObject JoinRoomPanel;
    public InputField JoinRoomName;

    [Header("Game Panel")]
    public GameObject GamePanel;
    public Text roomInfoText;
    public GameObject TombolMulaiGame;
    public GameObject daftarplayerPrefab;
    public GameObject daftarplayerContent;


    [Header("Daftar Room Panel")]
    public GameObject DaftarRoomPanel;
    public GameObject daftarEntriRoomPrefab;
    public GameObject daftarRoomUtamaGameobject;

    [Header("Gabung Room Acak/ Gabung Cepat")]
    public GameObject RoomAcakPanel;

    private Dictionary<string, RoomInfo> cachedDaftarRoom;
    private Dictionary<string, GameObject> daftarRoomGameObjects;
    private Dictionary<int, GameObject> daftarPlayerGameobjects;

    // Start is called before the first frame update
    private void Start()
    {
        ActivatePanel(PanelLogin.name);
        cachedDaftarRoom = new Dictionary<string, RoomInfo>();
        daftarRoomGameObjects = new Dictionary<string, GameObject>();
        PhotonNetwork.AutomaticallySyncScene = true;

        // Menampilkan nilai slider saat slider diubah
        maxPlayerSlider.onValueChanged.AddListener(delegate { UpdateMaxPlayerSliderValue(); });

        // Setting awal slider
        maxPlayerSlider.minValue = 1f;
        maxPlayerSlider.maxValue = 5f; // Slider akan memiliki range 0-100, dan kita akan memetakannya ke max 5 player
        maxPlayerSlider.wholeNumbers = false;
        UpdateMaxPlayerSliderValue();
    }

    // Update is called once per frame
    private void Update()
    {
        //Mengecek Status Koneksi
        statusKoneksi.text = "Status Koneksi: " + PhotonNetwork.NetworkClientState;

        // Cek apakah slider berada di float, jika ya, bulatkan ke int terdekat
        if (maxPlayerSlider.value != Mathf.Round(maxPlayerSlider.value))
        {
            maxPlayerSlider.value = Mathf.Round(maxPlayerSlider.value); // Paksa slider ke nilai integer
        }
    }

    //Tombol Login
    public void OnLoginButtonClicked()
    {
        string playerName = NamaPlayer.text;
        if (!string.IsNullOrEmpty(playerName))
        {

            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Nama Player Tidak Valid!");
        }
    }

    //Tombol Kembali
    public void OnCancelButtonClicked()
    {
        ActivatePanel(RoomPanel.name);
    }

    #region BUAT ROOM DAN JOIN ROOM
    //Tombol buat room
    public void OnRoomCreateButtonClicked()
    {
        string NamaRoom = NamaRoomInputField.text;

        if (string.IsNullOrEmpty(NamaRoom))
        {
            NamaRoom = "Room " + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)maxPlayerSlider.value;

        // Gunakan hasil slider sebagai jumlah maksimal pemain
        int maxPlayers = (int)maxPlayerSlider.value; // Gunakan integer dari slider
        roomOptions.MaxPlayers = (byte)maxPlayers;

        PhotonNetwork.CreateRoom(NamaRoom, roomOptions);
    }

    // Update nilai slider pada UI ketika slider diubah
    private void UpdateMaxPlayerSliderValue()
    {
        // Gunakan nilai integer dari slider (langsung ke int)
        int maxPlayers = (int)maxPlayerSlider.value; // Pastikan langsung menggunakan integer
        if (maxPlayers == 0) maxPlayers = 1; // Minimal 1 pemain

        // Update UI text
        maxPlayerSliderValueText.text = maxPlayers.ToString();
    }

    //Ketika Player Masuk Room
    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Bergabung dengan " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(GamePanel.name);

        roomInfoText.text = "Nama Room: " + PhotonNetwork.CurrentRoom.Name + "\n" +
                            "Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                            PhotonNetwork.CurrentRoom.MaxPlayers;

        if (PhotonNetwork.LocalPlayer.IsMasterClient && PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            TombolMulaiGame.SetActive(true);
        }
        else
        {
            TombolMulaiGame.SetActive(false);
        }

        if (daftarPlayerGameobjects == null)
        {
            daftarPlayerGameobjects = new Dictionary<int, GameObject>();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject daftarPlayerGameobject = Instantiate(daftarplayerPrefab);
            daftarPlayerGameobject.transform.SetParent(daftarplayerContent.transform);
            daftarPlayerGameobject.transform.localScale = Vector3.one;

            daftarPlayerGameobject.transform.Find("namaPlayerText").GetComponent<Text>().text = player.NickName;
            daftarPlayerGameobject.transform.Find("IndikatorPlayer").gameObject.SetActive(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

            daftarPlayerGameobjects.Add(player.ActorNumber, daftarPlayerGameobject);
        }
    }

    //Update Ketika Player Enter Ke Room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GameObject daftarPlayerGameobject = Instantiate(daftarplayerPrefab);
        daftarPlayerGameobject.transform.SetParent(daftarplayerContent.transform);
        daftarPlayerGameobject.transform.localScale = Vector3.one;

        daftarPlayerGameobject.transform.Find("namaPlayerText").GetComponent<Text>().text = newPlayer.NickName;
        daftarPlayerGameobject.transform.Find("IndikatorPlayer").gameObject.SetActive(newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

        daftarPlayerGameobjects.Add(newPlayer.ActorNumber, daftarPlayerGameobject);

        // Update jumlah player di roomInfoText
        roomInfoText.text = "Nama Room: " + PhotonNetwork.CurrentRoom.Name + "\n" +
                            "Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                            PhotonNetwork.CurrentRoom.MaxPlayers;

        // Periksa ulang apakah master client dan jumlah pemain penuh
        if (PhotonNetwork.LocalPlayer.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            TombolMulaiGame.SetActive(true);
        }
    }


    //Update Ketika Player Keluar Dari Room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Update jumlah player di roomInfoText
        roomInfoText.text = "Nama Room: " + PhotonNetwork.CurrentRoom.Name + "\n" +
                            "Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                            PhotonNetwork.CurrentRoom.MaxPlayers;

        Destroy(daftarPlayerGameobjects[otherPlayer.ActorNumber].gameObject);
        daftarPlayerGameobjects.Remove(otherPlayer.ActorNumber);

        // Periksa ulang apakah master client dan jumlah pemain penuh
        if (PhotonNetwork.LocalPlayer.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            TombolMulaiGame.SetActive(false); // Sembunyikan tombol jika pemain keluar dan room belum penuh
        }
    }

    //Ketika Button Keluar Room Di Tekan
    public override void OnLeftRoom()
    {
        ActivatePanel(PanelLogin.name);

        foreach (GameObject daftarPlayerGameobject in daftarPlayerGameobjects.Values)
        {
            Destroy(daftarPlayerGameobject);
            PhotonNetwork.LeaveRoom();
        }

        daftarPlayerGameobjects.Clear();
        daftarPlayerGameobjects = null;

    }

    //Button Ke Panel Join Random
    public void OnJoinRoomButton()
    {
        ActivatePanel(JoinRoomPanel.name);
    }

    //Button Untuk Join Room Setelah Input Nama Room
    public void JoinRoom()
    {
        string roomName = JoinRoomName.text;

        if (!string.IsNullOrEmpty(roomName))
        {
            // Cek apakah player sudah berada di lobby, jika iya maka langsung join
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }

            // Gabung ke room berdasarkan nama room yang dimasukkan
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            Debug.Log("Nama room tidak valid! Harap masukkan nama room yang benar.");
        }
    }

    //Button Join Random
    public void OnJoinRandomRoomButtonClicked()
    {
        ActivatePanel(RoomAcakPanel.name);
        PhotonNetwork.JoinRandomRoom();
    }

    //Ketika Gagal Join, buat room baru dengan ketentuan didalamnya
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        string roomName = "Room " + Random.Range(1000, 10000);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 5;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    #endregion

    public override void OnRoomListUpdate(List<RoomInfo> daftarRoom)
    {
        ClearRoomListView();

        foreach (RoomInfo room in daftarRoom)
        {
            Debug.Log(room.Name);
            if (!room.IsOpen || !room.IsVisible || room.RemovedFromList)
            {
                if (cachedDaftarRoom.ContainsKey(room.Name))
                {
                    cachedDaftarRoom.Remove(room.Name);
                }
            }
            else
            {
                //update cachedDaftar Room
                if (cachedDaftarRoom.ContainsKey(room.Name))
                {
                    cachedDaftarRoom[room.Name] = room;
                }
                //tambahkan room baru ke daftar room yang di-cache
                else
                {
                    cachedDaftarRoom.Add(room.Name, room);

                }
            }
        }

        foreach (RoomInfo room in cachedDaftarRoom.Values)
        {

            GameObject daftarEntriRoomGameobject = Instantiate(daftarEntriRoomPrefab);
            daftarEntriRoomGameobject.transform.SetParent(daftarRoomUtamaGameobject.transform);
            daftarEntriRoomGameobject.transform.localScale = Vector3.one;
            daftarEntriRoomGameobject.transform.Find("NamaRoomText").GetComponent<Text>().text = room.Name;
            daftarEntriRoomGameobject.transform.Find("MaksPlayerText").GetComponent<Text>().text = room.PlayerCount + " / " + room.MaxPlayers;
            daftarEntriRoomGameobject.transform.Find("TombolGabungRoom").GetComponent<Button>().onClick.AddListener(() => OnJoinRoomButtonClicked(room.Name));

            daftarRoomGameObjects.Add(room.Name, daftarEntriRoomGameobject);

        }
    }

    void OnJoinRoomButtonClicked(string _namaRoom)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        PhotonNetwork.JoinRoom(_namaRoom);
    }
    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }

    public override void OnLeftLobby()
    {
        ClearRoomListView();
        cachedDaftarRoom.Clear();
    }

    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        ActivatePanel(DaftarRoomPanel.name);
    }

    void ClearRoomListView()
    {
        foreach (var daftarRoomGameobject in daftarRoomGameObjects.Values)
        {
            Destroy(daftarRoomGameobject);
        }

        daftarRoomGameObjects.Clear();
    }

    //Manager Active Panel
    public void ActivatePanel(string panelToBeActivated)
    {
        PanelLogin.SetActive(panelToBeActivated.Equals(PanelLogin.name));
        RoomPanel.SetActive(panelToBeActivated.Equals(RoomPanel.name));
        BuatRoomPanel.SetActive(panelToBeActivated.Equals(BuatRoomPanel.name));
        GamePanel.SetActive(panelToBeActivated.Equals(GamePanel.name));
        DaftarRoomPanel.SetActive(panelToBeActivated.Equals(DaftarRoomPanel.name));
        RoomAcakPanel.SetActive(panelToBeActivated.Equals(RoomAcakPanel.name));
        JoinRoomPanel.SetActive(panelToBeActivated.Equals(JoinRoomPanel.name));
    }

    #region STATUS PLAYER
    public override void OnConnected()
    {
        Debug.Log("Terhubung ke Internet");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Terhubung ke Photon");
        ActivatePanel(RoomPanel.name);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " Berhasil Membuat Room.");
    }
    #endregion
}
