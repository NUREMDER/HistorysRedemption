using UnityEngine;

/// <summary>
/// Bu scripti her savaş sahnesindeki UI panellerinin parent objesine ekleyin.
/// Sahne yüklendiğinde otomatik olarak GameManager'a panel referanslarını bağlar.
/// Ayrıca butonların OnClick eventlerinde bu scriptin metodlarını kullanın —
/// böylece DontDestroyOnLoad GameManager'a güvenli şekilde erişilir.
/// </summary>
public class BattleUIConnector : MonoBehaviour
{
    [Header("Savaş UI Panelleri")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public GameObject fleePanel;
    public GameObject fleeButton;

    void Start()
    {
        if (GameManager.instance == null)
        {
            Debug.LogWarning("BattleUIConnector: GameManager bulunamadı!");
            return;
        }

        GameManager gm = GameManager.instance;

        gm.victoryPanel = victoryPanel;
        gm.defeatPanel = defeatPanel;
        gm.fleePanel = fleePanel;
        gm.fleeButton = fleeButton;

        Debug.Log("BattleUIConnector: Paneller GameManager'a bağlandı ✓");
    }

    // ─── Butonlar için Relay Metodları ───
    // Panel butonlarının OnClick eventlerinde bu metodları kullanın.

    /// <summary>Lobiye dön butonu</summary>
    public void OnReturnToLobby()
    {
        if (GameManager.instance != null)
            GameManager.instance.ReturnToLobby();
    }

    /// <summary>Ana menüye dön butonu</summary>
    public void OnReturnToMainMenu()
    {
        if (GameManager.instance != null)
            GameManager.instance.ReturnToMainMenu();
    }

    /// <summary>Savaştan kaç butonu</summary>
    public void OnFleeBattle()
    {
        if (GameManager.instance != null)
            GameManager.instance.FleeBattle();
    }
}
