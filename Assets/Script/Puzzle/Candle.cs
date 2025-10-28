using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Candle : MonoBehaviour
{
    [Header("Candle Identity")]
    [Tooltip("Index used by the manager (1..5). Unique per candle.")]
    public int candleIndex = 1;

    [Header("Visuals")]
    [Tooltip("The flame GameObject (ParticleSystem or prefab). Should be inactive at start).")]
    public GameObject flameEffect;
    [Tooltip("Optional point light to enable with flame.")]
    public Light flameLight;
    [Tooltip("Optional smoke effect GameObject (play on extinguish)")]
    public GameObject smokeEffect;

    [Header("Audio")]
    public AudioSource audioSource; // optional; can be on this GameObject
    public AudioClip igniteClip;
    public AudioClip extinguishClip;

    [Header("Interaction")]
    public string inspectPrompt = "[E] Thắp nến";

    [HideInInspector] public bool isLit = false;

    private CandlePuzzleManager manager;

    void Awake()
    {
        manager = FindObjectOfType<CandlePuzzleManager>();
        if (manager == null)
            Debug.LogWarning("Candle: CandlePuzzleManager not found in scene.");

        // Ensure flame and light are off at start
        if (flameEffect != null) flameEffect.SetActive(false);
        if (flameLight != null) flameLight.enabled = false;
        if (smokeEffect != null) smokeEffect.SetActive(false);

        // Setup audio source if missing
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && (igniteClip != null || extinguishClip != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        // Ensure this collider is trigger (you already set it in Inspector)
        Collider col = GetComponent<Collider>();
      
    }

    public void LightCandle()
    {
        if (isLit) return;
        isLit = true;

        // enable flame visuals
        if (flameEffect != null) flameEffect.SetActive(true);
        if (flameLight != null) flameLight.enabled = true;
        if (smokeEffect != null) smokeEffect.SetActive(false); // ensure smoke not playing

        // play ignite sound (optionally loop crackling)
        if (audioSource != null && igniteClip != null)
        {
            audioSource.PlayOneShot(igniteClip);
        }

        // notify manager
        if (manager != null)
            manager.RegisterLitCandle(candleIndex);

        // Hide prompt after lighting
        if (TextManager.Instance != null)
            TextManager.Instance.HidePrompt();
    }

    // used by manager when resetting
    public void Extinguish(bool playSound = true)
    {
        isLit = false;
        if (flameEffect != null) flameEffect.SetActive(false);
        if (flameLight != null) flameLight.enabled = false;
        if (smokeEffect != null)
        {
            smokeEffect.SetActive(true);
            var ps = smokeEffect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }

        if (playSound && audioSource != null && extinguishClip != null)
            audioSource.PlayOneShot(extinguishClip);
    }
}