using UnityEngine;

public class RotatePicture : MonoBehaviour
{
    [Tooltip("Số độ xoay mỗi lần (thường 90).")]
    public float rotationStep = 90f;

    [Tooltip("Góc Y đúng **tương đối** so với orientation ban đầu (ví dụ 0, 90, 180, 270).")]
    public float correctAngle = 0f;

    [Tooltip("Transform thực sự xoay (object con).")]
    public Transform targetToRotate;

    [HideInInspector]
    public bool isCorrect = false;

    [Header("Audio (optional)")]
    public AudioClip rotateSfx;
    public AudioClip correctSfx;
    private AudioSource audioSource;

    // Lưu góc Y ban đầu của target để so sánh tương đối
    private float initialY = 0f;

    // Ngưỡng so sánh (độ) để chấp nhận là "đúng"
    private const float tolerance = 1f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (targetToRotate != null)
        {
            initialY = Mathf.Repeat(targetToRotate.eulerAngles.y, 360f);
        }
    }

    public void Rotate()
    {
        // safety checks
        if (PuzzleManager.Instance == null)
        {
            Debug.LogWarning("PuzzleManager.Instance == null — chưa có PuzzleManager trong scene.");
            return;
        }

        if (PuzzleManager.Instance.IsPuzzleCompleted())
        {
            Debug.Log($"⛔ {name} không thể xoay — puzzle đã hoàn thành.");
            return;
        }

        if (targetToRotate == null)
        {
            Debug.LogWarning($"{name}: Chưa gán targetToRotate!");
            return;
        }

        // Xoay
        targetToRotate.Rotate(0f, rotationStep, 0f, Space.Self);

        // Lấy góc Y hiện tại, chuẩn hóa 0..360
        float currentY = Mathf.Repeat(targetToRotate.eulerAngles.y, 360f);

        // Tính góc tương đối so với góc ban đầu
        float relativeY = Mathf.Repeat(currentY - initialY + 360f, 360f);

        // Snap relativeY về bội của rotationStep để tránh sai số
        float nearestStepCount = Mathf.Round(relativeY / rotationStep);
        float snappedRelative = Mathf.Repeat(nearestStepCount * rotationStep, 360f);

        // Cập nhật isCorrect: so sánh snappedRelative với correctAngle
        isCorrect = Mathf.Abs(Mathf.DeltaAngle(snappedRelative, Mathf.Repeat(correctAngle, 360f))) < tolerance;

        Debug.Log($"{name} rotated -> currentY={currentY:F2}, initialY={initialY:F2}, relativeY={relativeY:F2}, snappedRelative={snappedRelative:F2}, isCorrect={isCorrect}");

        // Play sfx
        if (audioSource != null && rotateSfx != null) audioSource.PlayOneShot(rotateSfx);
        if (isCorrect && audioSource != null && correctSfx != null) audioSource.PlayOneShot(correctSfx);

        // Nếu muốn, "snap" transform chính xác tới giá trị snappedRelative (để loại số lẻ)
        // Ta cần đặt y mới = initialY + snappedRelative  (và chuẩn hóa)
        float finalY = Mathf.Repeat(initialY + snappedRelative, 360f);
        Vector3 e = targetToRotate.eulerAngles;
        targetToRotate.eulerAngles = new Vector3(e.x, finalY, e.z);

        // Thông báo PuzzleManager kiểm tra
        PuzzleManager.Instance.CheckPuzzleState();
    }
}
