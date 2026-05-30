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

    private bool isPaused = false;

    void Update()
    {
        // Toggle pause state when Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        
        // Play the sliding animation for the pause menu window
        if (panelAnimator != null)
        {
            panelAnimator.Play("pauseslide1", 0, 0f);
        }

        // Freeze global game time
        Time.timeScale = 0f; 
        isPaused = true;
    }

    public void ResumeGame()
    {   
        Debug.Log("Resume process started...");
        
        // Hide the panel immediately and start the unpause countdown
        pausePanel.SetActive(false);
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        countdownText.gameObject.SetActive(true);
        int counter = 3;

        // Count down from 3 to 1 using real world time since game time is frozen
        while (counter > 0)
        {
            countdownText.text = counter.ToString();
            yield return new WaitForSecondsRealtime(1f);
            counter--;
        }

        countdownText.text = "GO!"; 
        yield return new WaitForSecondsRealtime(0.5f);
        
        countdownText.gameObject.SetActive(false);

        // Safely unfreeze the game time after countdown finishes
        Time.timeScale = 1f; 
        isPaused = false;
    }

    public void ReturnToLobby()
    {   
        // Always reset time scale to normal before shifting scenes
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Araf_Lobby"); 
    }
}