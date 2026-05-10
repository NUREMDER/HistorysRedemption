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
    private ParkourController activeParkourController;

    public GameObject bossHealthBar;

    public TextMeshProUGUI countdownText;

    void Awake()
    {
        if (instance == null) instance = this;
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

    // Shadow1 (ParkourController) icin diyalog baslatma
    public void StartDialogue(DialogueLine[] lines, ParkourController parkourController)
    {
        currentDialogue = lines;
        currentLineIndex = 0;

        activePlayer = null;
        activeBoss = null;
        activeParkourController = parkourController;

        if (activeParkourController != null)
        {
            activeParkourController.PauseParkour();
        }

        dialoguePanel.SetActive(true);
        nextButton.SetActive(true);
        StartCoroutine(TypeLine());
    }

    // Player + Boss icin diyalog baslatma (mevcut)
    public void StartDialogue(DialogueLine[] lines, PlayerController player, EnemyAI boss = null, TeslaAI teslaBoss = null)
    {
        currentDialogue = lines;
        currentLineIndex = 0;

        activePlayer = player;
        activeBoss = boss;
        activeTeslaBoss = teslaBoss;
        activeParkourController = null;

        if (activeTeslaBoss != null)
        {
            activeTeslaBoss.enabled = false;
            Rigidbody2D rb = activeTeslaBoss.GetComponent<Rigidbody2D>();
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
        Debug.Log("EndDialogue tetiklendi. ActiveBoss: " + (activeBoss != null));
        dialoguePanel.SetActive(false);
        nextButton.SetActive(false);

        // --- COUNTDOWN MANTIĞI ---
        if (bossHealthBar != null)
        {
            StartCoroutine(CountdownRoutine());
        }
        else
        {
            if (activePlayer != null)
            {
                activePlayer.enabled = true;
            }

            // --- CAN BARINI AKTİF ETME (Garantili ve Şartsız Yöntem) ---
            // Boss'un aktif olup olmamasından bağımsız olarak, diyalog bittiği an bu barı açıyoruz.
            if (bossHealthBar != null)
            {
                bossHealthBar.SetActive(true); 
                Debug.Log("Diyalog bitti: Boss Can Barı zorla aktif edildi: " + bossHealthBar.name);
            }
            else
            {
                // Eğer hala görünmüyorsa Inspector'dan sürüklemeyi unutmuşsun demektir.
                Debug.LogError("DİKKAT: bossHealthBar kutucuğu hala boş!");
            }

            if (activeBoss != null)
            {
                activeBoss.enabled = true;
                Debug.Log("BOSS UYANDI! SAVAS BASLADI!");
            }
            if (activeTeslaBoss != null)
            {
                activeTeslaBoss.enabled = true;
                Debug.Log("TESLA UYANDI! SAVAS BASLADI!");
            }

            // Shadow1 parkura devam etsin
            if (activeParkourController != null)
            {
                activeParkourController.ResumeParkour();
                Debug.Log("Diyalog bitti, parkura devam!");
            }
        }
    }

    IEnumerator CountdownRoutine()
    {
        if (bossHealthBar != null)
        {
            bossHealthBar.SetActive(true);
            Debug.Log("Diyalog bitti: Boss Can Barı zorla aktif edildi: " + bossHealthBar.name);
        }

        Time.timeScale = 0f;
        countdownText.gameObject.SetActive(true);

        int counter = 3;
        while (counter > 0)
        {
            countdownText.text = counter.ToString();
            yield return new WaitForSecondsRealtime(1f);
            counter--;
        }

        countdownText.text = "FIGHT!";
        yield return new WaitForSecondsRealtime(0.7f);
        countdownText.gameObject.SetActive(false);

        Time.timeScale = 1f;

        if (activePlayer != null)
        {
            activePlayer.enabled = true;
        }

        if (activeBoss != null)
        {
            activeBoss.enabled = true;
            Debug.Log("BOSS UYANDI! SAVAS BASLADI!");
        }

        if (activeTeslaBoss != null)
        {
            activeTeslaBoss.enabled = true;
            Debug.Log("TESLA UYANDI! SAVAS BASLADI!");
        }

        if (activeParkourController != null)
        {
            activeParkourController.ResumeParkour();
            Debug.Log("Diyalog bitti, parkura devam!");
        }
    }
}