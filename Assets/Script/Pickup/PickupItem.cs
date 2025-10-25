using UnityEngine;

[System.Serializable]
public class PickupItem : MonoBehaviour
{
    [Header("Thông tin vật phẩm")]
    public string itemID;
    public string itemName;
    public GameObject model;

    [Tooltip("Nếu true thì vật phẩm sẽ biến mất khi nhặt")]
    public bool destroyOnPickup = true;

    [Header("Sound Settings")]
    public AudioClip pickupSound;
    [Range(0f, 1f)]
    public float soundVolume = 1f;
    public bool spatialSound = false;

    [Header("Monologue Settings - TRƯỚC khi nhặt")]
    [Tooltip("Hiển thị suy nghĩ TRƯỚC khi nhặt (nhấn E lần 1)")]
    public bool showMonologueBeforePickup = false;

    [Tooltip("Nội dung suy nghĩ TRƯỚC khi nhặt")]
    [TextArea(3, 5)]
    public string[] monologueBeforePickup = new string[]
    {
        "Đây là gì nhỉ?",
        "Có vẻ quan trọng..."
    };

    [Header("Monologue Settings - SAU khi nhặt")]
    [Tooltip("Hiển thị suy nghĩ SAU khi nhặt xong")]
    public bool showMonologueAfterPickup = false;

    [Tooltip("Nội dung suy nghĩ SAU khi nhặt")]
    [TextArea(3, 5)]
    public string[] monologueAfterPickup = new string[]
    {
        "Đã lấy được rồi.",
        "Hy vọng nó sẽ hữu ích..."
    };

    private AudioSource audioSource;
    public bool isCollected = false;
    private bool hasShownBeforeMonologue = false; // Đã hiện monologue trước chưa

    void Awake()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource
        audioSource.playOnAwake = false;
        audioSource.clip = pickupSound;
        audioSource.volume = soundVolume;
        audioSource.spatialBlend = spatialSound ? 1f : 0f;

        // For 3D sound settings
        if (spatialSound)
        {
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
    }

    public virtual void OnPickup()
    {
        if (isCollected) return;

        // CASE 1: Có monologue trước khi nhặt VÀ chưa hiển thị
        if (showMonologueBeforePickup && !hasShownBeforeMonologue && monologueBeforePickup.Length > 0)
        {
            if (CharacterMonologue.Instance != null)
            {
                hasShownBeforeMonologue = true;
                CharacterMonologue.Instance.ShowMonologueWithCallback(monologueBeforePickup, () =>
                {
                    // SAU KHI monologue trước xong, hiển thị prompt để nhặt tiếp
                    if (TextManager.Instance != null)
                    {
                        TextManager.Instance.ShowPrompt($"[E] Nhặt {itemName}");
                    }
                });

                // Ẩn prompt hiện tại trong lúc xem monologue
                if (TextManager.Instance != null)
                {
                    TextManager.Instance.HidePrompt();
                }

                Debug.Log($"📖 Đang xem suy nghĩ về {itemName}. Nhấn E lần nữa để nhặt.");
                return; // DỪNG LẠI, chưa nhặt
            }
        }

        // CASE 2: Đã xem monologue trước (hoặc không có), giờ nhặt thật
        isCollected = true;

        // Ẩn prompt ngay khi bắt đầu nhặt
        if (TextManager.Instance != null)
        {
            TextManager.Instance.HidePrompt();
        }

        // CASE 2A: Có monologue SAU khi nhặt
        if (showMonologueAfterPickup && monologueAfterPickup.Length > 0)
        {
            // Nhặt trước
            PerformPickup();

            // Rồi hiển thị monologue sau
            if (CharacterMonologue.Instance != null)
            {
                CharacterMonologue.Instance.ShowMonologueWithCallback(monologueAfterPickup, null);
            }
        }
        // CASE 2B: Không có monologue sau, nhặt luôn
        else
        {
            PerformPickup();
        }
    }

    /// <summary>
    /// Thực hiện hành động nhặt đồ
    /// </summary>
    private void PerformPickup()
    {
        Debug.Log($"✅ Đã nhặt: {itemName}");

        // Play sound effect if assigned
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound, soundVolume);
            Debug.Log($"🔊 Playing sound for {itemName}");
        }
        else
        {
            Debug.LogWarning($"⚠ No pickup sound assigned or AudioSource missing for {itemName}!");
        }

        // Just hide the object, don't destroy it yet
        if (destroyOnPickup)
        {
            // Hide visually but keep the GameObject alive for sound playback
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }

            // Disable collider so it can't be picked up again
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            // Destroy after sound finishes
            float delay = pickupSound != null ? pickupSound.length + 0.1f : 0.1f;
            Destroy(gameObject, delay);
        }
    }

    // Reset state khi cần (ví dụ khi load lại game)
    public void ResetState()
    {
        hasShownBeforeMonologue = false;
        isCollected = false;
    }
}