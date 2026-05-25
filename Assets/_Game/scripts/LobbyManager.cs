using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("Arayüz Panelleri")]
    public GameObject marketPanel; 

    [Header("Bölüm Seçim Butonları")]
    [Tooltip("Level1Button, Level2Button, Level3Button, Level4Button sırasıyla sürükle")]
    public Button[] chapterButtons; // 4 buton: index 0=Ch1, 1=Ch2, 2=Ch3, 3=Ch4

    [Header("Kilitli Buton Görünümü")]
    [Tooltip("Kilitli butonların şeffaflık değeri (0=tamamen görünmez, 1=tam görünür)")]
    [Range(0f, 1f)]
    public float lockedAlpha = 0.3f;

    [Header("XP & Level UI")]
    public Image xpBarFill;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;

    [Header("Reputation UI")]
    public RectTransform repNeedle;
    public TextMeshProUGUI repText;
    [Tooltip("İbrenin ortadan en sağa/sola gidebileceği maksimum piksel mesafesi")]
    public float repNeedleMaxOffset = 150f;
    [Tooltip("Barın kapladığı toplam reputation aralığı (-maxRep ile +maxRep)")]
    public int maxReputation = 100;

    // Sahne isimleri — buton index'ine göre eşleşir
    private readonly string[] chapterSceneNames = { "Chapter1", "Chapter2", "Chapter3", "Chapter4" };

    void Start()
    {
        SetupChapterButtons();
        UpdateLobbyUI();
        CheckChapterLocks();
    }



    /// <summary>
    /// Her butona runtime listener olarak doğru Chapter sahnesini yükleyen onClick ekler.
    /// Inspector'daki mevcut onClick eventlerini (ör. ses efekti) BOZMAZ, yanına eklenir.
    /// </summary>
    private void SetupChapterButtons()
    {
        if (chapterButtons == null || chapterButtons.Length == 0)
        {
            Debug.LogError("LOBBY SETUP: chapterButtons dizisi boş veya atanmamış!");
            return;
        }

        for (int i = 0; i < chapterButtons.Length; i++)
        {
            if (chapterButtons[i] == null)
            {
                Debug.LogError("LOBBY SETUP: chapterButtons[" + i + "] null! Inspector'da buton atanmamış.");
                continue;
            }

            // Closure için yerel değişkenler
            int chapterNum = i + 1; // 1, 2, 3, 4, 5
            string sceneName = (i < chapterSceneNames.Length) 
                ? chapterSceneNames[i] 
                : "Chapter" + chapterNum;

            // Inspector onClick'leri silmeden, YANINA runtime listener ekle
            chapterButtons[i].onClick.AddListener(() => LoadChapter(sceneName, chapterNum));

            Debug.Log("LOBBY SETUP: " + chapterButtons[i].name + " → " + sceneName + " (Chapter " + chapterNum + ") atandı ✓");
        }
    }

    /// <summary>
    /// Bölüm kilit durumlarını kontrol eder.
    /// Chapter 1 her zaman açıktır.
    /// </summary>
    public void CheckChapterLocks()
    {
        if (GameManager.instance == null) { Debug.LogError("LOBBY: GameManager bulunamadı!"); return; }
        if (chapterButtons == null || chapterButtons.Length == 0) { Debug.LogError("LOBBY: Butonlar diziye eklenmemiş!"); return; }

        int unlocked = GameManager.instance.unlockedChapter;
        Debug.Log("LOBBY KONTROL: unlockedChapter = " + unlocked);

        for (int i = 0; i < chapterButtons.Length; i++)
        {
            if (chapterButtons[i] == null) { Debug.LogError("LOBBY: " + i + ". indeksteki buton boş!"); continue; }

            bool isUnlocked = (i + 1 <= unlocked);
            
            // Tıklama kontrolü sadece Button.interactable ile
            chapterButtons[i].interactable = isUnlocked;

            // CanvasGroup sadece görsel alpha için — tıklamaya DOKUNMUYOR
            CanvasGroup cg = chapterButtons[i].GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = chapterButtons[i].gameObject.AddComponent<CanvasGroup>();
            }
            cg.alpha = isUnlocked ? 1f : lockedAlpha;
            cg.blocksRaycasts = true;  // Her zaman true
            cg.interactable = true;    // Her zaman true

            Debug.Log(chapterButtons[i].name + " → " + (isUnlocked ? "AÇIK ✓" : "KİLİTLİ ✗"));
        }
    }

    public void UpdateLobbyUI()
    {
        if (GameManager.instance == null) return;

        GameManager gm = GameManager.instance;

        if (xpBarFill != null)
        {
            // Sadece dolgu barını gizleyelim, parent'ı gizlersek xpText de kaybolabilir!
            xpBarFill.gameObject.SetActive(false);
            
            // Eğer barın bir arka planı varsa (kardeş objesi vb.) ve siz onu Inspector'dan silmezseniz burada görünebilir.
            // En temizi Inspector'dan sadece yazıları bırakıp bar görsellerini silmektir.
        }

        if (levelText != null)
        {
            levelText.gameObject.SetActive(false);
        }

        if (xpText != null)
        {
            // Eğer daha önce parent gizlendiği için kapalı kaldıysa, parent'ı zorla açalım (güvenlik amaçlı)
            if (xpText.transform.parent != null && !xpText.transform.parent.gameObject.activeSelf)
                xpText.transform.parent.gameObject.SetActive(true);
                
            xpText.gameObject.SetActive(true);
            xpText.text = "Experience: " + gm.playerXP;
        }

        if (repNeedle != null)
        {
            repNeedle.gameObject.SetActive(false);
        }

        if (repText != null)
        {
            repText.text = "Reputation: " + gm.playerReputation;
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
            UpdateLobbyUI();
        }
    }

    /// <summary>
    /// Sahneyi yükler ve hangi chapter oynandığını GameManager'a bildirir.
    /// </summary>
    public void LoadChapter(string sceneName, int chapterNumber)
    {
        Debug.Log(">>> SAHNE YÜKLENİYOR: " + sceneName + " (Chapter " + chapterNumber + ")");
        
        // GameManager'a hangi chapter'ı oynadığımızı kaydet
        if (GameManager.instance != null)
        {
            GameManager.instance.playingChapterNumber = chapterNumber;
            Debug.Log(">>> playingChapterNumber = " + chapterNumber + " ayarlandı");
        }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // Inspector OnClick için - her biri doğrudan chapter numarası ile çağırır
    public void StartChapter1() { LoadChapter("Chapter1", 1); }
    public void StartChapter2() { LoadChapter("Chapter2", 2); }
    public void StartChapter3() { LoadChapter("Chapter3", 3); }
    public void StartChapter4() { LoadChapter("Chapter4", 4); }

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