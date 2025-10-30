using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script để tạo circle marker cho DifferenceSpot
/// Gắn vào GameObject chứa Image component để tự động tạo vòng tròn đánh dấu
/// </summary>
[RequireComponent(typeof(Image))]
public class CircleMarkerHelper : MonoBehaviour
{
    [Header("Circle Settings")]
    [SerializeField] private Color circleColor = new Color(1f, 0f, 0f, 0.7f); // Đỏ trong suốt
    [SerializeField] private float circleThickness = 5f;
    [SerializeField] private float circleRadius = 50f;

    void Start()
    {
        SetupCircleMarker();
    }

    void SetupCircleMarker()
    {
        Image image = GetComponent<Image>();
        if (image != null)
        {
            // Tạo circle sprite nếu chưa có
            if (image.sprite == null)
            {
                image.sprite = CreateCircleSprite();
            }

            image.color = circleColor;
            image.raycastTarget = false; // Không chặn raycast

            // Set size
            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(circleRadius * 2, circleRadius * 2);
            }
        }
    }

    /// <summary>
    /// Tạo sprite hình tròn đơn giản
    /// </summary>
    Sprite CreateCircleSprite()
    {
        // Tạo texture 128x128 với vòng tròn
        int resolution = 128;
        Texture2D texture = new Texture2D(resolution, resolution);
        Color[] pixels = new Color[resolution * resolution];

        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float outerRadius = resolution / 2f - 2f;
        float innerRadius = outerRadius - circleThickness;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);

                // Vẽ vòng tròn rỗng
                if (dist <= outerRadius && dist >= innerRadius)
                {
                    // Smooth edge
                    float alpha = 1f;
                    if (dist > outerRadius - 1f)
                        alpha = outerRadius - dist;
                    else if (dist < innerRadius + 1f)
                        alpha = dist - innerRadius;

                    pixels[y * resolution + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    pixels[y * resolution + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f), 100f);
    }

#if UNITY_EDITOR
    // Editor helper để preview circle
    void OnValidate()
    {
        if (Application.isPlaying) return;
        
        Image image = GetComponent<Image>();
        if (image != null)
        {
            image.color = circleColor;
        }
    }
#endif
}