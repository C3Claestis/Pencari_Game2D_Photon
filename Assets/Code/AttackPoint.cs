using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AttackPoint : MonoBehaviourPun
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)  // Only MasterClient triggers RPC
            {
                // Panggil RPC untuk knock player
                player.photonView.RPC("ApplyKnock", RpcTarget.All, true);
            }
        }
    }
}
