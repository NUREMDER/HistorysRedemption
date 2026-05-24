using UnityEngine;

public class ThrowingKnife : MonoBehaviour
{
    [Header("Bıçak Ayarları")]
    public float speed = 15f;
    public int damage = 25;
    public float lifetime = 2f; 
    public bool isEnemyProjectile = false; // Düşman tarafından fırlatıldıysa işaretlenir

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 1. Fiziksel olarak çarpışıp durmasını engellemek için kodla Trigger yapıyoruz
        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.isTrigger = true;

        if (rb != null)
        {
            rb.gravityScale = 0f; // Yere düşmesini engeller
            
            // 2. Bıçağın oyuncunun baktığı yöne uçmasını sağla (- işareti kaldırıldı!)
            rb.velocity = transform.right * speed;
        }

        // 3. Y rotasyonunu 180 derece döndürüyoruz
        transform.Rotate(0f, 180f, 0f);

        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.enabled = true;
        
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (isEnemyProjectile)
        {
            // Düşmanın kendi mermisi kendisine veya kendi triggerlarına çarpmasın
            if (hitInfo.CompareTag("Enemy") || hitInfo.GetComponentInParent<EnemyAI>() != null || hitInfo.GetComponentInParent<TeslaAI>() != null) return;

            PlayerController player = hitInfo.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("Düşman mermisi oyuncuyu vurdu!");
                Destroy(gameObject);
            }
            else if (!hitInfo.isTrigger)
            {
                Debug.Log("Düşman mermisi " + hitInfo.name + " objesine çarptığı için YOK OLDU!");
                Destroy(gameObject);
            }
        }
        else
        {
            // Kendi karakterimize çarpmasını engelle
            if (hitInfo.CompareTag("Player")) return;

            // 4. "EnemyAI" veya "TeslaAI" bileşeni var mı kontrol et
            EnemyAI enemy = hitInfo.GetComponentInParent<EnemyAI>();
            TeslaAI teslaEnemy = hitInfo.GetComponentInParent<TeslaAI>();
            
            if (enemy != null || teslaEnemy != null)
            {
                int totalDamage = damage;
                if (GameManager.instance != null)
                {
                    totalDamage += GameManager.instance.bonusDamage;
                }
                
                if (enemy != null)
                {
                    enemy.TakeDamage(totalDamage);
                    Debug.Log("Bıçak düşmanı vurdu: " + enemy.gameObject.name);
                }
                else if (teslaEnemy != null)
                {
                    teslaEnemy.TakeDamage(totalDamage);
                    Debug.Log("Bıçak Tesla'yı vurdu: " + teslaEnemy.gameObject.name);
                }

                Destroy(gameObject); // Düşmana vurduktan sonra yok ol
            }
            else
            {
                // Eğer trigger olmayan bir duvara / yere çarpılırsa yok ol
                if (!hitInfo.isTrigger)
                {
                    Debug.Log("Bıçak " + hitInfo.name + " objesine çarptığı için YOK OLDU!");
                    Destroy(gameObject);
                }
            }
        }
    }
}
