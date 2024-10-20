using UnityEngine;
using Photon.Pun;
public class PortalFinishPlayer : MonoBehaviourPunCallbacks
{
    private GameObject panel_crew;
    private GameObject panel_treasure;
    private GameManager gameManager;

    void Start()
    {
        if (photonView.IsMine)
        {
            // Menemukan panel secara manual melalui parent mereka
            GameObject panelWinnerCrewParent = GameObject.Find("Panel-WinnerCrew");
            panel_treasure = GameObject.Find("Panel-Game");

            // Mengambil child-nya walaupun awalnya nonaktif
            panel_crew = panelWinnerCrewParent.transform.GetChild(0).gameObject;

            // Memastikan panel tetap tidak aktif di awal
            panel_crew.SetActive(false);

            // Get reference to GameManager
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Portal") && photonView.IsMine)
        {
            // Disable the game panel and show the crew panel
            panel_treasure.SetActive(false);
            panel_crew.SetActive(true);

            // Add this player to the crew order via GameManager
            gameManager.AddPlayerToCrew(PhotonNetwork.LocalPlayer.NickName);  // Add the player's name

            StartCoroutine(gameManager.WaitForPlayerCrewCountIncrease(1));

            // Send an RPC to hide this player across all clients
            photonView.RPC("HidePlayer", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    [PunRPC]
    private void HidePlayer(int actorNumber)
    {
        foreach (PhotonView pView in FindObjectsOfType<PhotonView>())
        {
            Debug.Log("Checking PhotonView: " + pView.gameObject.name);
            if (pView.Owner.ActorNumber == actorNumber && pView.gameObject.CompareTag("Player"))
            {
                Debug.Log("Hiding Player: " + pView.gameObject.name);
                pView.gameObject.SetActive(false);  // Hide player GameObject across all clients
                break;
            }
        }
    }

}
