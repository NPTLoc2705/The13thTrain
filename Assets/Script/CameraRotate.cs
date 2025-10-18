using UnityEngine;

public class CameraWindFloat : MonoBehaviour
{
    [Header("Target to Look At")]
    public Transform target;

    [Header("Float Settings")]
    public float horizontalAmplitude = 0.15f; // biên độ đung đưa trái phải
    public float verticalAmplitude = 0.1f;    // biên độ lên xuống
    public float depthAmplitude = 0.05f;      // biên độ tiến lùi
    public float swaySpeed = 0.2f;            // tốc độ "gió", càng nhỏ càng chậm và êm

    private Vector3 startPosition;

    void Start()
    {
        // Ghi lại vị trí ban đầu của camera
        startPosition = transform.position;
    }

    void Update()
    {
        if (target == null) return;

        float t = Time.time * swaySpeed;

        // Dao động nhẹ nhàng theo sóng sin/cos
        float x = Mathf.Sin(t) * horizontalAmplitude;
        float y = Mathf.Sin(t * 0.7f) * verticalAmplitude;
        float z = Mathf.Cos(t * 0.5f) * depthAmplitude;

        // Vị trí mới
        Vector3 swayOffset = new Vector3(x, y, z);
        transform.position = startPosition + swayOffset;

        // Luôn nhìn về đối tượng trung tâm
        transform.LookAt(target.position);
    }
}
