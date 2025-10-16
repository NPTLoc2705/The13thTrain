using UnityEngine;
public class LetterUIController : MonoBehaviour
{
    public PaperCollector paperCollector; // Drag the GameObject (with PaperCollector) here

    public void CloseUI()
    {
        if (paperCollector != null)
        {
            paperCollector.HideFullLetter();
        }
    }
}