using UnityEngine;

public class CinematicTrigger : MonoBehaviour
{
    public DialogueLine[] conversation;

    [Header("Diyalog Bitince Uyanacak Boss (Istege Bagli)")]
    public EnemyAI bossToWakeUp;
    public TeslaAI teslaBossToWakeUp;

    private bool hasTriggered = false;

    // PlayerController (Boss/Combat 2D Karakter) Icin
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

    // Shadow1 (ParkourController 3D Karakter) Icin
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Cinematic Trigger icine bir sey girdi: " + other.gameObject.name + " | Tag: " + other.tag);
        
        if (other.CompareTag("Player") && !hasTriggered)
        {
            Debug.Log("- Giren sey Player tagina sahip!");
            
            ParkourController parkour = other.GetComponent<ParkourController>();
            if (parkour != null)
            {
                Debug.Log("- O objede ParkourController var, islem devam ediyor.");
                hasTriggered = true;
                
                if (DialogueManager.instance != null)
                {
                    Debug.Log("- DialogueManager bulundu, diyalog aciliyor...");
                    DialogueManager.instance.StartDialogue(conversation, parkour);
                }
                else
                {
                    Debug.LogError("!! SAHNEDE DIALOGUE MANAGER YOK VEYA AKTIF DEGIL !!");
                }
            }
            else
            {
                Debug.LogError("!! Player tagina sahip ama uzerinde ParkourController scripti yok !!");
            }
        }
    }
}