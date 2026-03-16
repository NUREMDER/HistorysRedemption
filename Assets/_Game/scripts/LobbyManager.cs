using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("Arayüz Panelleri")]
    public GameObject marketPanel; 

    [Header("User Status")]
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI repText;

    void Start()
    {
        UpdateLobbyUI(); // Lobi açıldığında değerleri yazdır
    }

    public void UpdateLobbyUI()
    {
        if (GameManager.instance != null)
        {
            if (xpText != null)
                xpText.text = "XP: " + GameManager.instance.playerXP;
            
            if (repText != null)
                repText.text = "Reputation: " + GameManager.instance.playerReputation;
        }
    }

    public void OpenMarket()
    {
        if (marketPanel != null)
        {
            marketPanel.SetActive(true);
        }
    }

    public void CloseMarket()
    {
        if (marketPanel != null) 
        {
            marketPanel.SetActive(false);
            UpdateLobbyUI(); // Marketten çıkınca harcanan XP'leri lobide güncelle
        }
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