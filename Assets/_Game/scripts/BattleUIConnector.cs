using UnityEngine;

//Automatically links battle UI panels and button events to the GameManager on scene load.
public class BattleUIConnector : MonoBehaviour
{
    [Header("Battle UI Panels")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public GameObject fleePanel;
    public GameObject fleeButton;

    void Start()
    {
        if (GameManager.instance == null)
        {
            Debug.LogWarning("BattleUIConnector: GameManager not found!");
            return;
        }

        GameManager gm = GameManager.instance;

        gm.victoryPanel = victoryPanel;
        gm.defeatPanel = defeatPanel;
        gm.fleePanel = fleePanel;
        gm.fleeButton = fleeButton;

        Debug.Log("BattleUIConnector: Panels connected to GameManager");
    }

    // ─── BUTTON METHODS ───

    /// back to lobby button
    public void OnReturnToLobby()
    {
        if (GameManager.instance != null)
            GameManager.instance.ReturnToLobby();
    }

    /// main menu button
    public void OnReturnToMainMenu()
    {
        if (GameManager.instance != null)
            GameManager.instance.ReturnToMainMenu();
    }

    /// flee button
    public void OnFleeBattle()
    {
        if (GameManager.instance != null)
            GameManager.instance.FleeBattle();
    }
}
