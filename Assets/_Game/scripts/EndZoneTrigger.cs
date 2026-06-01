using UnityEngine;
using System.Collections;
using Cinemachine;

public class EndZoneTrigger : MonoBehaviour
{
    [Header("Character Settings")]
    public GameObject parkourCharacter; 
    
    public GameObject combatCharacter; 

    public GameObject enemyCharacter;

    public GameObject barrierForParkour;

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // If the entering object has a Player tag and hasn't been triggered yet
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(SwitchToCombatRoutine());
        }
    }

    private IEnumerator SwitchToCombatRoutine()
    {
        SceneChanger changer = FindObjectOfType<SceneChanger>();

        //  Opens loading panel
        if (changer != null && changer.loadingCanvas != null)
        {
            changer.loadingCanvas.SetActive(true);
        }

        // Waits 3 seconds in Loading panel
        yield return new WaitForSeconds(3f);

        // 
        if (parkourCharacter != null && combatCharacter != null)
        {
        //Teleport the combat character to the exact position where the parkour character finished
            combatCharacter.transform.position = parkourCharacter.transform.position;

            // Delete parkourCharacter and acivate combatCharacter
            parkourCharacter.SetActive(false);
            combatCharacter.SetActive(true);

            // Focuses camera to combatCharacter
            CameraManager camManager = FindObjectOfType<CameraManager>();
            if (camManager != null && camManager.openWorldCam != null)
            {
                // If there is a empty object named CameraTarget focus on it
                Transform focusTarget = combatCharacter.transform;
                Transform customTarget = combatCharacter.transform.Find("CameraTarget");
                
                if (customTarget != null)
                {
                    focusTarget = customTarget;
                }

                camManager.openWorldCam.Follow = focusTarget;
                camManager.openWorldCam.LookAt = focusTarget;
            }
        }

        // Activate Enemy
        if (enemyCharacter != null)
        {
            enemyCharacter.SetActive(true);
        }

        // Activate barrier after parkour
        if (barrierForParkour != null)
        {
            barrierForParkour.SetActive(true);
        }

        // Closes loading panel and battle part starts
        if (changer != null && changer.loadingCanvas != null)
        {
            changer.loadingCanvas.SetActive(false);
        }
    }
}