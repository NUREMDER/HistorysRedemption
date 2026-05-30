using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singleton instance of the GameManager

    // Enumeration for keeping track of the last match outcome
    public enum LastMatchResult { None, Won, Fled, Lost }
    public string lastWonSceneName = ""; //!!!!!
    
    [Header("Active Chapter Tracking")]
    public int playingChapterNumber = 0;

    [Header("Economy and Reputation")]
    public int playerGold = 0;
    public int playerXP = 0;
    public int playerReputation = 0;
    public int playerKnives = 0;
    public int unlockedKnifeLevel = 0; // 0=Locked, 1=Lv1, 2=Lv2, 3=Max

    [Header("Level System")]
    public int playerLevel = 1;
    public int xpForCurrentLevel = 0;  // XP accumulated in the current level

    // Property that calculates required XP for the next level (Level 1 = 100 XP, Level 2 = 200 XP, etc.)
    public int XpToNextLevel => 100 * playerLevel;

    [Header("Permanent Upgrades")]
    public int bonusMaxHealth = 0;
    public int bonusDamage = 0;

    [Header("UI Panels")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public GameObject fleePanel;
    public GameObject fleeButton;

    [Header("Match Memory")]
    public LastMatchResult lastMatchStatus = LastMatchResult.None;
    public int lastMatchHealthDifference = 0; //!!!!!

    [Header("Chapter Locks")]
    public int unlockedChapter = 1; // Chapter 1 is unlocked by default

    void Awake()
    {
        if (instance == null)
        {
            // If this is the first time the game runs, set this as the main instance
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else if (instance != this)
        {
            // Scene duplicate: transfer the new UI panel references to the main instance
            instance.victoryPanel = this.victoryPanel;
            instance.defeatPanel = this.defeatPanel;
            instance.fleePanel = this.fleePanel;
            instance.fleeButton = this.fleeButton;

            // DO NOT destroy the GameObject because buttons are attached to this object.
            // Just prevent duplicate logic from running.
            Debug.Log("GameManager proxy: UI panels transferred to main instance ✓");
        }
    }

    /// <summary>Bu instance proxy mi (sahne kopyası)?</summary>
    private bool IsProxy => instance != null && instance != this;

    public void EnemyDefeated(int goldReward, int xpReward, int repReward)
{
    // If this is a proxy GameManager, forward the call to the main instance and exit
    if (IsProxy) 
    { 
        instance.EnemyDefeated(goldReward, xpReward, repReward); 
        return; 
    }

    // Hide the flee button since the match is over
    if (fleeButton != null) 
    {
        fleeButton.SetActive(false);
    }

    // Update match history states
    lastMatchStatus = LastMatchResult.Won;
    lastWonSceneName = SceneManager.GetActiveScene().name;

    Debug.Log("=== ENEMY DEFEATED ===");
    Debug.Log("playingChapterNumber = " + playingChapterNumber);
    Debug.Log("unlockedChapter (before) = " + unlockedChapter);

    // Check if the current playing chapter is valid to unlock the next one
    if (playingChapterNumber > 0 && playingChapterNumber <= 5)
    {
        int nextChapter = playingChapterNumber + 1;
        // Unlock the next chapter if it is not already unlocked
        if (nextChapter <= 5 && unlockedChapter < nextChapter)
        {
            unlockedChapter = nextChapter;
            Debug.Log(">>> Chapter " + nextChapter + " Unlocked!");
        }
    }
    else
    {
        Debug.LogWarning(">>> playingChapterNumber is not set (" + playingChapterNumber + "). Cannot unlock next chapter.");
    }

    Debug.Log("unlockedChapter (after) = " + unlockedChapter);

    // Save player's health data for match memory
    PlayerController player = GameObject.FindObjectOfType<PlayerController>();
    if (player != null)
    {
        lastMatchHealthDifference = player.maxHealth; 
    }

    // Add rewarded gold, experience points, and reputation
    playerGold += goldReward;
    AddXP(xpReward);
    playerReputation += repReward;

    // Save player progress to PlayerPrefs and open victory screen
    SaveProgress();
    StartCoroutine(ShowVictoryScreen());
}
    public void PlayerDefeated()
    {
        // If proxy, forward to main instance and exit
        if (IsProxy) { instance.PlayerDefeated(); return; }

        if (fleeButton != null) fleeButton.SetActive(false);

        lastMatchStatus = LastMatchResult.Lost;

        // Reset all player stats as a penalty for losing completely
        ResetAllStats();

        StartCoroutine(ShowDefeatScreen());
    }

    public void ShowFleeOption()
    {
        if (IsProxy) { instance.ShowFleeOption(); return; }

        // Enable the flee button if it is hidden
        if (fleeButton != null && !fleeButton.activeSelf)
        {
            fleeButton.SetActive(true);
        }
    }

    public void FleeBattle()
    {
        if (IsProxy) { instance.FleeBattle(); return; }

        if (fleeButton != null) fleeButton.SetActive(false);

        // Decrease reputation points as a penalty for escaping
        playerReputation -= 10; 

        // Save progress and show the flee panel
        SaveProgress(); 
        if (fleePanel != null)
        {
            fleePanel.SetActive(true);
            Time.timeScale = 0f; // Pause the game
        }
        else
        {
            Debug.LogWarning("FleePanel not found! Returning to lobby...");
            ReturnToLobby();
        }
    }

    // --- Save and Load Functions ---
    public void SaveProgress()
    {
        if (IsProxy) { instance.SaveProgress(); return; }

        // Save all player stats and game progression to local storage
        PlayerPrefs.SetInt("PlayerXP", playerXP);
        PlayerPrefs.SetInt("PlayerRep", playerReputation);
        PlayerPrefs.SetInt("PlayerGold", playerGold);
        PlayerPrefs.SetInt("PlayerKnives", playerKnives);
        PlayerPrefs.SetInt("UnlockedKnifeLevel", unlockedKnifeLevel);
        PlayerPrefs.SetInt("BonusHealth", bonusMaxHealth);
        PlayerPrefs.SetInt("BonusDamage", bonusDamage);
        PlayerPrefs.SetInt("PlayerLevel", playerLevel);
        PlayerPrefs.SetInt("XPForCurrentLevel", xpForCurrentLevel);
        PlayerPrefs.SetInt("LastMatchStatus", (int)lastMatchStatus);
        PlayerPrefs.SetInt("LastHealthDiff", lastMatchHealthDifference);
        PlayerPrefs.SetInt("UnlockedChapter", unlockedChapter);
        
        PlayerPrefs.DeleteKey("CurrentChapter"); // Clean up old version keys
        PlayerPrefs.Save(); // Force write changes to disk
    }

    public void LoadProgress()
    {
        if (IsProxy) { instance.LoadProgress(); return; }

        // Load all saved player data from local storage on startup
        playerXP = PlayerPrefs.GetInt("PlayerXP", 0);
        playerReputation = PlayerPrefs.GetInt("PlayerRep", 0);
        playerGold = PlayerPrefs.GetInt("PlayerGold", 0);
        playerKnives = PlayerPrefs.GetInt("PlayerKnives", 0);
        unlockedKnifeLevel = PlayerPrefs.GetInt("UnlockedKnifeLevel", 0);
        bonusMaxHealth = PlayerPrefs.GetInt("BonusHealth", 0);
        bonusDamage = PlayerPrefs.GetInt("BonusDamage", 0);
        playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        xpForCurrentLevel = PlayerPrefs.GetInt("XPForCurrentLevel", 0);
        lastMatchStatus = (LastMatchResult)PlayerPrefs.GetInt("LastMatchStatus", 0);
        lastMatchHealthDifference = PlayerPrefs.GetInt("LastHealthDiff", 0);
        unlockedChapter = PlayerPrefs.GetInt("UnlockedChapter", 1);
        playingChapterNumber = 0; // Reset runtime variables since they don't need to be saved
    }

    public void ResetAllStats()
    {
        if (IsProxy) { instance.ResetAllStats(); return; }

        // Reset all progression variables to default values as a death penalty
        playerXP = 0;
        playerReputation = 0;
        playerGold = 0;
        playerKnives = 0;
        unlockedKnifeLevel = 0;
        bonusMaxHealth = 0;
        bonusDamage = 0;
        playerLevel = 1;
        xpForCurrentLevel = 0;
        unlockedChapter = 1; // Chapter 1 remains open, others are locked again
        
        SaveProgress(); // Save the cleared data immediately
        Debug.Log("All game progress has been successfully reset!");
    }
    IEnumerator ShowVictoryScreen()
    {
        // Wait a bit before showing the victory panel and pausing the game
        yield return new WaitForSecondsRealtime(1.5f);
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            // If panel is missing, wait 3 seconds and safely go back to lobby
            Debug.LogWarning("VictoryPanel not found! Returning to lobby in 3 seconds...");
            yield return new WaitForSecondsRealtime(3f);
            ReturnToLobby();
        }
    }

    IEnumerator ShowDefeatScreen()
    {
        // Wait a bit before showing the defeat panel and pausing the game
        yield return new WaitForSecondsRealtime(1.5f);
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            // If panel is missing, wait 3 seconds and safely go back to lobby
            Debug.LogWarning("DefeatPanel not found! Returning to lobby in 3 seconds...");
            yield return new WaitForSecondsRealtime(3f);
            ReturnToLobby();
        }
    }

    public void ReturnToLobby()
    {   
        Debug.Log("Button clicked!");
        if (IsProxy) { instance.ReturnToLobby(); return; }

        Debug.Log("Returning to lobby button pressed!");
        // Reset time scale to normal and load the lobby scene
        Time.timeScale = 1f;
        SceneManager.LoadScene("Araf_Lobby");
    }

    public void ReturnToMainMenu()
    {
        if (IsProxy) { instance.ReturnToMainMenu(); return; }

        // Reset time scale to normal and load the main menu scene
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void AddXP(int amount)
    {
        if (IsProxy) { instance.AddXP(amount); return; }

        playerXP += amount;
        xpForCurrentLevel += amount;

        // Carry over remaining XP to next levels if it overflows
        while (xpForCurrentLevel >= XpToNextLevel)
        {
            xpForCurrentLevel -= XpToNextLevel;
            playerLevel++;
        }
    }

    public bool BuyKnives(int amount, int cost)
    {
        if (IsProxy) { return instance.BuyKnives(amount, cost); }

        // Check if the player has enough gold to buy knives
        if (playerGold >= cost)
        {
            playerGold -= cost;
            playerKnives += amount;
            SaveProgress();
            return true;
        }
        return false;
    }

    public void UnlockFirstKnife()
    {
        if (IsProxy) { instance.UnlockFirstKnife(); return; }

        // Unlock the first knife level if it is currently locked
        if (unlockedKnifeLevel == 0)
        {
            unlockedKnifeLevel = 1;
            SaveProgress();
            Debug.Log("First knife unlocked!");
        }
    }
}