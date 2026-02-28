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

    // --- GÜNCELLEME: Artýk StartDialogue içine bir Boss (EnemyAI) da alabiliyor ---
    public void StartDialogue(DialogueLine[] lines, PlayerController player, EnemyAI boss = null)
    {
        currentDialogue = lines;
        currentLineIndex = 0;

        activePlayer = player;
        activeBoss = boss; // Boss'u hafýzaya al

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
        dialoguePanel.SetActive(false);
        nextButton.SetActive(false);

        
        if (activePlayer != null)
        {
            activePlayer.enabled = true;
        }

        
        if (activeBoss != null)
        {
            activeBoss.enabled = true;
            Debug.Log("TESLA UYANDI! SAVAÞ BAÞLADI!");
        }
    }
}