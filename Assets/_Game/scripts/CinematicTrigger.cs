using UnityEngine;

public class CinematicTrigger : MonoBehaviour
{
    public DialogueLine[] conversation;

    [Header("Boss Activation")]
    public EnemyAI bossToWakeUp;
    public TeslaAI teslaBossToWakeUp;

    private bool hasTriggered = false;

    // For Player Controller
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                hasTriggered = true;
                if (DialogueManager.instance != null)
                {
                    DialogueManager.instance.StartDialogue(conversation, player, bossToWakeUp, teslaBossToWakeUp);
                }
            }
        }
    }

    // For 3D Parkour Player Controller
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("CinematicTrigger: Object entered trigger: " + other.gameObject.name + " | Tag: " + other.tag);
        
        if (other.CompareTag("Player") && !hasTriggered)
        {
            Debug.Log("CinematicTrigger: Object has Player tag.");
            
            ParkourController parkour = other.GetComponent<ParkourController>();
            if (parkour != null)
            {
                Debug.Log("CinematicTrigger: ParkourController found. Starting sequence.");
                hasTriggered = true;
                
                if (DialogueManager.instance != null)
                {
                    Debug.Log("CinematicTrigger: DialogueManager found. Opening dialogue...");
                    DialogueManager.instance.StartDialogue(conversation, parkour);
                }
                else
                {
                    Debug.LogError("CinematicTrigger: DialogueManager instance is missing or inactive in the scene!");
                }
            }
            else
            {
                Debug.LogError("CinematicTrigger: Object has Player tag but is missing a ParkourController component!");
            }
        }
    }
}