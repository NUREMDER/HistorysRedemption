using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MapManager : MonoBehaviour
{
    public static MapManager instance;

    [Header("UI Panelleri")]
    public GameObject lockPanel;
    public GameObject unlockPanel;
    public GameObject notEnoughXPPanel;

    [Header("Panel Metinleri")]
    public TextMeshProUGUI unlockInfoText;

    private string selectedSceneName;
    private int selectedPrice;
    private string requiredPreviousScene;

    void Awake() { instance = this; }

    private void HandleChapterClick(string dispName, string sceneName, int price, string requiredScene)
    {
        selectedSceneName = sceneName;
        selectedPrice = price;
        requiredPreviousScene = requiredScene;

        CloseAllPanels();

        // Bölüm zaten açıksa direkt gir
        if (PlayerPrefs.GetInt(selectedSceneName + "_Unlocked", 0) == 1)
        {
            LoadLevel(selectedSceneName);
            return;
        }

        // Önceki bölüm kazanıldı mı?
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
                unlockInfoText.text = dispName + " kilidini açmak için " + selectedPrice + " XP ödemek ister misin?";
        }
    }

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