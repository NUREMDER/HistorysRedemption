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

    [Header("Starting Prices (XP)")]
    public int healthUpgradePrice = 50;
    public int damageUpgradePrice = 100;

    [Header("Reputation Settings")]
    public float maxDiscountPercent = 0.5f; 
    public float repEffectMultiplier = 0.001f; 

    [Header("Lobby UI Elements (To Hide)")]
    public GameObject lobbyXPContainer;
    public GameObject lobbyRepContainer; 

    void OnEnable()
    {
        UpdateUI();
        if (lobbyXPContainer != null) lobbyXPContainer.SetActive(false);
        if (lobbyRepContainer != null) lobbyRepContainer.SetActive(false);
    }

    void OnDisable()
    {
        if (lobbyXPContainer != null) lobbyXPContainer.SetActive(true);
        if (lobbyRepContainer != null) lobbyRepContainer.SetActive(true);
        
    }

    public void BuyHealthUpgrade()
    {
        int currentPrice = GetDiscountedPrice(healthUpgradePrice);

        // Kontrol ve harcama artık indirimli fiyat (currentPrice) üzerinden yapılıyor
        if (GameManager.instance.playerXP >= currentPrice)
        {
            GameManager.instance.playerXP -= currentPrice;
            GameManager.instance.bonusMaxHealth += 20;
            healthUpgradePrice += 25;

            // Verileri kalıcı olarak kaydet
            GameManager.instance.SaveProgress();
            UpdateUI();
        }
    }

    public int GetDiscountedPrice(int basePrice)
    {
        if (GameManager.instance == null) return basePrice;

        float discountRate = Mathf.Clamp(GameManager.instance.playerReputation * repEffectMultiplier, 0, maxDiscountPercent);
        int finalPrice = Mathf.RoundToInt(basePrice * (1.0f - discountRate));
        return finalPrice;
    }

    public void BuyDamageUpgrade()
    {
        int currentPrice = GetDiscountedPrice(damageUpgradePrice);

        // Kontrol ve harcama artık indirimli fiyat (currentPrice) üzerinden yapılıyor
        if (GameManager.instance.playerXP >= currentPrice)
        {
            GameManager.instance.playerXP -= currentPrice;
            GameManager.instance.bonusDamage += 5;
            damageUpgradePrice += 50;

            // Verileri kalıcı olarak kaydet
            GameManager.instance.SaveProgress();
            UpdateUI();
        }
    }

    void UpdateUI()
    {   
        if (GameManager.instance != null)
        {
            if (reputationText != null)
                reputationText.text = "Reputation: " + GameManager.instance.playerReputation;

            // Fiyatları UI'da gösterirken indirimli hallerini hesaplıyoruz
            int hPrice = GetDiscountedPrice(healthUpgradePrice);
            int dPrice = GetDiscountedPrice(damageUpgradePrice);

            if (xpText != null)
                xpText.text = "XP: " + GameManager.instance.playerXP;

            if (healthPriceText != null)
                healthPriceText.text = "(+20 Max Health)";

            if(healthbutton != null){
                // Buton üzerinde indirimli fiyatı gösteriyoruz
                healthbutton.text = "Upgrade: " + hPrice + "XP";
            }

            if (damagePriceText != null){
                damagePriceText.text = "(+5 Extra Damage)";
            }
            if(damagebutton != null){
                // Buton üzerinde indirimli fiyatı gösteriyoruz
                damagebutton.text = "Upgrade: " + dPrice + "XP";
            }
        }
    }
}