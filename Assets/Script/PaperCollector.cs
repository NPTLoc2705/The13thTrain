using UnityEngine;
using System.Collections.Generic;
using TMPro; // If using TextMeshPro; otherwise use UnityEngine.UI for Text

public class PaperCollector : MonoBehaviour
{
    public List<string> collectedPieces = new List<string>(); // Tracks piece names, e.g., "TornPiece1"
    public GameObject fullPaperPrefab; // Drag FullPiece prefab here (your FullPiece.fbx prefab)
    public GameObject letterUICanvas; // Drag LetterUI Canvas here in Inspector
    public Transform modelViewerTransform; // Drag ModelViewer GameObject here
    public TextMeshProUGUI letterText; // Drag LetterText TMP Text component here (or use UnityEngine.UI.Text if not TMP)

    private bool hasLetter = false;
    private GameObject instantiatedLetter;

    public bool HasAllPieces()
    {
        return collectedPieces.Count == 5;
    }

    public void AddPiece(string pieceName)
    {
        if (!collectedPieces.Contains(pieceName))
        {
            collectedPieces.Add(pieceName);
            if (HasAllPieces())
            {
                CombinePieces();
            }
        }
    }

    private void CombinePieces()
    {
        hasLetter = true;
        Debug.Log("All pieces collected! Letter combined.");

        // Automatically show the full letter UI
        ShowFullLetter();
    }

    private void ShowFullLetter()
    {
        if (letterUICanvas != null)
        {
            // Instantiate the 3D model under ModelViewer
            instantiatedLetter = Instantiate(fullPaperPrefab, modelViewerTransform.position, Quaternion.identity, modelViewerTransform);
            instantiatedLetter.layer = LayerMask.NameToLayer("LetterModel"); // Ensure it's on the correct layer

            // Set the text (customize the message as needed)
            if (letterText != null)
            {
                letterText.text = "Dear adventurer, the safe code is 18102. Use it wisely.";
            }

            // Activate the UI
            letterUICanvas.SetActive(true);

            // Optional: Lock cursor or pause game if needed
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void HideFullLetter()
    {
        if (instantiatedLetter != null)
        {
            Destroy(instantiatedLetter);
        }

        if (letterUICanvas != null)
        {
            letterUICanvas.SetActive(false);
        }

        // Restore cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
