using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;              // Nhân vật cần theo dõi

    [Header("Camera Position Settings")]
    public float distance = 5f;           // Khoảng cách ra sau lưng nhân vật
    public float height = 2f;             // Độ cao so với nhân vật

    [Header("Smooth Settings")]
    public float followSmoothness = 8f;   // Độ mượt khi di chuyển
    public float rotateSmoothness = 12f;  // Độ mượt khi quay nhìn

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // --- Tính vị trí mong muốn của camera ---
        Vector3 desiredPosition = target.position - target.forward * distance + Vector3.up * height;

        // --- Di chuyển camera mượt đến vị trí mong muốn ---
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 1f / followSmoothness);

        // --- Quay camera nhìn về phía nhân vật (phần đầu hoặc ngực) ---
        Vector3 lookPoint = target.position + Vector3.up * 1.5f;
        Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - transform.position);

        // --- Xoay mượt ---
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, rotateSmoothness * Time.deltaTime);
    }
}
