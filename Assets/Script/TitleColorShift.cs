using UnityEngine;
using TMPro;

public class TitleColorShift : MonoBehaviour
{
    public TextMeshPro titleText;
    public float colorSpeed = 1.5f;
    public Gradient colorGradient;

    void Update()
    {
        if (titleText == null || colorGradient == null) return;

        float t = (Mathf.Sin(Time.time * colorSpeed) + 1f) * 0.5f;
        titleText.color = colorGradient.Evaluate(t);
    }
}
