using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    [Header("=========== ATRIBUT ============")]
    [SerializeField] GameObject panelChat;
    [SerializeField] InputField inputField;
    [SerializeField] GameObject message;
    [SerializeField] GameObject content;
    [SerializeField] Text nicname;

    private bool isChatActive = false;

    void Start()
    {
        nicname.text = PhotonNetwork.NickName;
        panelChat.SetActive(false); // Pastikan panelChat tidak aktif saat awal
        inputField.onEndEdit.AddListener(HandleInputEndEdit);
    }

    void Update()
    {
        // Aktifkan atau nonaktifkan panelChat dengan tombol Enter dan Escape
        if (Input.GetKeyDown(KeyCode.Return) && !isChatActive)
        {
            ActivateChat();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isChatActive)
        {
            DeactivateChat();
        }

        // Kirim pesan jika InputField aktif dan Enter ditekan
        if (Input.GetKeyDown(KeyCode.Return) && isChatActive && inputField.isFocused)
        {
            SendMessage();
        }
    }

    void ActivateChat()
    {
        panelChat.SetActive(true);
        isChatActive = true;
        inputField.ActivateInputField(); // Fokuskan ke InputField
    }

    void DeactivateChat()
    {
        panelChat.SetActive(false);
        isChatActive = false;
        inputField.text = ""; // Kosongkan InputField jika diperlukan
    }

    void HandleInputEndEdit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(text))
        {
            SendMessage();
        }
    }

    public void SendMessage()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            GetComponent<PhotonView>().RPC("RPC_GetMessage", RpcTarget.AllBuffered, PhotonNetwork.NickName, inputField.text);
            inputField.text = ""; // Kosongkan InputField setelah mengirim pesan
            inputField.ActivateInputField(); // Fokus kembali ke InputField
        }
    }

    [PunRPC]
    public void RPC_GetMessage(string nickname, string isichat)
    {
        GameObject M = Instantiate(message, Vector3.zero, Quaternion.identity, content.transform);
        M.GetComponent<Message>().nickname.text = nickname;
        M.GetComponent<Message>().myMessage.text = isichat;
    }
}
