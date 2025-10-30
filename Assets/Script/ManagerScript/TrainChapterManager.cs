using UnityEngine;

public class TrainChapterManager : MonoBehaviour
{
    [Header("Train Doors")]
    public TrainDoorController carriage1;
    public TrainDoorController carriage2;
    public TrainDoorController carriage3;

    [Header("Chapter Progress")]
    [Tooltip("Chương cao nhất mà người chơi đã mở khóa.")]
    [Range(1, 3)] public int unlockedChapter = 1;

    void Start()
    {
        UpdateDoorLocks();
    }

    public void UnlockNextChapter()
    {
        unlockedChapter = Mathf.Clamp(unlockedChapter + 1, 1, 3);
        UpdateDoorLocks();
    }

    public void UpdateDoorLocks()
    {
        if (carriage1 != null) carriage1.LockDoor(unlockedChapter < 1);
        if (carriage2 != null) carriage2.LockDoor(unlockedChapter < 2);
        if (carriage3 != null) carriage3.LockDoor(unlockedChapter < 3);

        Debug.Log($"🚂 Doors updated — Current unlocked chapter: {unlockedChapter}");
    }
}
