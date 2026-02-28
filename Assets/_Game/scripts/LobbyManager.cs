using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("Arayüz Panelleri")]
    public GameObject marketPanel; 

    public void OpenMarket()
    {
        if (marketPanel != null)
        {
            marketPanel.SetActive(true);
            
        }
    }

    
    public void CloseMarket()
    {
        if (marketPanel != null) marketPanel.SetActive(false);
    }

   
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    
    public void QuitGame()
    {
        Debug.Log("OYUNDAN ÇIKILIYOR...");
        Application.Quit();
    }
}