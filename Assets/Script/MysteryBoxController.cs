using UnityEngine;

public class MysteryBoxController : MonoBehaviour
{
    public GameObject toyTrainPrefab; // Drag toy train prefab here
    private bool isOpen = false;

    public void OpenBox()
    {
        if (!isOpen && PickupManager.Instance != null && PickupManager.Instance.IsCollected("SafeKey"))
        {
            isOpen = true;
            if (toyTrainPrefab != null)
            {
                GameObject toyTrain = Instantiate(toyTrainPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
                Debug.Log("Toy train popped up!");
                gameObject.SetActive(false); // Hide the mystery box
                // Add animation trigger here if toyTrain has an Animator
                // e.g., toyTrain.GetComponent<Animator>().Play("RunAnimation");
            }
        }
        else
        {
            Debug.Log("Need the key to open the mystery box!");
        }
    }
}