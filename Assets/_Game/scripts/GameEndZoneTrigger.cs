using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Attach this to the GameEndZone trigger object. When the player enters,
/// it freezes the game and shows a "Game Completed" panel with
/// Return to Araf and Return to Main Menu buttons.
/// </summary>
public class GameEndZoneTrigger : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject gameEndPanel;

    [Header("Hide on Game End")]
    public GameObject pauseCanvas;
    public GameObject healthBarUI;

    private bool hasTriggered = false;

    void Start()
    {
        // Ensure the panel is hidden at start
        if (gameEndPanel != null)
            gameEndPanel.SetActive(false);
    }

    // 3D trigger for Chapter 4 parkour character
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            ShowGameEndPanel(other.gameObject);
        }
    }

    private void ShowGameEndPanel(GameObject player)
    {
        // Freeze the player
        ParkourController parkour = player.GetComponent<ParkourController>();
        if (parkour != null)
        {
            parkour.PauseParkour();
        }

        // Save progress
        if (GameManager.instance != null)
        {
            GameManager.instance.SaveProgress();
        }

        // Hide pause button and health bar
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (healthBarUI != null) healthBarUI.SetActive(false);

        // Show panel and pause game
        if (gameEndPanel != null)
        {
            gameEndPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    // ─── Button Functions (assign these to UI buttons via Inspector) ───

    public void ReturnToLobby()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Araf_Lobby");
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
