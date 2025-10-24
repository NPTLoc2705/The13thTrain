using UnityEngine;
using System.Collections.Generic;

public class PaintingProgressController : MonoBehaviour
{
    [Header("Painting Models")]
    [Tooltip("Model bức tranh chưa hoàn chỉnh (có mảnh bị thiếu)")]
    public GameObject incompletePainting;

    [Tooltip("Model bức tranh hoàn chỉnh")]
    public GameObject completePainting;

    [Header("Piece Settings")]
    [Tooltip("Danh sách ID các mảnh cần nhặt để hoàn thiện bức tranh này")]
    public List<string> requiredPieceIDs = new List<string>();

    [Header("Visual Effects (Optional)")]
    public ParticleSystem completeEffect;
    public AudioSource audioSource;
    public AudioClip completeSound;

    private bool isComplete = false;

    void Start()
    {
        // Khởi tạo trạng thái ban đầu
        if (incompletePainting != null)
        {
            incompletePainting.SetActive(true);
            Debug.Log($"✅ Incomplete painting set to ACTIVE: {incompletePainting.name}");
        }
        else
        {
            Debug.LogError("❌ Incomplete Painting is NULL! Drag the model in Inspector.");
        }

        if (completePainting != null)
        {
            completePainting.SetActive(false);
            Debug.Log($"✅ Complete painting set to INACTIVE: {completePainting.name}");
        }
        else
        {
            Debug.LogError("❌ Complete Painting is NULL! Drag the model in Inspector.");
        }

        Debug.Log($"📋 Required pieces: {string.Join(", ", requiredPieceIDs)}");
    }

    void Update()
    {
        // Kiểm tra liên tục xem đã nhặt đủ mảnh chưa
        if (!isComplete && CheckAllPiecesCollected())
        {
            CompletePainting();
        }
    }

    bool CheckAllPiecesCollected()
    {
        if (PickupManager.Instance == null)
        {
            Debug.LogWarning("⚠️ PickupManager.Instance is null!");
            return false;
        }

        if (requiredPieceIDs.Count == 0)
        {
            Debug.LogWarning("⚠️ Required Piece IDs is empty! Add piece IDs in Inspector.");
            return false;
        }

        // Kiểm tra từng mảnh có trong danh sách đã nhặt không
        foreach (string pieceID in requiredPieceIDs)
        {
            if (!PickupManager.Instance.IsCollected(pieceID))
            {
                return false; // Còn mảnh chưa nhặt
            }
        }

        return true; // Đã nhặt đủ tất cả mảnh
    }

    void CompletePainting()
    {
        isComplete = true;
        Debug.Log("✨✨✨ CompletePainting() called! ✨✨✨");

        // Ẩn tranh chưa hoàn chỉnh
        if (incompletePainting != null)
        {
            incompletePainting.SetActive(false);
            Debug.Log($"❌ Incomplete painting HIDDEN: {incompletePainting.name}");
        }

        // Hiển thị tranh hoàn chỉnh
        if (completePainting != null)
        {
            completePainting.SetActive(true);
            Debug.Log($"✅ Complete painting SHOWN: {completePainting.name}");
            Debug.Log($"   Position: {completePainting.transform.position}");
            Debug.Log($"   Active in hierarchy: {completePainting.activeInHierarchy}");
            Debug.Log($"   Active self: {completePainting.activeSelf}");

            // Kiểm tra renderer
            Renderer[] renderers = completePainting.GetComponentsInChildren<Renderer>();
            Debug.Log($"   Found {renderers.Length} renderers");
            foreach (Renderer r in renderers)
            {
                Debug.Log($"     - {r.name}: enabled={r.enabled}");
            }
        }
        else
        {
            Debug.LogError("❌❌❌ COMPLETE PAINTING IS NULL! Cannot show it!");
        }

        // Phát hiệu ứng
        if (completeEffect != null)
        {
            completeEffect.transform.position = transform.position;
            completeEffect.Play();
            Debug.Log("🎆 Particle effect played!");
        }

        // Phát âm thanh
        if (audioSource != null && completeSound != null)
        {
            audioSource.PlayOneShot(completeSound);
            Debug.Log("🔊 Sound played!");
        }
    }

    // Gọi hàm này từ ngoài nếu muốn force complete (để test)
    public void ForceComplete()
    {
        Debug.Log("🔨 FORCE COMPLETE called!");
        if (!isComplete)
        {
            CompletePainting();
        }
    }

    // Debug: Hiển thị trạng thái trong Editor
    void OnDrawGizmos()
    {
        if (isComplete)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}