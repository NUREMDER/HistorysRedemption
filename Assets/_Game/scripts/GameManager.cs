using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public enum LastMatchResult { None, Won, Fled, Lost }
    public string lastWonSceneName = "";
    
    [Header("Aktif Chapter Takibi")]
    [Tooltip("Şu an hangi chapter oynanıyor (1-5). Lobiden butona basılınca ayarlanır.")]
    public int playingChapterNumber = 0;

    [Header("Ekonomi ve İtibar")]
    public int playerGold = 0;
    public int playerXP = 0;
    public int playerReputation = 0;
    public int playerKnives = 0;
    public int unlockedKnifeLevel = 0; // 0=Kilitli, 1=Lv1, 2=Lv2, 3=Max

    [Header("Level Sistemi")]
    public int playerLevel = 1;
    public int xpForCurrentLevel = 0;  // Mevcut level içindeki XP

    /// <summary>Mevcut level'i geçmek için gereken XP miktarı (Level 1→100, Level 2→200, ...)</summary>
    public int XpToNextLevel => 100 * playerLevel;

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

    [Header("Bölüm Kilitleri")]
    public int unlockedChapter = 1; // Başlangıçta sadece Chapter 1 açık

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else if (instance != this)
        {
            // Sahne kopyası: UI panel referanslarını asıl instance'a aktar
            instance.victoryPanel = this.victoryPanel;
            instance.defeatPanel = this.defeatPanel;
            instance.fleePanel = this.fleePanel;
            instance.fleeButton = this.fleeButton;

            // GameObject'i YOK ETME — butonlar bu objeye bağlı.
            // Sadece gereksiz mantığı engelle.
            Debug.Log("GameManager proxy: UI panelleri asıl instance'a aktarıldı ✓");
        }
    }

    /// <summary>Bu instance proxy mi (sahne kopyası)?</summary>
    private bool IsProxy => instance != null && instance != this;

    public void EnemyDefeated(int goldReward, int xpReward, int repReward)
{
    if (IsProxy) { instance.EnemyDefeated(goldReward, xpReward, repReward); return; }

    if (fleeButton != null) fleeButton.SetActive(false);

    lastMatchStatus = LastMatchResult.Won;
    lastWonSceneName = SceneManager.GetActiveScene().name;

    // --- BÖLÜM KİLİDİ AÇMA (NUMARA BAZLI - BASİT VE SAĞLAM) ---
    Debug.Log("=== DÜŞMAN YENİLDİ ===");
    Debug.Log("playingChapterNumber = " + playingChapterNumber);
    Debug.Log("unlockedChapter (önceki) = " + unlockedChapter);

    if (playingChapterNumber > 0 && playingChapterNumber <= 5)
    {
        int nextChapter = playingChapterNumber + 1;
        if (nextChapter <= 5 && unlockedChapter < nextChapter)
        {
            unlockedChapter = nextChapter;
            Debug.Log(">>> Chapter " + nextChapter + " Kilidi Açıldı!");
        }
    }
    else
    {
        Debug.LogWarning(">>> playingChapterNumber ayarlanmamış (" + playingChapterNumber + "). Kilit açılamadı.");
    }

    Debug.Log("unlockedChapter (sonraki) = " + unlockedChapter);
    // ---------------------------------

    PlayerController player = GameObject.FindObjectOfType<PlayerController>();
    if (player != null)
    {
        lastMatchHealthDifference = player.maxHealth; 
    }

    playerGold += goldReward;
    AddXP(xpReward);
    playerReputation += repReward;

    SaveProgress();
    StartCoroutine(ShowVictoryScreen());
}

    public void PlayerDefeated()
    {
        if (IsProxy) { instance.PlayerDefeated(); return; }

        if (fleeButton != null) fleeButton.SetActive(false);

        lastMatchStatus = LastMatchResult.Lost;

        // Flee yapmadan CİDDEN YENİLİNCE tüm ilerleme sıfırlanır:
        ResetAllStats();

        StartCoroutine(ShowDefeatScreen());
    }

    public void ShowFleeOption()
    {
        if (IsProxy) { instance.ShowFleeOption(); return; }

        if (fleeButton != null && !fleeButton.activeSelf)
        {
            fleeButton.SetActive(true);
        }
    }

    public void FleeBattle()
    {
        if (IsProxy) { instance.FleeBattle(); return; }

        if (fleeButton != null) fleeButton.SetActive(false);

        playerReputation -= 10; // Negatife düşebilir

        SaveProgress(); // Kaydet
        if (fleePanel != null)
        {
            fleePanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogWarning("FleePanel bulunamadı! Lobiye dönülüyor...");
            ReturnToLobby();
        }
    }

    // --- Kayıt ve Yükleme Fonksiyonları ---
    public void SaveProgress()
    {
        if (IsProxy) { instance.SaveProgress(); return; }

        PlayerPrefs.SetInt("PlayerXP", playerXP);
        PlayerPrefs.SetInt("PlayerRep", playerReputation);
        PlayerPrefs.SetInt("PlayerGold", playerGold);
        PlayerPrefs.SetInt("PlayerKnives", playerKnives);
        PlayerPrefs.SetInt("UnlockedKnifeLevel", unlockedKnifeLevel);
        PlayerPrefs.SetInt("BonusHealth", bonusMaxHealth);
        PlayerPrefs.SetInt("BonusDamage", bonusDamage);
        PlayerPrefs.SetInt("PlayerLevel", playerLevel);
        PlayerPrefs.SetInt("XPForCurrentLevel", xpForCurrentLevel);
        //son maç durumu oyun kapatıldığında da hatırlansın diye 
        PlayerPrefs.SetInt("LastMatchStatus", (int)lastMatchStatus);
        PlayerPrefs.SetInt("LastHealthDiff", lastMatchHealthDifference);
        PlayerPrefs.SetInt("UnlockedChapter", unlockedChapter);
        PlayerPrefs.DeleteKey("CurrentChapter"); // Eski versiyon temizliği
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        if (IsProxy) { instance.LoadProgress(); return; }

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
        playingChapterNumber = 0; // Runtime değişken, PlayerPrefs'ten okunmaz
    }

    public void ResetAllStats()
    {
        if (IsProxy) { instance.ResetAllStats(); return; }

        playerXP = 0;
        playerReputation = 0;
        playerGold = 0;
        playerKnives = 0;
        unlockedKnifeLevel = 0;
        bonusMaxHealth = 0;
        bonusDamage = 0;
        playerLevel = 1;
        xpForCurrentLevel = 0;
        unlockedChapter = 1; // Chapter 1 her zaman açık kalır, diğerleri kilitlenir
        
        // Son maç durumu ve sağlık farkı gibi meta verileri sıfırlamayabilirsiniz ama isterseniz:
        // lastMatchStatus = LastMatchResult.None;
        // lastMatchHealthDifference = 0;

        SaveProgress();
        Debug.Log("Tüm oyun ilerlemesi (XP, Reputation, Altın, Bıçak, Geliştirmeler, Bölüm Kilitleri) SIFIRLANDI!");
    }

    IEnumerator ShowVictoryScreen()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogWarning("VictoryPanel bulunamadı! 3 sn sonra lobiye dönülüyor...");
            yield return new WaitForSecondsRealtime(3f);
            ReturnToLobby();
        }
    }

    IEnumerator ShowDefeatScreen()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogWarning("DefeatPanel bulunamadı! 3 sn sonra lobiye dönülüyor...");
            yield return new WaitForSecondsRealtime(3f);
            ReturnToLobby();
        }
    }

    public void ReturnToLobby()
    {   
        Debug.Log("Butona tıklandı!");
        if (IsProxy) { instance.ReturnToLobby(); return; }

        Debug.Log("Lobiye dönme tuşuna basıldı!");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Araf_Lobby");
    }

    public void ReturnToMainMenu()
    {
        if (IsProxy) { instance.ReturnToMainMenu(); return; }

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // ─── XP & Level-Up Yardımcı Metot ───
    public void AddXP(int amount)
    {
        if (IsProxy) { instance.AddXP(amount); return; }

        playerXP += amount;
        xpForCurrentLevel += amount;

        // Taşan XP'yi sonraki level'lere aktar
        while (xpForCurrentLevel >= XpToNextLevel)
        {
            xpForCurrentLevel -= XpToNextLevel;
            playerLevel++;
        }
    }

    public bool BuyKnives(int amount, int cost)
    {
        if (IsProxy) { return instance.BuyKnives(amount, cost); }

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

        // Daha önceden açılmadıysa 1. seviyeyi aç
        if (unlockedKnifeLevel == 0)
        {
            unlockedKnifeLevel = 1;
            SaveProgress();
            Debug.Log("İlk bıçağın kilidi açıldı!");
        }
    }
}