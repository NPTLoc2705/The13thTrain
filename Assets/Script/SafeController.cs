using UnityEngine;
using TMPro;
using System.Collections; // For coroutine

public class SafeController : MonoBehaviour
{
    public GameObject passwordUI; // Drag PasswordUI Canvas here
    public TMP_InputField passwordInput; // Drag TMP_InputField here
    public TextMeshProUGUI noticeText; // Drag the small notice TextMeshProUGUI here (set font size small in Inspector)
    public GameObject key; // Drag Key GameObject here (inactive near safe position)
    public GameObject mysteryBox; // Drag MysteryBox GameObject here
    private bool isOpen = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            ShowPasswordUI();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
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
    }

    public void ShowPasswordUI()
    {
        if (passwordUI != null && !isOpen)
        {
            passwordUI.SetActive(true);
            if (passwordInput != null)
            {
                passwordInput.text = ""; // Clear previous input
                passwordInput.ActivateInputField(); // Focus the input field
            }
        }
    }

    void HidePasswordUI()
    {
        if (passwordUI != null && !isOpen)
        {
            passwordUI.SetActive(false);
        }
    }

    public void CheckPassword()
    {
        if (passwordInput != null && passwordInput.text == "18082" && !isOpen)
        {
            isOpen = true;
            passwordUI.SetActive(false);

            // Activate key and mystery box
            if (key != null)
            {
                key.SetActive(true);
                Debug.Log("Key activated at: " + key.transform.position); // Debug key position
                // Ensure key is not already collected
                PickupItem keyItem = key.GetComponent<PickupItem>();
                if (keyItem != null)
                {
                    keyItem.isCollected = false; // Reset if previously collected
                    Debug.Log("Key itemID: " + keyItem.itemID + ", isCollected: " + keyItem.isCollected);
                }
            }

            if (mysteryBox != null)
            {
                mysteryBox.SetActive(true);
                Debug.Log("Mystery box activated at: " + mysteryBox.transform.position);
            }

            // Show notice text
            if (noticeText != null)
            {
                StartCoroutine(ShowNotice("Mystery box appeared!", 3f));
                Debug.Log("Started ShowNoticeAndDestroy coroutine with noticeText: " + noticeText.name);
    
            }
            else
            {
                Debug.LogError("noticeText is null! Check Inspector assignment.");
            }

            Debug.Log("Safe opened! Key and Mystery Box revealed.");

            // Destroy the safe after success
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Incorrect password!");
        }
    }

    private IEnumerator ShowNotice(string message, float duration)
    {
        if (noticeText != null)
        {
            noticeText.text = message;
            noticeText.gameObject.SetActive(true);
            Debug.Log("Showing notice: " + message);
            yield return new WaitForSeconds(duration);
            noticeText.gameObject.SetActive(false);
            noticeText.text = ""; // Clear for next use
            Debug.Log("Notice hidden");
        }
        else
        {
            Debug.LogError("noticeText is null in coroutine!");
        }
    }
}