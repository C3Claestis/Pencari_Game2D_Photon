using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System;

public class PlayerMovement : MonoBehaviourPun, IPunObservable
{
    float dirX;
    float dirY;

    [Header("=========== ATRIBUT SCRIPT ===========")]
    [SerializeField] bool isPlayer;
    [SerializeField] float speed;

    [Header("=========== ATRIBUT KARAKTER ===========")]
    [SerializeField] Transform sprite_karakter;
    [SerializeField] Animator animator_karakter;
    [SerializeField] Text nickPlayer;

    [Header("=========== ATRIBUT ICON AND TRIGGER ===========")]
    [SerializeField] GameObject teleIcon;
    [SerializeField] GameObject pointAttack;
    private GameObject minimapItem;
    private Rigidbody2D rb;
    private GameObject cameraFollow;
    private Vector3 networkPosition;
    private Vector2 lastVelocity;
    private bool isAttacking;
    private bool isCanFixing;
    private bool Fixing;
    private bool isKnock;

    public bool GetKnock() => isKnock;
    public void SetKnock(bool knock) => this.isKnock = knock;
    public void SetCanFixing(bool canfix) => this.isCanFixing = canfix;
    public bool SetTeleActive(bool teleIcons)
    {
        teleIcon.SetActive(teleIcons);
        return teleIcon;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (photonView.IsMine)
        {
            nickPlayer.text = PhotonNetwork.NickName;
            cameraFollow = GameObject.Find("Main Camera");
            cameraFollow.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
        }
        else
        {
            nickPlayer.text = photonView.Owner.NickName;
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            HandleMovement();
            HandleAnimation();
            HandleCameraFollow();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
        }
    }

    void HandleMovement()
    {
        if (isAttacking || Fixing || isKnock)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        dirX = Input.GetAxis("Horizontal");
        dirY = Input.GetAxis("Vertical");

        Vector2 velocity = new Vector2(dirX * speed, dirY * speed);
        rb.velocity = velocity;

        if (velocity != lastVelocity)
        {
            lastVelocity = velocity;
        }
    }

    void HandleAnimation()
    {
        bool isWalking = (dirX != 0 || dirY != 0);
        animator_karakter.SetBool("Walk", isWalking);

        // Sinkronisasi animasi jalan menggunakan RPC
        photonView.RPC("SyncWalking", RpcTarget.Others, isWalking);

        FlipSprite();

        if (!isPlayer)
        {
            // Jika minimapItem tidak ada (null), cari di awal atau simpan referensi secara manual
            if (minimapItem == null)
            {
                Transform minimapPanel = GameObject.Find("Panel-Game").transform;
                if (minimapPanel.childCount > 0)
                {
                    minimapItem = minimapPanel.GetChild(0).gameObject;
                }
                else
                {
                    Debug.LogWarning("Tidak ada child pada Panel-Impostor");
                }
            }

            if (Input.GetKey(KeyCode.Q))
            {
                pointAttack.SetActive(true);
                animator_karakter.SetBool("Attack", true);
                isAttacking = true;

                photonView.RPC("SyncAttack", RpcTarget.Others, true);

                Invoke(nameof(EndAttack), animator_karakter.GetCurrentAnimatorStateInfo(0).length);
            }
            // Aktifkan minimapItem dengan memeriksa key input
            if (Input.GetKey(KeyCode.M))
            {
                if (minimapItem != null)
                {
                    minimapItem.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("minimapItem tidak ditemukan atau tidak di-set");
                }
            }
            else if (Input.GetKeyUp(KeyCode.M))
            {
                if (minimapItem != null)
                {
                    minimapItem.SetActive(false);
                }
            }
        }
        else
        {
            if (isCanFixing)
            {
                if (Input.GetKey(KeyCode.Space) && !Fixing)
                {
                    PlayerAttack(true);
                }
                else if (Input.GetKeyUp(KeyCode.Space) && Fixing)
                {
                    PlayerAttack(false);
                }
            }
        }
        if (isKnock)
        {
            animator_karakter.SetBool("Knock", true);
        }
        else
        {
            animator_karakter.SetBool("Knock", false);
        }
    }

    void PlayerAttack(bool condition)
    {
        pointAttack.SetActive(condition);
        animator_karakter.SetBool("Attack", condition);
        Fixing = condition;

        photonView.RPC("SyncAttack", RpcTarget.Others, condition);
    }

    void EndAttack()
    {
        isAttacking = false;
        animator_karakter.SetBool("Attack", false);
        pointAttack.SetActive(false);

        photonView.RPC("SyncAttack", RpcTarget.Others, false);
    }

    void HandleCameraFollow()
    {
        cameraFollow.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
    }

    void FlipSprite()
    {
        if (dirX < 0)
        {
            sprite_karakter.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (dirX > 0)
        {
            sprite_karakter.localScale = new Vector3(1f, 1f, 1f);
        }
    }
    [PunRPC]
    void SyncWalking(bool isWalking)
    {
        animator_karakter.SetBool("Walk", isWalking);
    }

    [PunRPC]
    void SyncAttack(bool isAttacking)
    {
        pointAttack.SetActive(isAttacking);
        animator_karakter.SetBool("Attack", isAttacking);
    }

    // RPC untuk mengaktifkan knock pada player
    [PunRPC]
    void ApplyKnock(bool knock)
    {
        isKnock = knock;
        if (isKnock)
        {
            // Logika ketika terkena knock
            rb.velocity = Vector2.zero;
            animator_karakter.SetBool("Knock", true);
            // Tambahkan ke CountPlayerKnock jika player knock
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.SetCountPlayerKnock();
            }
        }
        else
        {
            animator_karakter.SetBool("Knock", false);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(rb.velocity);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            rb.velocity = (Vector2)stream.ReceiveNext();
        }
    }
}
