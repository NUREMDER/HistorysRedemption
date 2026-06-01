using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    public CameraManager cameraManager;

    public bool isBossZone = true;

    [Header("Arena Barriers")]
    public GameObject barrierLeft;
    public GameObject barrierRight;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Trigger battle or world camera when player enters the zone
        if (other.CompareTag("Player"))
        {
            if (isBossZone)
            {
                Debug.Log("CameraTrigger: Boss Zone entered! Switching camera...");
                cameraManager.EnterBossMode();

                // Activate barriers to lock the player in the arena
                if (barrierLeft != null) barrierLeft.SetActive(true);
                if (barrierRight != null) barrierRight.SetActive(true);
            }
            else
            {
                cameraManager.ExitBossMode();
            }
        }
    }

    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isBossZone)
        {
            // Optional: Handle camera reset when player exits, if needed later
        }
    }
}
