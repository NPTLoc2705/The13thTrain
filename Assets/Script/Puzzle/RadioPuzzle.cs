using UnityEngine;
using System.Collections;

public class RadioPuzzle : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource staticSound; // Looping static noise
    [SerializeField] private AudioSource clearStationSound; // Clear audio when tuned
    [SerializeField] private float staticMaxVolume = 1f; // Max static volume

    [Header("Tuning Settings")]
    [SerializeField] private float targetFrequency = 155.0f; // Hidden target (150-160 range)
    [SerializeField] private float startingFrequency = 88f; // Start at low end
    [SerializeField] private float minFrequency = 88f; // Extended FM range
    [SerializeField] private float maxFrequency = 300f; // Extended to 300 FM
    [SerializeField] private float tuneSpeed = 10f; // Faster tuning for larger range
    [SerializeField] private float tolerance = 0.5f; // How close to accept (within 0.5 FM)
    [SerializeField] private float driftAmount = 0.3f; // Random drift when not tuning
    [SerializeField] private float holdTime = 2.5f; // Time to hold in correct range (2-3s)

    [Header("Audio Feedback Settings")]
    [SerializeField] private float maxHearingDistance = 20f; // Max distance to hear static changes
    [SerializeField] private float optimalRange = 10f; // Range where static is quietest

    [Header("Puzzle Reward")]
    [SerializeField] private PickupItem pieceItem; // The torn piece to reveal

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 3f; // Raycast distance

    [Header("UI References")]
    [SerializeField] private Canvas frequencyCanvas; // Assign RadioUI Canvas
    [SerializeField] private Transform dialTransform; // Assign DialKnob's Transform
    [SerializeField] private TMPro.TextMeshProUGUI frequencyText; // Assign FrequencyText
    [SerializeField] private UnityEngine.UI.Button leftButton; // Assign LeftButton
    [SerializeField] private UnityEngine.UI.Button rightButton; // Assign RightButton
    [SerializeField] private UnityEngine.UI.Image leftHighlight; // Optional glow on LeftButton
    [SerializeField] private UnityEngine.UI.Image rightHighlight; // Optional glow on RightButton
    [SerializeField] private Color highlightColor = Color.yellow; // Glow color
    [SerializeField] private Color normalColor = Color.white;

    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController; // Assign PlayerController in Inspector

    private float currentFrequency;
    private bool isTuning = false; // Is player in tuning mode?
    private bool isSolved = false; // Prevent re-solving
    private bool radioActivated = false; // Track if radio was turned on
    private bool isFrequencyLocked = false; // Locked on target frequency
    private bool isWaitingForAudio = false; // Waiting for clear station audio to finish
    private bool hasPlayedAudio = false; // Track if audio has been played

    // New variables for hold mechanic
    private float timeInCorrectRange = 0f; // Time spent in correct frequency range
    private bool isInCorrectRange = false; // Currently in correct range?
    private bool hasShownHoldPrompt = false; // Track if we've shown the "hold" prompt

    void Start()
    {
        currentFrequency = startingFrequency;

        // Setup audio sources
        if (staticSound != null)
        {
            staticSound.loop = true;
            staticSound.Stop(); // Don't play until activated
        }

        if (clearStationSound != null)
        {
            clearStationSound.loop = false;
            clearStationSound.Stop();
        }

        // Hide piece initially
        if (pieceItem != null) pieceItem.gameObject.SetActive(false);

        // Hide UI initially
        if (frequencyCanvas != null) frequencyCanvas.gameObject.SetActive(false);

        // Check if already collected via manager
        if (PickupManager.Instance != null && pieceItem != null &&
            PickupManager.Instance.IsCollected(pieceItem.itemID))
        {
            isSolved = true;
            if (staticSound != null) staticSound.Stop();
            gameObject.SetActive(false);
        }

        // Optional: Setup button clicks for mouse/touch support
        if (leftButton != null) leftButton.onClick.AddListener(TuneLeft);
        if (rightButton != null) rightButton.onClick.AddListener(TuneRight);
    }

    void Update()
    {
        if (isSolved) return;

        // Activate radio (first E press)
        if (Input.GetKeyDown(KeyCode.E) && !radioActivated && !isTuning)
        {
            if (IsPlayerLookingAtRadio())
            {
                ActivateRadio();
            }
        }

        // Enter tuning mode (second E press after activation)
        if (radioActivated && !isTuning)
        {
            if (Input.GetKeyDown(KeyCode.E) && IsPlayerLookingAtRadio())
            {
                EnterTuningMode();
            }
        }

        // Handle tuning if active
        if (isTuning && !isFrequencyLocked) HandleTuning();
    }

    private void ActivateRadio()
    {
        radioActivated = true;

        if (staticSound != null)
        {
            staticSound.Play();
            staticSound.volume = staticMaxVolume;
        }

        // Delay to enter tuning mode for better UX
        Invoke(nameof(EnterTuningMode), 0.3f);
    }

    private void EnterTuningMode()
    {
        isTuning = true;

        // 🔒 LOCK PLAYER MOVEMENT - Player cannot move while tuning!
        if (playerController != null)
        {
            playerController.SetMovementLocked(true);
        }

        if (frequencyCanvas != null) frequencyCanvas.gameObject.SetActive(true);

        UpdateFrequencyUI();

        // Unlock cursor for tuning
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (TextManager.Instance != null)
        {
            TextManager.Instance.ShowPrompt("[A/D] để điều chỉnh | [ESC] Thoát\nLắng nghe kỹ tiếng tĩnh điện!");
        }
    }

    private void HandleTuning()
    {
        bool isTuningInput = false;

        // Keyboard tuning (A/D keys now ONLY tune, not move character!)
        if (Input.GetKey(KeyCode.A))
        {
            TuneFrequency(-tuneSpeed * Time.deltaTime);
            isTuningInput = true;
            // Reset hold timer when actively tuning
            timeInCorrectRange = 0f;
            hasShownHoldPrompt = false;
        }
        if (Input.GetKey(KeyCode.D))
        {
            TuneFrequency(tuneSpeed * Time.deltaTime);
            isTuningInput = true;
            // Reset hold timer when actively tuning
            timeInCorrectRange = 0f;
            hasShownHoldPrompt = false;
        }

        // Add drift when not actively tuning
        if (!isTuningInput)
        {
            currentFrequency += Random.Range(-driftAmount, driftAmount) * Time.deltaTime;
        }

        // Clamp frequency
        currentFrequency = Mathf.Clamp(currentFrequency, minFrequency, maxFrequency);

        // Update audio and UI
        UpdateStaticVolume();
        UpdateFrequencyUI();
        UpdateDialVisual();
        UpdateButtonHighlights();

        // Check if frequency is in correct range
        float distance = Mathf.Abs(targetFrequency - currentFrequency);

        // NEW LOGIC: Check if in correct range and not actively tuning
        if (distance <= tolerance && !isTuningInput)
        {
            if (!isInCorrectRange)
            {
                // Just entered correct range
                isInCorrectRange = true;
                timeInCorrectRange = 0f;
            }

            // Accumulate time in correct range
            timeInCorrectRange += Time.deltaTime;

            // Show "hold" prompt after 0.5s in range
            if (timeInCorrectRange > 0.5f && !hasShownHoldPrompt)
            {
                hasShownHoldPrompt = true;
                if (TextManager.Instance != null)
                {
                    TextManager.Instance.ShowPrompt($"✓ Giữ nguyên! ({Mathf.CeilToInt(holdTime - timeInCorrectRange)}s)");
                }
            }

            // Update countdown if showing hold prompt
            if (hasShownHoldPrompt && TextManager.Instance != null)
            {
                float remaining = holdTime - timeInCorrectRange;
                if (remaining > 0)
                {
                    TextManager.Instance.ShowPrompt($"✓ Giữ nguyên! ({Mathf.CeilToInt(remaining)}s)");
                }
            }

            // Check if held long enough
            if (timeInCorrectRange >= holdTime && !isFrequencyLocked)
            {
                LockFrequency();
            }
        }
        else
        {
            // Outside correct range or actively tuning
            if (isInCorrectRange)
            {
                // Just left correct range
                isInCorrectRange = false;
                timeInCorrectRange = 0f;
                hasShownHoldPrompt = false;

                if (TextManager.Instance != null)
                {
                    TextManager.Instance.ShowPrompt("[A/D] để điều chỉnh | [ESC] Thoát\nLắng nghe kỹ tiếng tĩnh điện!");
                }
            }
        }

        // Exit tuning (only if not waiting for audio)
        if (Input.GetKeyDown(KeyCode.Escape) && !isWaitingForAudio)
        {
            ExitTuningMode();
        }
    }

    private void TuneFrequency(float delta)
    {
        currentFrequency += delta;
    }

    // Public methods for button OnClick events
    public void TuneLeft()
    {
        if (isTuning && !isFrequencyLocked) TuneFrequency(-tuneSpeed * Time.deltaTime);
    }

    public void TuneRight()
    {
        if (isTuning && !isFrequencyLocked) TuneFrequency(tuneSpeed * Time.deltaTime);
    }

    private void UpdateStaticVolume()
    {
        if (staticSound == null) return;

        float distance = Mathf.Abs(targetFrequency - currentFrequency);

        // NEW AUDIO FEEDBACK: Static gets quieter as you get closer to target
        // Use exponential falloff for better audio feedback
        float normalizedDistance;

        if (distance <= optimalRange)
        {
            // Very close - exponential reduction
            normalizedDistance = Mathf.Pow(distance / optimalRange, 2f);
        }
        else if (distance <= maxHearingDistance)
        {
            // Medium range - linear increase
            float t = (distance - optimalRange) / (maxHearingDistance - optimalRange);
            normalizedDistance = Mathf.Lerp(1f, 0.3f, 1f - t);
        }
        else
        {
            // Far away - constant loud static
            normalizedDistance = 1f;
        }

        staticSound.volume = Mathf.Clamp01(normalizedDistance) * staticMaxVolume;

        // Pitch changes when very close (audio cue)
        if (distance <= tolerance * 3f)
        {
            staticSound.pitch = Mathf.Lerp(1.2f, 1.0f, distance / (tolerance * 3f));
        }
        else
        {
            staticSound.pitch = 1.0f;
        }
    }

    private void LockFrequency()
    {
        isFrequencyLocked = true;
        isWaitingForAudio = true;

        // Stop static
        if (staticSound != null) staticSound.Stop();

        // Play clear station audio (only once)
        if (clearStationSound != null && !hasPlayedAudio)
        {
            hasPlayedAudio = true;
            clearStationSound.volume = 0f;
            clearStationSound.Play();
            StartCoroutine(FadeInAudio(clearStationSound, 1.5f));
        }

        // Update UI to show locked frequency
        if (frequencyText != null)
        {
            frequencyText.text = $"FM: {currentFrequency:F1} MHz ✓";
            frequencyText.color = Color.green;
        }

        if (TextManager.Instance != null)
        {
            TextManager.Instance.ShowPrompt("Tần số chính xác! Hãy lắng nghe đoạn ghi âm...");
        }

        // Wait for audio to finish, THEN auto-collect the piece
        if (clearStationSound != null)
        {
            float audioLength = clearStationSound.clip.length;
            StartCoroutine(WaitForAudioAndAutoCollect(audioLength));
        }
        else
        {
            // Fallback if no audio
            StartCoroutine(WaitForAudioAndAutoCollect(3f));
        }
    }

    private IEnumerator WaitForAudioAndAutoCollect(float duration)
    {
        // Wait for audio to finish
        yield return new WaitForSeconds(duration);

        // NOW auto-collect the piece!
        AutoCollectPieceAndFinish();
    }

    private void AutoCollectPieceAndFinish()
    {
        if (isSolved) return;

        isSolved = true;
        isTuning = false;
        isWaitingForAudio = false;

        // 🔓 UNLOCK PLAYER MOVEMENT WHEN PUZZLE IS SOLVED!
        if (playerController != null)
        {
            playerController.SetMovementLocked(false);
        }

        // AUTO-COLLECT the torn piece instead of just revealing it
        if (pieceItem != null && PickupManager.Instance != null)
        {
            // Mark as collected immediately
            pieceItem.isCollected = true;

            // Add to PickupManager's collected list
            if (!PickupManager.Instance.collectedItemIDs.Contains(pieceItem.itemID))
            {
                PickupManager.Instance.collectedItemIDs.Add(pieceItem.itemID);
            }

            // Play pickup sound if available
            if (pieceItem.pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pieceItem.pickupSound, Camera.main.transform.position, pieceItem.soundVolume);
            }

            // Show collection message
            if (TextManager.Instance != null)
            {
                TextManager.Instance.ShowNotice("Bạn đã nhận được 1 mảnh giấy!", 3f);
            }

            // Check if all pieces collected (trigger letter UI)
            if (PickupManager.Instance.collectedItemIDs.Count == 5)
            {
                if (LetterUIController.Instance != null)
                {
                    LetterUIController.Instance.ShowLetterUI();
                }
            }

            Debug.Log($"[RadioPuzzle] Auto-collected: {pieceItem.itemID} ({PickupManager.Instance.collectedItemIDs.Count}/5)");
        }

        StartCoroutine(HideUIAfterDelay(2f));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ExitTuningMode()
    {
        isTuning = false;

        // 🔓 UNLOCK PLAYER MOVEMENT - Player can move again!
        if (playerController != null)
        {
            playerController.SetMovementLocked(false);
        }

        if (frequencyCanvas != null) frequencyCanvas.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (TextManager.Instance != null) TextManager.Instance.HidePrompt();
    }

    private bool IsPlayerLookingAtRadio()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            return hit.collider.gameObject == this.gameObject || hit.collider.transform.IsChildOf(this.transform);
        }
        return false;
    }

    private IEnumerator HideUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (frequencyCanvas != null) frequencyCanvas.gameObject.SetActive(false);
    }

    private IEnumerator FadeInAudio(AudioSource audio, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audio.volume = Mathf.Lerp(0f, 1f, timer / duration);
            yield return null;
        }
        audio.volume = 1f;
    }

    private void UpdateFrequencyUI()
    {
        if (frequencyText != null)
        {
            if (isFrequencyLocked)
            {
                frequencyText.text = $"FM: {currentFrequency:F1} MHz ✓";
                frequencyText.color = Color.green;
            }
            else
            {
                // Always show white text - no color hints until locked
                frequencyText.text = $"FM: {currentFrequency:F1} MHz";
                frequencyText.color = Color.white;
            }
        }
    }

    private void UpdateDialVisual()
    {
        if (dialTransform != null)
        {
            float normalizedFreq = (currentFrequency - minFrequency) / (maxFrequency - minFrequency);
            float angle = Mathf.Lerp(-90f, 90f, normalizedFreq);
            dialTransform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void UpdateButtonHighlights()
    {
        if (isFrequencyLocked)
        {
            // Disable highlights when locked
            if (leftHighlight != null) leftHighlight.color = normalColor;
            if (rightHighlight != null) rightHighlight.color = normalColor;
        }
        else
        {
            if (leftHighlight != null) leftHighlight.color = Input.GetKey(KeyCode.A) ? highlightColor : normalColor;
            if (rightHighlight != null) rightHighlight.color = Input.GetKey(KeyCode.D) ? highlightColor : normalColor;
        }
    }

    // Public method to get interaction prompt (for PlayerController)
    public string GetPromptMessage()
    {
        if (!radioActivated) return "[E] Bật Radio lên";
        else if (!isTuning) return "[E] Chỉnh âm Radio";
        return "";
    }

    // Public property for PlayerController to check if we're tuning
    public bool IsTuning => isTuning;
}