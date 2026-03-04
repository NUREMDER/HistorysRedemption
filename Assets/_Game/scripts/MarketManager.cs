using UnityEngine;
using TMPro;

public class MarketManager : MonoBehaviour
{
    [Header("UI Texts")]
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI healthPriceText;
    public TextMeshProUGUI damagePriceText;

    [Header("Starting Prices (XP)")]
    public int healthUpgradePrice = 50;
    public int damageUpgradePrice = 100;

    void OnEnable()
    {
        UpdateUI();
    }

    public void BuyHealthUpgrade()
    {
        if (GameManager.instance.playerXP >= healthUpgradePrice)
        {
            GameManager.instance.playerXP -= healthUpgradePrice;
            GameManager.instance.bonusMaxHealth += 20;
            healthUpgradePrice += 25;

            UpdateUI();
        }
    }

    public void BuyDamageUpgrade()
    {
        if (GameManager.instance.playerXP >= damageUpgradePrice)
        {
            GameManager.instance.playerXP -= damageUpgradePrice;
            GameManager.instance.bonusDamage += 5;
            damageUpgradePrice += 50;

            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (GameManager.instance != null)
        {
            if (xpText != null)
                xpText.text = "Current XP: " + GameManager.instance.playerXP;

            if (healthPriceText != null)
                healthPriceText.text = "Upgrade: " + healthUpgradePrice + " XP\n(+20 Max Health)";

            if (damagePriceText != null)
                damagePriceText.text = "Upgrade: " + damageUpgradePrice + " XP\n(+5 Extra Damage)";
        }
    }
}