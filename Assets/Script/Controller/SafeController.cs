using UnityEngine;
using TMPro;
using System.Collections;

public class SafeController : MonoBehaviour
{
    public GameObject passwordUI; // Drag PasswordUI Canvas here
    public TMP_InputField passwordInput; // Drag TMP_InputField here
    public GameObject key; // Drag Key GameObject here (inactive near safe position)
    public GameObject mysteryBox; // Drag MysteryBox GameObject here

    [Header("Shake Settings")]
    public RectTransform shakeTarget; // Drag the Panel (child of Canvas) here
    public float shakeDuration = 0.5f;
    public float shakeIntensity = 20f;

    private bool isOpen = false;
    private bool isShaking = false;
    private Vector2 originalAnchoredPosition;
    private bool isPlayerNearby = false;

    void Start()
    {
        // If shakeTarget is not assigned, try to find the Panel automatically
        if (shakeTarget == null && passwordUI != null)
        {
            // Try to find a child named "Panel"
            Transform panelTransform = passwordUI.transform.Find("Panel");
            if (panelTransform != null)
            {
                shakeTarget = panelTransform.GetComponent<RectTransform>();
            }
        }

        // Store the original anchored position
        if (shakeTarget != null)
        {
            originalAnchoredPosition = shakeTarget.anchoredPosition;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            isPlayerNearby = true;
            ShowPasswordUI();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            isPlayerNearby = false;
            HidePasswordUI();
        }
    }

    void Update()
    {
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
            TextManager.Instance.ShowNotice("Hộp bí ẩn đã xuất hiện, hãy tìm nó!", 3f);

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
            // Generate random offset for shake effect
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
}