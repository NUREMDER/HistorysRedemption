using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Ekonomi ve Ýtibar")]
    public int playerGold = 0;
    public int playerReputation = 0;

    [Header("UI Panelleri")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public GameObject fleePanel;
    public GameObject fleeButton;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EnemyDefeated(int goldReward, int repReward)
    {
        if (fleeButton != null) fleeButton.SetActive(false);

        playerGold += goldReward;
        playerReputation += repReward;

        StartCoroutine(ShowVictoryScreen());
    }

    public void PlayerDefeated()
    {
        if (fleeButton != null) fleeButton.SetActive(false);

        playerReputation = Mathf.Max(0, playerReputation - 5);

        StartCoroutine(ShowDefeatScreen());
    }

    public void ShowFleeOption()
    {
        if (fleeButton != null && !fleeButton.activeSelf)
        {
            fleeButton.SetActive(true);
        }
    }

    public void FleeBattle()
    {
        if (fleeButton != null) fleeButton.SetActive(false);

        playerReputation = Mathf.Max(0, playerReputation - 10);

        if (fleePanel != null) fleePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    IEnumerator ShowVictoryScreen()
    {
        yield return new WaitForSeconds(1.5f);
        if (victoryPanel != null) victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    IEnumerator ShowDefeatScreen()
    {
        yield return new WaitForSeconds(1.5f);
        if (defeatPanel != null) defeatPanel.SetActive(true);
        Time.timeScale = 0f;
    }

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