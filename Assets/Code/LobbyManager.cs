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
    public InputField maxPlayerInputField;

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
    }

    // Update is called once per frame
    private void Update()
    {
        //Mengecek Status Koneksi
        statusKoneksi.text = "Status Koneksi: " + PhotonNetwork.NetworkClientState;
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
    //Tombol buat room
    public void OnRoomCreateButtonClicked()
    {
        string NamaRoom = NamaRoomInputField.text;

        if (string.IsNullOrEmpty(NamaRoom))
        {
            NamaRoom = "Room " + Random.Range(1000, 10000);

        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)int.Parse(maxPlayerInputField.text);

        PhotonNetwork.CreateRoom(NamaRoom, roomOptions);
    }
    public void OnCancelButtonClicked()
    {
        ActivatePanel(RoomPanel.name);
    }

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

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Bergabung dengan " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(GamePanel.name);

        if (PhotonNetwork.LocalPlayer.IsMasterClient && PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            TombolMulaiGame.SetActive(true);
        }
        else
        {
            TombolMulaiGame.SetActive(false);
        }

        roomInfoText.text = "Nama Room : " + PhotonNetwork.CurrentRoom.Name + "\n" +
                            "Players : " +
                            PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                            PhotonNetwork.CurrentRoom.MaxPlayers;

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
            if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                daftarPlayerGameobject.transform.Find("IndikatorPlayer").gameObject.SetActive(true);
            }
            else
            {
                daftarPlayerGameobject.transform.Find("IndikatorPlayer").gameObject.SetActive(false);
            }

            daftarPlayerGameobjects.Add(player.ActorNumber, daftarPlayerGameobject);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomInfoText.text = "Nama Room: " + PhotonNetwork.CurrentRoom.Name + " " +
                           "Maks.players: " +
                           PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                           PhotonNetwork.CurrentRoom.MaxPlayers;

        GameObject daftarPlayerGameobject = Instantiate(daftarplayerPrefab);
        daftarPlayerGameobject.transform.SetParent(daftarplayerContent.transform);
        daftarPlayerGameobject.transform.localScale = Vector3.one;

        daftarPlayerGameobject.transform.Find("namaPlayerText").GetComponent<Text>().text = newPlayer.NickName;
        if (newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            daftarPlayerGameobject.transform.Find("IndikatorPlyer").gameObject.SetActive(true);
        }
        else
        {
            daftarPlayerGameobject.transform.Find("IndikatorPlayer").gameObject.SetActive(false);
        }

        daftarPlayerGameobjects.Add(newPlayer.ActorNumber, daftarPlayerGameobject);

        // Periksa ulang apakah pemain MasterClient dan apakah jumlah pemain sudah penuh
        if (PhotonNetwork.LocalPlayer.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            TombolMulaiGame.SetActive(true);
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomInfoText.text = "Nama Room: " + PhotonNetwork.CurrentRoom.Name + " " +
                            "Maks.players: " +
                           PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                           PhotonNetwork.CurrentRoom.MaxPlayers;

        Destroy(daftarPlayerGameobjects[otherPlayer.ActorNumber].gameObject);
        daftarPlayerGameobjects.Remove(otherPlayer.ActorNumber);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            TombolMulaiGame.SetActive(true);
        }
    }
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
    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnJoinRandomRoomButtonClicked()
    {
        ActivatePanel(RoomAcakPanel.name);
        PhotonNetwork.JoinRandomRoom();
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


    void OnJoinRoomButtonClicked(string _namaRoom)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        PhotonNetwork.JoinRoom(_namaRoom);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        string roomName = "Room " + Random.Range(1000, 10000);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 5;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    void ClearRoomListView()
    {
        foreach (var daftarRoomGameobject in daftarRoomGameObjects.Values)
        {
            Destroy(daftarRoomGameobject);
        }

        daftarRoomGameObjects.Clear();
    }

    public void ActivatePanel(string panelToBeActivated)
    {
        PanelLogin.SetActive(panelToBeActivated.Equals(PanelLogin.name));
        RoomPanel.SetActive(panelToBeActivated.Equals(RoomPanel.name));
        BuatRoomPanel.SetActive(panelToBeActivated.Equals(BuatRoomPanel.name));
        GamePanel.SetActive(panelToBeActivated.Equals(GamePanel.name));
        DaftarRoomPanel.SetActive(panelToBeActivated.Equals(DaftarRoomPanel.name));
        RoomAcakPanel.SetActive(panelToBeActivated.Equals(RoomAcakPanel.name));
    }
}
