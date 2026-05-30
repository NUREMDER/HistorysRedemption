using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject marketPanel; 

    [Header("Chapter Selection Buttons")]
    public Button[] chapterButtons; // 4 buttons: index 0=Ch1, 1=Ch2, 2=Ch3, 3=Ch4

    [Header("Locked Button Appearance")]
    [Range(0f, 1f)]
    public float lockedAlpha = 0.3f;

    [Header("XP & Level UI")]
    public Image xpBarFill;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;

    [Header("Reputation UI")]
    public RectTransform repNeedle;
    public TextMeshProUGUI repText;
    public float repNeedleMaxOffset = 150f;
    public int maxReputation = 100;

    // Scene names matching button indexes
    private readonly string[] chapterSceneNames = { "Chapter1", "Chapter2", "Chapter3", "Chapter4" };

    void Start()
    {
        SetupChapterButtons();
        UpdateLobbyUI();
        CheckChapterLocks();
    }

    // Assigns onClick listeners to each chapter button dynamically
    private void SetupChapterButtons()
    {
        if (chapterButtons == null || chapterButtons.Length == 0)
        {
            Debug.LogError("LOBBY SETUP: chapterButtons array is empty or not assigned!");
            return;
        }

        for (int i = 0; i < chapterButtons.Length; i++)
        {
            if (chapterButtons[i] == null)
            {
                Debug.LogError("LOBBY SETUP: chapterButtons[" + i + "] is null! Assign it in Inspector.");
                continue;
            }

            // Local variables for closure
            int chapterNum = i + 1; 
            string sceneName = (i < chapterSceneNames.Length) 
                ? chapterSceneNames[i] 
                : "Chapter" + chapterNum;

            // Add runtime listener without wiping existing Inspector onClick events
            chapterButtons[i].onClick.AddListener(() => LoadChapter(sceneName, chapterNum));

            Debug.Log("LOBBY SETUP: " + chapterButtons[i].name + " → " + sceneName + " (Chapter " + chapterNum + ") assigned ✓");
        }
    }

    // Controls button interactability based on progression
    public void CheckChapterLocks()
    {
        if (GameManager.instance == null) { Debug.LogError("LOBBY: GameManager not found!"); return; }
        if (chapterButtons == null || chapterButtons.Length == 0) { Debug.LogError("LOBBY: Buttons array is empty!"); return; }

        int unlocked = GameManager.instance.unlockedChapter;
        Debug.Log("LOBBY CONTROL: unlockedChapter = " + unlocked);

        for (int i = 0; i < chapterButtons.Length; i++)
        {
            if (chapterButtons[i] == null) { Debug.LogError("LOBBY: Button at index " + i + " is empty!"); continue; }

            bool isUnlocked = (i + 1 <= unlocked);
            
            // Toggle click functionality
            chapterButtons[i].interactable = isUnlocked;

            // Handle visual alpha using CanvasGroup
            CanvasGroup cg = chapterButtons[i].GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = chapterButtons[i].gameObject.AddComponent<CanvasGroup>();
            }
            cg.alpha = isUnlocked ? 1f : lockedAlpha;
            cg.blocksRaycasts = true;  
            cg.interactable = true;    

            Debug.Log(chapterButtons[i].name + " → " + (isUnlocked ? "UNLOCKED ✓" : "LOCKED ✗"));
        }
    }

    // Refreshes texts and hides unused UI layout elements
    public void UpdateLobbyUI()
    {
        if (GameManager.instance == null) return;

        GameManager gm = GameManager.instance;

        if (xpBarFill != null)
        {
            // Hide fill bar graphic safely
            xpBarFill.gameObject.SetActive(false);
        }

        if (levelText != null)
        {
            levelText.gameObject.SetActive(false);
        }

        if (xpText != null)
        {
            // Force active parent object if it was disabled before
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

    // Sets the active chapter index and loads the scene
    public void LoadChapter(string sceneName, int chapterNumber)
    {
        Debug.Log(">>> LOADING SCENE: " + sceneName + " (Chapter " + chapterNumber + ")");
        
        if (GameManager.instance != null)
        {
            GameManager.instance.playingChapterNumber = chapterNumber;
            Debug.Log(">>> playingChapterNumber = " + chapterNumber + " updated");
        }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // Inspector OnClick fallbacks for direct calls
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
        Debug.Log("Exiting game...");
        Application.Quit();
    }
}