using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Button References")]
    public TextMesh playButton;
    public TextMesh settingsButton;
    public TextMesh quitButton;

    private Color normalColor = new Color(0.8f, 0.8f, 0.8f);
    private Color hoverColor = new Color(1f, 0.889f, 0.318f);


    private TextMesh currentHover;

    void Update()
    {
        HandleMouseHover();
        HandleMouseClick();
    }

    void HandleMouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            TextMesh hovered = hit.transform.GetComponent<TextMesh>();
            if (hovered != null)
            {
                if (currentHover != hovered)
                {
                    ResetColor();
                    currentHover = hovered;
                    hovered.color = hoverColor;
                }
                return;
            }
        }
        ResetColor();
    }

    void ResetColor()
    {
        if (currentHover != null)
        {
            currentHover.color = normalColor;
            currentHover = null;
        }
    }

    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0) && currentHover != null)
        {
            switch (currentHover.name)
            {
                case "PlayButton":
                    SceneManager.LoadScene("GameScene"); // đổi tên scene thật của bạn
                    break;
                case "SettingsButton":
                    Debug.Log("Open Settings...");
                    break;
                case "QuitButton":
                    Application.Quit();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                    break;
            }
        }
    }
}
