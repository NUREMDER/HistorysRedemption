using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("Arayüz Panelleri")]
    public GameObject marketPanel; 

    [Header("XP & Level UI")]
    public Image xpBarFill;              // Filled Image — XP bar doluluk göstergesi
    public TextMeshProUGUI levelText;    // "Level 3" gibi metin
    public TextMeshProUGUI xpText;       // XP sayısal değeri (opsiyonel)

    [Header("Reputation UI")]
    public RectTransform repNeedle;      // İbre — sola/sağa kayan RectTransform
    public TextMeshProUGUI repText;      // Reputation sayısal değeri
    [Tooltip("İbrenin ortadan en sağa/sola gidebileceği maksimum piksel mesafesi")]
    public float repNeedleMaxOffset = 150f;
    [Tooltip("Barın kapladığı toplam reputation aralığı (-maxRep ile +maxRep)")]
    public int maxReputation = 100;

    void Start()
    {
        UpdateLobbyUI(); // Lobi açıldığında değerleri yazdır
    }

    public void UpdateLobbyUI()
    {
        if (GameManager.instance == null) return;

        GameManager gm = GameManager.instance;

        // ─── XP Bar ───
        if (xpBarFill != null)
        {
            float fill = (float)gm.xpForCurrentLevel / gm.XpToNextLevel;
            xpBarFill.fillAmount = Mathf.Clamp01(fill);
        }

        if (levelText != null)
            levelText.text = "Level " + gm.playerLevel;

        if (xpText != null)
            xpText.text = gm.xpForCurrentLevel + " / " + gm.XpToNextLevel + " XP";

        // ─── Reputation İbre ───
        if (repNeedle != null)
        {
            // Reputation'ı -maxRep..+maxRep aralığında normalize et (-1..+1)
            float normalized = Mathf.Clamp((float)gm.playerReputation / maxReputation, -1f, 1f);
            // İbreyi ortadan sola veya sağa kaydır
            Vector2 pos = repNeedle.anchoredPosition;
            pos.x = normalized * repNeedleMaxOffset;
            repNeedle.anchoredPosition = pos;
        }

        if (repText != null)
            repText.text = "İtibar: " + gm.playerReputation;
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