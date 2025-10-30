using UnityEngine;

public class PlayerDoorTrigger : MonoBehaviour
{
    public float detectRange = 3f;
    public LayerMask doorLayer;

    private TrainDoorController lastDoor;

    void Update()
    {
        DetectNearbyDoor();
    }

    void DetectNearbyDoor()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRange, doorLayer);
        if (hits.Length > 0)
        {
            TrainDoorController nearest = hits[0].GetComponent<TrainDoorController>();
            if (nearest != null && nearest != lastDoor)
            {
                nearest.TryOpenDoor();
                lastDoor = nearest; // ✅ nhớ cửa vừa mở để không gọi lại
            }
        }
    }
}
