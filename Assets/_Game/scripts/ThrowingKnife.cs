using UnityEngine;

public class ThrowingKnife : MonoBehaviour
{
    [Header("Bıçak Ayarları")]
    public float speed = 15f;
    public int damage = 25;
    public float lifetime = 2f; 

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Bıçağın Y rotasyonu 180 çevrildiği için "transform.right"ın tersini (sola doğru olanı) alıyoruz.
        rb.velocity = -transform.right * speed;

        
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        
        if (hitInfo.CompareTag("Player")) return;

    
        EnemyAI enemy = hitInfo.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            // İleride GameManager'dan ek hasar (bonus damage) almak istersen:
            int totalDamage = damage;
            if (GameManager.instance != null)
            {
                totalDamage += GameManager.instance.bonusDamage;
            }
            
            enemy.TakeDamage(totalDamage);
        }

        
        Destroy(gameObject);
    }
}
