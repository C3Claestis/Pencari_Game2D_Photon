using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Unity.VisualScripting;
public class PlayerMovement : MonoBehaviourPun, IPunObservable
{
    float dirX;
    float dirY;
    [SerializeField] float speed;
    [SerializeField] Transform sprite_karakter;
    [SerializeField] Animator animator_karakter;
    [SerializeField] Text nickPlayer;
    [SerializeField] GameObject teleIcon;
    private Rigidbody2D rb;
    private GameObject cameraFollow;
    private Vector3 networkPosition;
    private Vector2 lastVelocity;
    public bool SetTeleActive(bool teleIcons)
    {
        teleIcon.SetActive(teleIcons);
        return teleIcon;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Kamera hanya mengikuti pemain lokal 
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
            // Interpolasi gerakan pemain lain
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
        }
    }

    void HandleMovement()
    {
        dirX = Input.GetAxis("Horizontal");
        dirY = Input.GetAxis("Vertical");

        Vector2 velocity = new Vector2(dirX * speed, dirY * speed);
        rb.velocity = velocity;

        // Hanya kirim data saat ada perubahan kecepatan
        if (velocity != lastVelocity)
        {
            lastVelocity = velocity;
        }
    }

    void HandleAnimation()
    {
        bool isWalking = (dirX != 0 || dirY != 0);
        animator_karakter.SetBool("Walk", isWalking);
        FlipSprite();
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)  // Pemain lokal menulis data
        {
            stream.SendNext(transform.position);
            stream.SendNext(rb.velocity);
        }
        else  // Pemain remote membaca data
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            rb.velocity = (Vector2)stream.ReceiveNext();
        }
    }
}
