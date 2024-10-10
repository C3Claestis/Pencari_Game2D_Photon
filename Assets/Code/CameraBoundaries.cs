using UnityEngine;

public class CameraBoundaries : MonoBehaviour
{
    [SerializeField] Transform cameraTransform; // Referensi ke transformasi kamera
    [SerializeField] BoxCollider2D boundary;    // Collider untuk area batas kamera
    private float halfHeight;
    private float halfWidth;

    void Start()
    {
        // Mendapatkan separuh tinggi dan lebar kamera
        Camera cam = Camera.main;
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
    }

    void LateUpdate()
    {
        // Mendapatkan batas dari BoxCollider2D
        Bounds bounds = boundary.bounds;

        // Menghitung posisi kamera agar tetap berada dalam batas collider
        float clampedX = Mathf.Clamp(cameraTransform.position.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth);
        float clampedY = Mathf.Clamp(cameraTransform.position.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight);

        // Mengatur posisi kamera
        cameraTransform.position = new Vector3(clampedX, clampedY, cameraTransform.position.z);
    }
}
