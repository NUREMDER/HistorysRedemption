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
        
        // Fırlatıldığı yönü belirle (Sağa veya sola).
        // Player sağa bakıyorsa rotation Y=0, sola bakıyorsa rotation Y=180 olacaktır.
        rb.velocity = transform.right * speed;

        // Bıçak bir yere çarpmazsa 2 saniye sonra yok olsun.
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Eğer çarptığımız obje "Player" veya kendi trigger'ımız ise yoksay.
        if (hitInfo.CompareTag("Player")) return;

        // Düşmana çarptıysak hasar ver
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

        // Yere, duvara veya düşmana çarptığı an bıçağı yok et
        Destroy(gameObject);
    }
}
