using UnityEngine;
using TMPro;
using System.Collections;

public class SafeController : MonoBehaviour
{
    public GameObject passwordUI;
    public TMP_InputField passwordInput;
    public GameObject key;
    public GameObject mysteryBox;

    [Header("Shake Settings")]
    public RectTransform shakeTarget;
    public float shakeDuration = 0.5f;
    public float shakeIntensity = 20f;

    [Header("Interaction")]
    public string interactPrompt = "[E] Nhập mật khẩu két sắt";

    private bool isOpen = false;
    private bool isShaking = false;
    private Vector2 originalAnchoredPosition;
    private bool isPlayerNearby = false;

    void Start()
    {
        if (shakeTarget == null && passwordUI != null)
        {
            Transform panelTransform = passwordUI.transform.Find("Panel");
            if (panelTransform != null)
            {
                shakeTarget = panelTransform.GetComponent<RectTransform>();
            }
        }

        if (shakeTarget != null)
        {
            originalAnchoredPosition = shakeTarget.anchoredPosition;
        }

        // ✅ Make sure password UI is hidden at start
        if (passwordUI != null)
        {
            passwordUI.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            isPlayerNearby = true;
            // ✅ Show prompt instead of opening UI immediately
            if (TextManager.Instance != null)
            {
                TextManager.Instance.ShowPrompt(interactPrompt);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            // ✅ Hide prompt when leaving
            if (TextManager.Instance != null)
            {
                TextManager.Instance.HidePrompt();
            }

            // ✅ Close password UI if player walks away
            if (!isOpen && passwordUI != null && passwordUI.activeSelf)
            {
                HidePasswordUI();
            }
        }
    }

    void Update()
    {
        // ✅ Allow player to press E to open password UI when nearby
        if (isPlayerNearby && !isOpen && Input.GetKeyDown(KeyCode.E))
        {
            if (passwordUI != null && !passwordUI.activeSelf)
            {
                ShowPasswordUI();
            }
        }

        // Check for Enter key press when passwordUI is active
        if (passwordUI != null && passwordUI.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            CheckPassword();
        }

        // Check for Escape key to close the password UI
        if (passwordUI != null && passwordUI.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            HidePasswordUI();
        }
    }

    public void ShowPasswordUI()
    {
        if (passwordUI != null && !isOpen)
        {
            passwordUI.SetActive(true);

            // Reset position when showing UI
            if (shakeTarget != null)
            {
                shakeTarget.anchoredPosition = originalAnchoredPosition;
            }

            if (passwordInput != null)
            {
                passwordInput.text = ""; // Clear previous input
                passwordInput.ActivateInputField(); // Focus the input field
            }

            // Show cursor for input
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // ✅ Hide the prompt when UI opens
            if (TextManager.Instance != null)
            {
                TextManager.Instance.HidePrompt();
            }
        }
    }

    void HidePasswordUI()
    {
        if (passwordUI != null && !isOpen)
        {
            passwordUI.SetActive(false);

            // Lock cursor again
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // ✅ Show prompt again if player is still nearby
            if (isPlayerNearby && TextManager.Instance != null)
            {
                TextManager.Instance.ShowPrompt(interactPrompt);
            }
        }
    }

    /// <summary>
    /// Check if the safe password UI is currently open
    /// </summary>
    public bool IsPasswordUIOpen()
    {
        return passwordUI != null && passwordUI.activeSelf && !isOpen;
    }

    public void CheckPassword()
    {
        if (passwordInput != null && passwordInput.text == "18082" && !isOpen)
        {
            isOpen = true;
            passwordUI.SetActive(false);

            // Lock cursor again
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Activate key and mystery box
            if (key != null)
            {
                key.SetActive(true);
                PickupItem keyItem = key.GetComponent<PickupItem>();
                if (keyItem != null)
                {
                    keyItem.isCollected = false;
                }
            }

            if (mysteryBox != null)
            {
                mysteryBox.SetActive(true);
            }

            // Show notice via TextManager
            if (TextManager.Instance != null)
            {
                TextManager.Instance.ShowNotice("Hộp bí ẩn đã xuất hiện, hãy tìm nó!", 3f);
            }

            // Destroy the safe immediately
            Destroy(gameObject);
        }
        else
        {
            // Trigger shake animation
            if (!isShaking && shakeTarget != null)
            {
                StartCoroutine(ShakePasswordUI());
            }
            else if (shakeTarget == null)
            {
                Debug.LogWarning("Shake Target not assigned! Please assign the Panel RectTransform in the Inspector.");
            }
        }
    }

    IEnumerator ShakePasswordUI()
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = Random.Range(-shakeIntensity, shakeIntensity);

            shakeTarget.anchoredPosition = originalAnchoredPosition + new Vector2(offsetX, offsetY);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset to original position
        shakeTarget.anchoredPosition = originalAnchoredPosition;
        isShaking = false;
    }

    /// <summary>
    /// Get prompt message for nearby interaction
    /// </summary>
    public string GetPromptMessage()
    {
        if (isPlayerNearby && !isOpen)
        {
            return interactPrompt;
        }
        return null;
    }

    /// <summary>
    /// Check if player can interact with safe
    /// </summary>
    public bool CanInteract()
    {
        return isPlayerNearby && !isOpen;
    }
}