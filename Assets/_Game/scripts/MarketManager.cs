using UnityEngine;
using TMPro;

public class MarketManager : MonoBehaviour
{
    [Header("UI Texts")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI healthPriceText;
    public TextMeshProUGUI damagePriceText;

    [Header("Starting Prices")]
    public int healthUpgradePrice = 50;
    public int damageUpgradePrice = 100;

    void OnEnable()
    {
        UpdateUI();
    }

    public void BuyHealthUpgrade()
    {
        if (GameManager.instance.playerGold >= healthUpgradePrice)
        {
            GameManager.instance.playerGold -= healthUpgradePrice;
            GameManager.instance.bonusMaxHealth += 20;
            healthUpgradePrice += 25;

            UpdateUI();
            Debug.Log("HEALTH UPGRADED! New Bonus: " + GameManager.instance.bonusMaxHealth);
        }
        else
        {
            Debug.Log("NOT ENOUGH GOLD!");
        }
    }

    public void BuyDamageUpgrade()
    {
        if (GameManager.instance.playerGold >= damageUpgradePrice)
        {
            GameManager.instance.playerGold -= damageUpgradePrice;
            GameManager.instance.bonusDamage += 5;
            damageUpgradePrice += 50;

            UpdateUI();
            Debug.Log("DAMAGE UPGRADED! New Bonus: " + GameManager.instance.bonusDamage);
        }
        else
        {
            Debug.Log("NOT ENOUGH GOLD!");
        }
    }

    void UpdateUI()
    {
        if (GameManager.instance != null)
        {
            if (goldText != null)
                goldText.text = "Current Gold: " + GameManager.instance.playerGold;

            if (healthPriceText != null)
                healthPriceText.text = "Upgrade: " + healthUpgradePrice + " Gold\n(+20 Max Health)";

            if (damagePriceText != null)
                damagePriceText.text = "Upgrade: " + damageUpgradePrice + " Gold\n(+5 Extra Damage)";
        }
    }
}