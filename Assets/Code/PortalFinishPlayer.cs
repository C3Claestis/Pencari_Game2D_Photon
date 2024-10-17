using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PortalFinishPlayer : MonoBehaviourPunCallbacks
{
    private GameObject panel_crew;
    private GameObject panel_treasure;
    // Start is called before the first frame update
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
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Portal"))
        {
            if (photonView.IsMine)
            {
                panel_treasure.SetActive(false);
                panel_crew.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }
}
