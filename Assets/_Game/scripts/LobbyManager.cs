using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("Arayüz Panelleri")]
    public GameObject marketPanel; 

    [Header("Bölüm Seçim Butonları")]
    public Button[] chapterButtons; // Ch1, Ch2, Ch3 butonlarını sırayla sürükle

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
        CheckChapterLocks(); // Bölüm kilitlerini kontrol et
    }

    public void CheckChapterLocks()
{
    if (GameManager.instance == null) { Debug.LogError("LOBBY: GameManager bulunamadı!"); return; }
    if (chapterButtons == null || chapterButtons.Length == 0) { Debug.LogError("LOBBY: Butonlar diziye eklenmemiş!"); return; }

    int unlocked = GameManager.instance.unlockedChapter;
    Debug.Log("LOBBY KONTROL: Şu an açılmış olan bölüm sayısı: " + unlocked);

    for (int i = 0; i < chapterButtons.Length; i++)
    {
        if (chapterButtons[i] == null) { Debug.LogError("LOBBY: " + i + ". indeksteki buton boş!"); continue; }

        // Mantığı basit tutalım
        bool sartsaglandi = (i + 1 <= unlocked);
        chapterButtons[i].interactable = sartsaglandi;

        // Görsel olarak da emin olalım: Kilitliyse butonu biraz şeffaf yap
        Color c = chapterButtons[i].image.color;
        c.a = sartsaglandi ? 1f : 0.3f; 
        chapterButtons[i].image.color = c;

        Debug.Log(chapterButtons[i].name + " butonu aktif mi? " + sartsaglandi);
    }
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

    public void LoadChapter(string sceneName)
    {
        Debug.Log(sceneName + " sahnesine gidiliyor...");
        Time.timeScale = 1f; // Oyunun donuk kalmadığından emin olalım
        SceneManager.LoadScene(sceneName);
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
    public void StartChapter2()
{
    SceneManager.LoadScene(6);
}
}