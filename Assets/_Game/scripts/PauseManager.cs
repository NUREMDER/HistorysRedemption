using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 
using System.Collections;

public class PauseManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pausePanel;
    public Animator panelAnimator;
    public TextMeshProUGUI countdownText; // Text element for 3-2-1 countdown

    [Header("Hide on Pause Settings")]
    public GameObject dialoguePanel;      
    public GameObject pauseButton;        

    private bool isPaused = false;
    // --- YENİ DURUM HAFIZASI ---
    private bool wasDialogueActiveBeforePause = false; 
    // ---------------------------

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        
        if (dialoguePanel != null) 
        {
            wasDialogueActiveBeforePause = dialoguePanel.activeSelf; 
            dialoguePanel.SetActive(false);
        }
        
        if (pauseButton != null) pauseButton.SetActive(false);

        if (panelAnimator != null)
        {
            panelAnimator.Play("pauseslide1", 0, 0f);
        }

        Time.timeScale = 0f; 
        isPaused = true;
    }

    public void ResumeGame()
    {   
        Debug.Log("Resume process started...");
        if (pausePanel != null) pausePanel.SetActive(false);
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        if (dialoguePanel != null && wasDialogueActiveBeforePause) 
        {
            dialoguePanel.SetActive(true);
        }
        
        if (pauseButton != null) pauseButton.SetActive(true);

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

            countdownText.text = "GO!"; 
            yield return new WaitForSecondsRealtime(0.5f);
            
            countdownText.gameObject.SetActive(false);
        }

        Time.timeScale = 1f; 
        isPaused = false;
    }

    public void ReturnToLobby()
    {   
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Araf_Lobby"); 
    }
}