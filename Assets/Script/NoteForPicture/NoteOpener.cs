using UnityEngine;

public class NoteOpener : MonoBehaviour
{
    public NoteUI noteUI;
    public float interactDistance = 2f;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null || noteUI == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance <= interactDistance && Input.GetKeyDown(KeyCode.E))
        {
            noteUI.OpenNote();
        }
    }
}
