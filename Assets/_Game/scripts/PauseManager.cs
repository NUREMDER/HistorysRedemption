using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro kullanıyorsan bu şart
using System.Collections;

public class PauseManager : MonoBehaviour
{
    [Header("UI Elemanları")]
    public GameObject pausePanel;
    public Animator panelAnimator;
    public TextMeshProUGUI countdownText; // Geri sayım metni (Sürükle bırak)

    private bool isPaused = false;

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
        pausePanel.SetActive(true);
        
        if (panelAnimator != null)
        {
            panelAnimator.Play("pauseslide1", 0, 0f);
        }

        Time.timeScale = 0f; 
        isPaused = true;
    }

    public void ResumeGame()
    {   
        Debug.Log("Resume süreci başladı...");
        // Paneli hemen kapatıyoruz, geri sayımı başlatıyoruz
        pausePanel.SetActive(false);
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        countdownText.gameObject.SetActive(true);
        int counter = 3;

        while (counter > 0)
        {
            countdownText.text = counter.ToString();
            // Time.timeScale = 0 olsa bile gerçek saniye sayması için Realtime kullanıyoruz
            yield return new WaitForSecondsRealtime(1f);
            counter--;
        }

        countdownText.text = "GO!"; // Veya "GO!"
        yield return new WaitForSecondsRealtime(0.5f);
        
        countdownText.gameObject.SetActive(false);

        // Zamanı şimdi geri akıtıyoruz
        Time.timeScale = 1f; 
        isPaused = false;
    }

    public void ReturnToLobby()
    {   
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Araf_Lobby"); 
    }
}