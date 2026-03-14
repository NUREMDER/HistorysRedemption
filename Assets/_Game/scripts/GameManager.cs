using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public enum LastMatchResult { None, Won, Fled, Lost }

    [Header("Ekonomi ve tibar")]
    public int playerGold = 0;
    public int playerXP = 0;
    public int playerReputation = 0;

    [Header("Kalc Gelitirmeler")]
    public int bonusMaxHealth = 0;
    public int bonusDamage = 0;

    [Header("UI Panelleri")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public GameObject fleePanel;
    public GameObject fleeButton;

    [Header("Maç Hafızası")]
    public LastMatchResult lastMatchStatus = LastMatchResult.None;
    public int lastMatchHealthDifference = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress(); // Oyun baþladýðýnda verileri yükle
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EnemyDefeated(int goldReward, int xpReward, int repReward)
    {
        if (fleeButton != null) fleeButton.SetActive(false);

        lastMatchStatus = LastMatchResult.Won;

        PlayerController player = GameObject.FindObjectOfType<PlayerController>();
        if (player != null)
        {
        
        lastMatchHealthDifference = player.maxHealth; 
        }
        playerGold += goldReward;
        playerXP += xpReward;
        playerReputation += repReward;

        SaveProgress(); // Kaydet
        StartCoroutine(ShowVictoryScreen());
    }

    public void PlayerDefeated()
    {
        if (fleeButton != null) fleeButton.SetActive(false);

        playerReputation = Mathf.Max(0, playerReputation - 5);

        SaveProgress(); // Kaydet
        StartCoroutine(ShowDefeatScreen());
    }

    public void ShowFleeOption()
    {
        if (fleeButton != null && !fleeButton.activeSelf)
        {
            fleeButton.SetActive(true);
        }
    }

    public void FleeBattle()
    {
        if (fleeButton != null) fleeButton.SetActive(false);

        playerReputation = Mathf.Max(0, playerReputation - 10);

        SaveProgress(); // Kaydet
        if (fleePanel != null) fleePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // --- Kayýt ve Yükleme Fonksiyonlarý ---
    public void SaveProgress()
    {
        PlayerPrefs.SetInt("PlayerXP", playerXP);
        PlayerPrefs.SetInt("PlayerRep", playerReputation);
        PlayerPrefs.SetInt("PlayerGold", playerGold);
        PlayerPrefs.SetInt("BonusHealth", bonusMaxHealth);
        PlayerPrefs.SetInt("BonusDamage", bonusDamage);
        //son maç durumu oyun kapatıldığında da hatırlansın diye 
        PlayerPrefs.SetInt("LastMatchStatus", (int)lastMatchStatus);
        PlayerPrefs.SetInt("LastHealthDiff", lastMatchHealthDifference);
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        playerXP = PlayerPrefs.GetInt("PlayerXP", 0);
        playerReputation = PlayerPrefs.GetInt("PlayerRep", 0);
        playerGold = PlayerPrefs.GetInt("PlayerGold", 0);
        bonusMaxHealth = PlayerPrefs.GetInt("BonusHealth", 0);
        bonusDamage = PlayerPrefs.GetInt("BonusDamage", 0);
        lastMatchStatus = (LastMatchResult)PlayerPrefs.GetInt("LastMatchStatus", 0);
        lastMatchHealthDifference = PlayerPrefs.GetInt("LastHealthDiff", 0);
    }

    IEnumerator ShowVictoryScreen()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    IEnumerator ShowDefeatScreen()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        if (defeatPanel != null) defeatPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ReturnToLobby()
    {
        Debug.Log("Lobiye dönme tuşuna basıldı!");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Araf_Lobby");
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}