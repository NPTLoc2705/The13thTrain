using UnityEngine;

/// <summary>
/// Script đơn giản để test phím X có hoạt động không
/// Gắn vào Canvas để test
/// </summary>
public class KeyPressTester : MonoBehaviour
{
    void Update()
    {
        // Test tất cả các phím quan trọng
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("🔑 PHÍM X ĐƯỢC NHẤN!");
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("🔑 PHÍM ESC ĐƯỢC NHẤN!");
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("🔑 PHÍM ← ĐƯỢC NHẤN!");
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("🔑 PHÍM → ĐƯỢC NHẤN!");
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("🔑 PHÍM A ĐƯỢC NHẤN!");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("🔑 PHÍM D ĐƯỢC NHẤN!");
        }

        // Test bất kỳ phím nào
        if (Input.anyKeyDown)
        {
            Debug.Log($"⌨️ Một phím nào đó được nhấn. Input string: '{Input.inputString}'");
        }
    }

    void OnGUI()
    {
        // Hiển thị trên màn hình để debug dễ hơn
        GUILayout.Label($"Nhấn phím bất kỳ để test...");
        GUILayout.Label($"Time: {Time.time:F2}");
    }
}