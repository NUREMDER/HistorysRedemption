using UnityEngine;
using TMPro;
using System.Collections;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(3, 10)]
    public string sentence;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject nextButton;

    public float typingSpeed = 0.04f;

    private DialogueLine[] currentDialogue;
    private int currentLineIndex = 0;
    private bool isTyping = false;

    private PlayerController activePlayer;
    private EnemyAI activeBoss;
    private TeslaAI activeTeslaBoss;
    private NewtonAI activeNewtonBoss;
    private ParkourController activeParkourController;

    public GameObject bossHealthBar;

    public TextMeshProUGUI countdownText;

    void Awake()
    {
        if (instance == null) instance = this;
    }
    void Update()
    {
        if (dialoguePanel != null && dialoguePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                OnNextClicked();
            }
        }
    }

    public void OnNextClicked()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentDialogue[currentLineIndex].sentence;
            isTyping = false;
        }
        else
        {
            DisplayNextLine();
        }
    }

    //Starts dialogue beginning of parkour
    public void StartDialogue(DialogueLine[] lines, ParkourController parkourController)
    {
        currentDialogue = lines;
        currentLineIndex = 0;

        activePlayer = null;
        activeBoss = null;
        activeTeslaBoss = null;
        activeParkourController = parkourController;

        if (activeParkourController != null)
        {
            activeParkourController.PauseParkour();
        }

        dialoguePanel.SetActive(true);
        nextButton.SetActive(true);
        StartCoroutine(TypeLine());
    }

    // Starts dialogue before fight
    public void StartDialogue(DialogueLine[] lines, PlayerController player, EnemyAI boss = null, TeslaAI teslaBoss = null, NewtonAI newtonBoss = null)
    {
        currentDialogue = lines;
        currentLineIndex = 0;

        activePlayer = player;
        activeBoss = boss;
        activeTeslaBoss = teslaBoss;
        activeNewtonBoss = newtonBoss;
        activeParkourController = null;

        // Disable EnemyAI boss during dialogue to prevent movement
        if (activeBoss != null)
        {
            activeBoss.enabled = false;
            Rigidbody2D bossRb = activeBoss.GetComponent<Rigidbody2D>();
            if (bossRb != null) bossRb.velocity = Vector2.zero;
        }

        if (activeTeslaBoss != null)
        {
            activeTeslaBoss.enabled = false;
            Rigidbody2D rb = activeTeslaBoss.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
        }

        if (activeNewtonBoss != null)
        {
            activeNewtonBoss.enabled = false;
            Rigidbody2D rb = activeNewtonBoss.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
        }

        if (activePlayer != null)
        {
            activePlayer.enabled = false;
            Rigidbody2D rb = activePlayer.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
            Animator anim = activePlayer.GetComponent<Animator>();
            if (anim != null) anim.SetFloat("Speed", 0);
        }

        dialoguePanel.SetActive(true);
        nextButton.SetActive(true);
        StartCoroutine(TypeLine());
    }

    public void DisplayNextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < currentDialogue.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        nameText.text = currentDialogue[currentLineIndex].speakerName;
        dialogueText.text = "";

        foreach (char c in currentDialogue[currentLineIndex].sentence.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        Debug.Log("DialogueManager: EndDialogue trigerred. ActiveBoss: " + (activeBoss != null));
        dialoguePanel.SetActive(false);
        nextButton.SetActive(false);

        // Countdown before fight
        // Starts countdown and health bar only if an active boss exists.
        if (bossHealthBar != null && (activeBoss != null || activeTeslaBoss != null || activeNewtonBoss != null))
        {
            StartCoroutine(CountdownRoutine());
        }
        else
        {
            if (activePlayer != null)
            {
                activePlayer.enabled = true;
            }

            // Only activate the boss bar if an actual boss exists in the scene.
            // This prevents the bar from showing up during dialogue segments.
            if (bossHealthBar != null && (activeBoss != null || activeTeslaBoss != null || activeNewtonBoss != null))
            {
                bossHealthBar.SetActive(true); 
                Debug.Log("DialogueManager: Dialogue ended: Boss Health Bar forced active." + bossHealthBar.name);
            }
            else
            {
                // Eğer dövüş sahnesindeysek ve hala görünmüyorsa Inspector uyarısı versin, parkurda hata vermesin
                if (activeBoss != null || activeTeslaBoss != null || activeNewtonBoss != null)
                {
                    Debug.LogError("DialogueManager: bossHealthBar object is empty !");
                }
            }

            if (activeBoss != null)
            {
                activeBoss.enabled = true;
                Debug.Log("DialogueManager: Boss awakened! Battle started!");
            }
            if (activeTeslaBoss != null)
            {
                activeTeslaBoss.enabled = true;
                Debug.Log("DialogueManager: TeslaBoss awakened! Battle started!");
            }
            if (activeNewtonBoss != null)
            {
                activeNewtonBoss.enabled = true;
                Debug.Log("DialogueManager: NewtonBoss awakened! Battle started!");
            }

            // Shadow1 parkura devam etsin
            if (activeParkourController != null)
            {
                activeParkourController.ResumeParkour();
                Debug.Log("DialogueManager: Dialogue finished, resume parkour.");
            }
        }
    }

    IEnumerator CountdownRoutine()
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.SetActive(true);
            Debug.Log("DialogueManager: Dialogue finished, Boss Health Bar forced active: " + bossHealthBar.name);
        }

        Time.timeScale = 0f;

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);

            int counter = 3;
            while (counter > 0)
            {
                countdownText.text = counter.ToString();
                yield return new WaitForSecondsRealtime(1f);
                counter--;
            }

            // after countdown set text unactive
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            // Text atanmamışsa bile oyunu başlatmadan önce 3 saniye beklesin (görünmez sayım)
            yield return new WaitForSecondsRealtime(3f);
        }

        Time.timeScale = 1f;

        if (activePlayer != null)
        {
            activePlayer.enabled = true;
        }

        if (activeBoss != null)
        {
            activeBoss.enabled = true;
            Debug.Log("DialogueManager: Boss awakened! Battle started!");
        }

        if (activeTeslaBoss != null)
        {
            activeTeslaBoss.enabled = true;
            Debug.Log("DialogueManager: TeslaBoss awakened! Battle started!");
        }

        if (activeNewtonBoss != null)
        {
            activeNewtonBoss.enabled = true;
            Debug.Log("DialogueManager: NewtonBoss awakened! Battle started!");
        }

        if (activeParkourController != null)
        {
            activeParkourController.ResumeParkour();
            Debug.Log("DialogueManager: Dialogue finished, resume parkour.");
        }
    }
}