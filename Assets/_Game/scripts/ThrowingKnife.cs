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
        if (trail != null) trail.enabled = false;
        
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Kendi karakterimize çarpmasını engelle
        if (hitInfo.CompareTag("Player")) return;

        // 4. "EnemyAI" bileşeni, dokunulan objede VEYA onun en üst (Parent) objesinde var mı kontrol et
        EnemyAI enemy = hitInfo.GetComponentInParent<EnemyAI>();
        
        if (enemy != null)
        {
            int totalDamage = damage;
            if (GameManager.instance != null)
            {
                totalDamage += GameManager.instance.bonusDamage;
            }
            
            enemy.TakeDamage(totalDamage);
            Debug.Log("Bıçak düşmanı vurdu: " + enemy.gameObject.name);
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
