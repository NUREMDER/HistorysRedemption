using UnityEngine;

public class CinematicTrigger : MonoBehaviour
{
    public DialogueLine[] conversation;

    [Header("Diyalog Bitince Uyanacak Boss (Ýsteðe Baðlý)")]
    public EnemyAI bossToWakeUp;

    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            PlayerController player = other.GetComponent<PlayerController>();

            
            DialogueManager.instance.StartDialogue(conversation, player, bossToWakeUp);
        }
    }
}