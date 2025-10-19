using UnityEngine;

[System.Serializable]
public class PickupItem : MonoBehaviour
{
    [Header("Thông tin vật phẩm")]
    public string itemID;        // ID duy nhất, ví dụ "Piece1"
    public string itemName;      // Tên hiển thị: "Mảnh tranh 1"
    public GameObject model;     // Model của vật phẩm (kéo prefab vào)

    [Tooltip("Nếu true thì vật phẩm sẽ biến mất khi nhặt")]
    public bool destroyOnPickup = true;

    [Header("Sound Settings")]
    public AudioClip pickupSound; // Drag your sound effect here (e.g., PickupPaper.wav)
    [Range(0f, 1f)]
    public float soundVolume = 1f; // Volume of the pickup sound
    public bool spatialSound = false; // 3D sound or 2D sound

    private AudioSource audioSource; // AudioSource to play the sound

    public bool isCollected = false;

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
        audioSource.spatialBlend = spatialSound ? 1f : 0f; // 0 = 2D, 1 = 3D

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

        isCollected = true;
        Debug.Log($"Đã nhặt: {itemName}");

        // Play sound effect if assigned
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound, soundVolume);
            Debug.Log($"Playing sound for {itemName}");
        }
        else
        {
            Debug.LogWarning($"No pickup sound assigned or AudioSource missing for {itemName}!");
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
}