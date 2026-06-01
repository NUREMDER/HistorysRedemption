using UnityEngine;
using TMPro;

public class MarketManager : MonoBehaviour
{
    [Header("UI Texts")]
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI healthPriceText;
    public TextMeshProUGUI damagePriceText;
    public TextMeshProUGUI reputationText;
    public TextMeshProUGUI damagebutton;
    public TextMeshProUGUI healthbutton;

    [Header("Knife Settings")]
    public TextMeshProUGUI knifePriceText;
    public TextMeshProUGUI knifeButtonText;
    public TextMeshProUGUI knifeCountText;
    public int knifePackagePrice = 50;
    public int knifePackageAmount = 5;

    [Header("Knife Upgrade Settings")]
    public int knifeUpgradePrice = 150;
    public TextMeshProUGUI knifeUpgradeButtonText;

    [Header("Starting Prices (XP)")]
    public int healthUpgradePrice = 50;
    public int damageUpgradePrice = 100;

    [Header("Reputation Settings")]
    public float maxDiscountPercent = 0.5f; 
    public float repEffectMultiplier = 0.001f; 

    [Header("Lobby UI Elements (To Hide)")]
    public GameObject lobbyXPContainer;
    public GameObject lobbyRepContainer; 

    // Hide lobby UI panels and refresh market text when panel opens
    void OnEnable()
    {
        UpdateUI();
        if (lobbyXPContainer != null) lobbyXPContainer.SetActive(false);
        if (lobbyRepContainer != null) lobbyRepContainer.SetActive(false);
    }

    // Restore lobby UI panels when market panel closes
    void OnDisable()
    {
        if (lobbyXPContainer != null) lobbyXPContainer.SetActive(true);
        if (lobbyRepContainer != null) lobbyRepContainer.SetActive(true);
    }

    // Purchase max health upgrade using discounted XP price
    public void BuyHealthUpgrade()
    {
        int currentPrice = GetDiscountedPrice(healthUpgradePrice);

        if (GameManager.instance.playerXP >= currentPrice)
        {
            GameManager.instance.playerXP -= currentPrice;
            GameManager.instance.bonusMaxHealth += 20;
            healthUpgradePrice += 25; // Increase base price for the next buy

            GameManager.instance.SaveProgress();
            UpdateUI();
        }
    }

    // Calculates price reductions dynamically based on player's reputation
    public int GetDiscountedPrice(int basePrice)
    {
        if (GameManager.instance == null) return basePrice;

        float discountRate = Mathf.Clamp(GameManager.instance.playerReputation * repEffectMultiplier, 0, maxDiscountPercent);
        int finalPrice = Mathf.RoundToInt(basePrice * (1.0f - discountRate));
        return finalPrice;
    }

    // Purchase a pack of throwing knives
    public void BuyKnives()
    {
        int currentPrice = GetDiscountedPrice(knifePackagePrice);

        if (GameManager.instance.playerXP >= currentPrice)
        {
            GameManager.instance.playerXP -= currentPrice;
            GameManager.instance.playerKnives += knifePackageAmount;
            
            // Automatically unlock the first tier if it was locked
            GameManager.instance.UnlockFirstKnife();
            
            GameManager.instance.SaveProgress();
            UpdateUI();
        }
    }

    // Upgrades knife tier level (Max Level 3)
    public void BuyKnifeUpgrade()
    {
        if (GameManager.instance == null) return;

        int currentLevel = GameManager.instance.unlockedKnifeLevel;
        if (currentLevel == 0 || currentLevel >= 3) return; 

        int currentPrice = GetDiscountedPrice(knifeUpgradePrice);

        if (GameManager.instance.playerXP >= currentPrice)
        {
            GameManager.instance.playerXP -= currentPrice;
            GameManager.instance.unlockedKnifeLevel++;
            knifeUpgradePrice += 100; // Increase price for next tier upgrade

            GameManager.instance.SaveProgress();
            UpdateUI();
        }
    }

    // Purchase permanent bonus damage upgrade using discounted XP price
    public void BuyDamageUpgrade()
    {
        int currentPrice = GetDiscountedPrice(damageUpgradePrice);

        if (GameManager.instance.playerXP >= currentPrice)
        {
            GameManager.instance.playerXP -= currentPrice;
            GameManager.instance.bonusDamage += 5;
            damageUpgradePrice += 50; // Increase base price for the next buy

            GameManager.instance.SaveProgress();
            UpdateUI();
        }
    }

    // Updates all market panel text fields with calculated discounted prices
    void UpdateUI()
    {   
        if (GameManager.instance != null)
        {
            if (reputationText != null)
                reputationText.text = "Reputation: " + GameManager.instance.playerReputation;

            int hPrice = GetDiscountedPrice(healthUpgradePrice);
            int dPrice = GetDiscountedPrice(damageUpgradePrice);

            if (xpText != null)
                xpText.text = "Experience: " + GameManager.instance.playerXP;

            if (healthPriceText != null)
                healthPriceText.text = "(+20 Max Health)";

            if(healthbutton != null){
                healthbutton.text = "Upgrade: " + hPrice + "XP";
            }

            if (damagePriceText != null){
                damagePriceText.text = "(+5 Extra Damage)";
            }
            if(damagebutton != null){
                damagebutton.text = "Upgrade: " + dPrice + "XP";
            }

            int kPrice = GetDiscountedPrice(knifePackagePrice);
            if (knifePriceText != null)
                knifePriceText.text = "(+" + knifePackageAmount + " Knives)";
            if (knifeButtonText != null)
                knifeButtonText.text = "Buy: " + kPrice + "XP";
            if (knifeCountText != null)
                knifeCountText.text = "Knives Owned: " + GameManager.instance.playerKnives;

            // Change upgrade button text dynamically depending on current knife tier level
            if (knifeUpgradeButtonText != null)
            {
                int currentLevel = GameManager.instance.unlockedKnifeLevel;
                if (currentLevel == 0)
                {
                    knifeUpgradeButtonText.text = "UPGRADE (LOCKED)";
                }
                else if (currentLevel >= 3)
                {
                    knifeUpgradeButtonText.text = "UPGRADE (MAX)";
                }
                else
                {
                    int kuPrice = GetDiscountedPrice(knifeUpgradePrice);
                    knifeUpgradeButtonText.text = "Upgrade (Lv" + (currentLevel + 1) + "): " + kuPrice + "XP";
                }
            }
        }
    }
}