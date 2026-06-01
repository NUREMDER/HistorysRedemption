using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// NOTE: Written for the old map system, currently unused but kept for future reference
public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    [Header("UI Panels")]
    public GameObject lockPanel;
    public GameObject unlockPanel;
    public GameObject notEnoughXPPanel;

    [Header("Panel Texts")]
    public TextMeshProUGUI unlockInfoText;

    private string selectedSceneName;
    private int selectedPrice;
    private string requiredPreviousScene;

    void Awake() 
    { 
        instance = this; 
    }

    // Handles the selection logic when a chapter node is clicked
    private void HandleChapterClick(string dispName, string sceneName, int price, string requiredScene)
    {
        selectedSceneName = sceneName;
        selectedPrice = price;
        requiredPreviousScene = requiredScene;

        CloseAllPanels();

        // If the chapter is already unlocked, load it directly
        if (PlayerPrefs.GetInt(selectedSceneName + "_Unlocked", 0) == 1)
        {
            LoadLevel(selectedSceneName);
            return;
        }

        // Check if the required previous chapter was completed successfully
        bool isPreviousWon = string.IsNullOrEmpty(requiredPreviousScene) || 
                             (GameManager.instance != null && GameManager.instance.lastWonSceneName == requiredPreviousScene);

        if (!isPreviousWon)
        {
            lockPanel.SetActive(true);
        }
        else
        {
            unlockPanel.SetActive(true);
            if (unlockInfoText != null)
                unlockInfoText.text = "Do you want to pay " + selectedPrice + " XP to unlock " + dispName + "?";
        }
    }

    // Spends XP to unlock the chosen level if the player can afford it
    public void ConfirmPurchase()
    {
        if (GameManager.instance != null && GameManager.instance.playerXP >= selectedPrice)
        {
            GameManager.instance.playerXP -= selectedPrice;
            PlayerPrefs.SetInt(selectedSceneName + "_Unlocked", 1);
            GameManager.instance.SaveProgress();
            LoadLevel(selectedSceneName);
        }
        else
        {
            unlockPanel.SetActive(false);
            notEnoughXPPanel.SetActive(true);
        }
    }

    public void OnCloseButtonClicked()
    {
        CloseAllPanels();
    }

    public void LoadLevel(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void CloseAllPanels()
    {
        if (lockPanel != null) lockPanel.SetActive(false);
        if (unlockPanel != null) unlockPanel.SetActive(false);
        if (notEnoughXPPanel != null) notEnoughXPPanel.SetActive(false);
    }
}