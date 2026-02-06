using UnityEngine;
using Cinemachine; 

public class CameraManager : MonoBehaviour
{
    public CinemachineVirtualCamera openWorldCam;
    public CinemachineVirtualCamera bossCam;

    
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            EnterBossMode();
        }
        
        if (Input.GetKeyDown(KeyCode.N))
        {
            ExitBossMode();
        }
    }

    public void EnterBossMode()
    {
       
        bossCam.Priority = 20;
        openWorldCam.Priority = 10;
        Debug.Log("Savaþ Modu Kamerasý Aktif!");
    }

    public void ExitBossMode()
    {
      
        bossCam.Priority = 9;
        openWorldCam.Priority = 10;
        Debug.Log("Açýk Dünya Kamerasý Aktif!");
    }
}