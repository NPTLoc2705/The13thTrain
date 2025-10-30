using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandlePuzzleManager : MonoBehaviour
{
    [Header("Candle Setup")]
    [Tooltip("List of Candle objects in the scene. Order doesn't matter, they are identified by candleIndex.")]
    public List<Candle> candles = new List<Candle>();

    [Tooltip("Correct sequence of candleIndex values (e.g., 3,1,4,2,5)")]
    public List<int> correctOrder = new List<int>() { 1, 2, 3, 4, 5 };

    [Header("Puzzle Settings")]
    [Tooltip("Time (s) before wrong sequence resets and extinguishes all candles")]
    public float resetDelay = 0.6f;

    [Header("Narrative Messages (shown each correct step)")]
    [Tooltip("One message per step (position in the sequence). These display when player lights the next correct candle.")]
    [TextArea(2,6)]
    public string[] motherMessages = new string[5]
    {
        "Con à... Ngày con chào đời, mẹ đã hứa sẽ bảo vệ con.",
        "Con còn nhớ không... mẹ từng tắm cho con, những tiếng cười như mặt trời nhỏ của mẹ.",
        "Những bữa cơm nhiều khi vắng, mẹ luôn lo con có ăn đủ không.",
        "Mẹ xin lỗi vì đã không thể mạnh khỏe hơn để ở bên con lâu hơn.",
        "Dù mẹ không còn ở đây... ánh sáng này sẽ luôn dẫn đường cho con. Mẹ yêu con."
    };

    [Tooltip("Optional voice clips to play with each message (same length as motherMessages). Leave empty if not using voice.")]
    public AudioClip[] motherVoiceClips = new AudioClip[5];

    [Tooltip("How long to show each text message if no voice clip is present")]
    public float messageDisplayTime = 3.0f;

    [Header("Reveal")]
    [Tooltip("The torn piece PickupItem to reveal when solved. Keep it inactive at start.")]
    public PickupItem tornPiece; // drop your TornPiece2 root here (should be inactive)
    public AudioClip successClip;
    public AudioSource sfxSource;

    // internal
    private List<int> playerSequence = new List<int>();
    private bool solved = false;
    private Coroutine resetCoroutine;

    // message handling
    private Coroutine messageCoroutine = null;
    private AudioSource voiceSource; // used to play mother voice clips (created if null)

    void Start()
    {
        // Optional: auto-find candles if the list is empty
        if (candles.Count == 0)
        {
            candles.AddRange(FindObjectsOfType<Candle>());
            Debug.Log($"[CandleManager] Auto-found {candles.Count} Candle(s).");
        }

        // Ensure the torn piece starts disabled
        if (tornPiece != null)
            tornPiece.gameObject.SetActive(false);

        // Ensure sfxSource exists if clips are provided
        if (sfxSource == null && successClip != null)
        {
            sfxSource = gameObject.GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
        }

        // Create a dedicated voiceSource for mother voice clips if any are assigned
        bool hasVoice = false;
        if (motherVoiceClips != null)
        {
            for (int i = 0; i < motherVoiceClips.Length; i++)
            {
                if (motherVoiceClips[i] != null)
                {
                    hasVoice = true;
                    break;
                }
            }
        }

        if (hasVoice)
        {
            voiceSource = gameObject.GetComponent<AudioSource>();
            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
                voiceSource.playOnAwake = false;
            }
            voiceSource.spatialBlend = 0f; // 2D voice
            voiceSource.volume = 1f;
            Debug.Log("[CandleManager] voiceSource created and ready. Clips assigned: " + CountNonNullClips());
        }
        else
        {
            Debug.Log("[CandleManager] No voice clips found in motherVoiceClips array. Will use text-only messages.");
        }
    }

    int CountNonNullClips()
    {
        if (motherVoiceClips == null) return 0;
        int c = 0;
        foreach (var clip in motherVoiceClips) if (clip != null) c++;
        return c;
    }

    /// <summary>
    /// Called by Candle.LightCandle() to register a lit candle index.
    /// </summary>
    public void RegisterLitCandle(int index)
    {
        if (solved) return;

        playerSequence.Add(index);

        // cancel any pending reset coroutine (we'll decide reset only when necessary)
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        int pos = playerSequence.Count - 1;

        // Partial correctness check
        if (pos < correctOrder.Count)
        {
            if (playerSequence[pos] != correctOrder[pos])
            {
                // Wrong candle -> start reset coroutine
                // Stop any message that might be showing
                StopCurrentMessage();
                resetCoroutine = StartCoroutine(ResetAfterDelay(resetDelay));
                return;
            }
            else
            {
                // Correct step: show the associated mother message (pos = step index)
                bool isFinalStep = (pos == correctOrder.Count - 1);
                Debug.Log($"[CandleManager] Correct candle at step {pos}. isFinalStep={isFinalStep}");
                StartMotherMessageForStep(pos, isFinalStep);
            }
        }
    }

    private void StartMotherMessageForStep(int stepIndex, bool isFinalStep)
    {
        // stop existing message coroutine and voice
        StopCurrentMessage();

        string msg = null;
        AudioClip clip = null;

        if (motherMessages != null && stepIndex < motherMessages.Length)
            msg = motherMessages[stepIndex];

        if (motherVoiceClips != null && motherVoiceClips.Length > stepIndex)
            clip = motherVoiceClips[stepIndex];

        Debug.Log($"[CandleManager] Step {stepIndex}: message present? {(!string.IsNullOrEmpty(msg))}, voiceClip present? {(clip != null)}, isFinal={isFinalStep}");

        // Start coroutine to show text + voice (if available)
        messageCoroutine = StartCoroutine(ShowMotherMessageCoroutine(msg, clip, isFinalStep));
    }

    private IEnumerator ShowMotherMessageCoroutine(string message, AudioClip voiceClip, bool isFinalStep)
    {
        // Show text (prefer ShowNotice for readable time)
        if (!string.IsNullOrEmpty(message) && TextManager.Instance != null)
        {
            TextManager.Instance.ShowNotice(message, messageDisplayTime);
            Debug.Log("[CandleManager] Showing text: " + message);
        }

        // Play voice if provided
        if (voiceClip != null && voiceSource != null)
        {
            voiceSource.clip = voiceClip;
            voiceSource.Play();
            Debug.Log("[CandleManager] Playing voiceClip via voiceSource: " + voiceClip.name);
            // wait until voice finished (or messageDisplayTime if no voice)
            yield return new WaitForSeconds(voiceClip.length);
        }
        else
        {
            // no voice clip – wait displayTime
            yield return new WaitForSeconds(messageDisplayTime);
        }

        // After message ends, hide prompt/notice if still visible
        if (TextManager.Instance != null)
            TextManager.Instance.HidePrompt();

        messageCoroutine = null;

        // If this was the final step, call solve now (after message finishes)
        if (isFinalStep)
        {
            Debug.Log("[CandleManager] Final message finished – calling OnPuzzleSolved()");
            OnPuzzleSolved();
        }
    }

    private void StopCurrentMessage()
    {
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
            messageCoroutine = null;
            Debug.Log("[CandleManager] Stopped message coroutine.");
        }

        if (voiceSource != null && voiceSource.isPlaying)
        {
            voiceSource.Stop();
            Debug.Log("[CandleManager] Stopped voiceSource playback.");
        }

        if (TextManager.Instance != null)
            TextManager.Instance.HidePrompt();
    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetAllCandles();
        playerSequence.Clear();
        resetCoroutine = null;

        // Stop any currently playing message/voice
        StopCurrentMessage();

        if (CharacterMonologue.Instance != null)
        {
            CharacterMonologue.Instance.ShowCandleWrongOrderMonologue();
        }
        else if (TextManager.Instance != null)
        {
            // Fallback if monologue system not available
            TextManager.Instance.ShowNotice("Thứ tự không đúng. Các ngọn nến tắt hết.", 2f);
        }
    }

    private void ResetAllCandles()
    {
        foreach (var c in candles)
        {
            if (c != null) c.Extinguish(true);
        }
    }

    private void OnPuzzleSolved()
    {
        if (solved) return;
        solved = true;

        // Stop any message/voice
        StopCurrentMessage();

        // Play success fx
        if (sfxSource != null && successClip != null)
        {
            sfxSource.PlayOneShot(successClip);
        }

        // AUTO-COLLECT the torn piece instead of just revealing it
        if (tornPiece != null && PickupManager.Instance != null)
        {
            // Mark as collected immediately
            tornPiece.isCollected = true;
            
            // Add to PickupManager's collected list
            if (!PickupManager.Instance.collectedItemIDs.Contains(tornPiece.itemID))
            {
                PickupManager.Instance.collectedItemIDs.Add(tornPiece.itemID);
            }

            // Play pickup sound if available
            if (tornPiece.pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(tornPiece.pickupSound, Camera.main.transform.position, tornPiece.soundVolume);
            }

            // Show collection message
            if (TextManager.Instance != null)
            {
                TextManager.Instance.ShowNotice("Bạn đã nhận được 1 mảnh giấy!", 3f);
            }

            if (CharacterMonologue.Instance != null)
            {
                // Wait a bit for collection message, then show monologue
                StartCoroutine(ShowCompletionMonologueAfterDelay(2.5f));
            }

            // Check if all pieces collected (trigger letter UI)
            if (PickupManager.Instance.collectedItemIDs.Count == 5)
            {
                if (LetterUIController.Instance != null)
                {
                    LetterUIController.Instance.ShowLetterUI();
                }
            }

        }

   }

    // Optional helper to force-solve (editor/test)
    [ContextMenu("Force Solve")]
    public void ForceSolve()
    {
        playerSequence = new List<int>(correctOrder);
        OnPuzzleSolved();
    }
    private IEnumerator ShowCompletionMonologueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (CharacterMonologue.Instance != null)
        {
            // Show monologue, then check for letter UI
            CharacterMonologue.Instance.ShowCandleCompletedMonologue(() =>
            {
                // After monologue finishes, check if all pieces collected
                if (PickupManager.Instance != null && PickupManager.Instance.collectedItemIDs.Count == 5)
                {
                    if (LetterUIController.Instance != null)
                    {
                        LetterUIController.Instance.ShowLetterUI();
                    }
                }
            });
        }
    }
}