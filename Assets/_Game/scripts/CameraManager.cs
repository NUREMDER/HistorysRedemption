using UnityEngine;
using Cinemachine; 

public class CameraManager : MonoBehaviour
{
    public CinemachineVirtualCamera openWorldCam;
    public CinemachineVirtualCamera bossCam;

    
    void Update()
    {
        // Hotkeys for testing camera switches
        if (Input.GetKeyDown(KeyCode.B))
        {
            EnterBossMode();
        }
        
        if (Input.GetKeyDown(KeyCode.N))
        {
            ExitBossMode();
        }
    }

    // Switches priority to the Boss Camera for a smooth transition.
    public void EnterBossMode()
    {
       
        bossCam.Priority = 20;
        openWorldCam.Priority = 10;
        Debug.Log("CameraManager: Boss Cam active");
    }

    // Restores priority back to the Open World Camera.
    public void ExitBossMode()
    {
      
        bossCam.Priority = 9;
        openWorldCam.Priority = 10;
        Debug.Log("CameraManager: World Cam active");
    }
}