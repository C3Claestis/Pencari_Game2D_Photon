using Photon.Pun;  // Import Photon PUN
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviourPun, IPunObservable  // Tambahkan IPunObservable untuk sinkronisasi manual
{
    float speed = 5f;
    float dirX;
    float dirY;
    [SerializeField] Transform sprite_karakter;
    [SerializeField] Animator animator_karakter;
    [SerializeField] Text nickPlayer;
    // [SerializeField] GameManager gameManager;
    private Rigidbody2D rb;
    private GameObject cameraFollow;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Kamera hanya mengikuti pemain lokal 
        if (photonView.IsMine)
        {
            // Set nickname untuk pemain lokal
            nickPlayer.text = PhotonNetwork.NickName;
            photonView.RPC("SyncNickname", RpcTarget.Others, PhotonNetwork.NickName);

            // gameManager = GameObject.Find("Manager").GetComponent<GameManager>();
            cameraFollow = GameObject.Find("Main Camera");
            cameraFollow.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
        }
        else
        {
            // Set nickname untuk pemain lain
            nickPlayer.text = photonView.Owner.NickName;
        }
    }

    void Update()
    {
        if (photonView.IsMine)  // Hanya untuk pemain lokal
        {
            HandleMovement();
            HandleAnimation();
            HandleCameraFollow();
        }
    }

    void HandleMovement()
    {
        dirX = Input.GetAxis("Horizontal");
        dirY = Input.GetAxis("Vertical");

        if (dirX == 0 && dirY == 0)
        {
            // Tidak ada input, tingkatkan drag agar karakter tetap diam
            rb.velocity = Vector2.zero;  // Set kecepatan ke nol
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            rb.drag = 10f;  // Tingkatkan drag untuk mengurangi gesekan dari gaya eksternal (dorongan dari pemain lain)

            // Kirim RPC untuk menyinkronkan status diam ke pemain lain
            photonView.RPC("SyncMovement", RpcTarget.Others, Vector2.zero, 0, true);
        }
        else
        {
            // Ada input, gerakkan pemain dan atur drag kembali ke nilai normal
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.drag = 0f;  // Atur drag ke nol agar pemain bisa bergerak bebas
            Vector2 velocity = new Vector2(dirX * speed, dirY * speed);
            rb.velocity = velocity;  // Gerakkan karakter sesuai input

            // Kirim RPC untuk menyinkronkan gerakan ke pemain lain
            photonView.RPC("SyncMovement", RpcTarget.Others, velocity, Mathf.RoundToInt(speed), false);
        }
    }

    void HandleAnimation()
    {
        bool isWalking = (dirX != 0 || dirY != 0);
        animator_karakter.SetBool("Walk", isWalking);

        photonView.RPC("SyncAnimation", RpcTarget.Others, isWalking);

        FlipSprite();
    }

    // Kamera hanya mengikuti pemain lokal
    void HandleCameraFollow()
    {
        cameraFollow.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
    }

    void FlipSprite()
    {
        if (dirX < 0)
        {
            sprite_karakter.localScale = new Vector3(-1f, 1f, 1f);
            // Sinkronisasi flip dengan RPC
            photonView.RPC("SyncFlip", RpcTarget.Others, sprite_karakter.localScale);
        }
        if (dirX > 0)
        {
            sprite_karakter.localScale = new Vector3(1f, 1f, 1f);
            // Sinkronisasi flip dengan RPC
            photonView.RPC("SyncFlip", RpcTarget.Others, sprite_karakter.localScale);
        }
    }

    [PunRPC]
    void SyncMovement(Vector2 syncedVelocity, int syncedSpeed, bool isIdle)
    {
        if (!photonView.IsMine)  // Pastikan hanya pemain lain yang menerima pembaruan
        {
            if (isIdle)
            {
                // Pemain lain dalam keadaan diam
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                rb.drag = 10f;  // Sinkronisasi drag untuk pemain diam
            }
            else
            {
                // Pemain lain sedang bergerak
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.drag = 0f;
                rb.velocity = syncedVelocity;  // Terapkan kecepatan yang disinkronkan
            }
        }
    }

    // RPC untuk sinkronisasi animasi
    [PunRPC]
    void SyncAnimation(bool isWalking)
    {
        animator_karakter.SetBool("Walk", isWalking);
    }

    // RPC untuk sinkronisasi flip (localScale)
    [PunRPC]
    void SyncFlip(Vector3 flipScale)
    {
        sprite_karakter.localScale = flipScale;
    }

    [PunRPC]
    void SyncNickname(string nickname)
    {
        nickPlayer.text = nickname;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)  // Pemain lokal menulis data
        {
            stream.SendNext(transform.position);
            stream.SendNext(rb.velocity);
        }
        else  // Pemain remote membaca data
        {
            transform.position = (Vector3)stream.ReceiveNext();
            rb.velocity = (Vector2)stream.ReceiveNext();
        }
    }
}
