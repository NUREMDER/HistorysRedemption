using UnityEngine;

public class TeslaProjectile : MonoBehaviour
{
    [Header("Mermi Ayarlari")]
    public float speed = 15f;
    public int damage = 25;
    public float lifetime = 3f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.isTrigger = true;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            // transform.right zaten TeslaAI tarafindan oyuncuya dogru ayarlanmis olacak
            rb.velocity = transform.right * speed;
        }

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Tesla'nin kendi objeleri ve diger dusmanlara carpmasin
        if (hitInfo.CompareTag("Enemy")) return;
        if (hitInfo.GetComponentInParent<TeslaAI>() != null) return;
        if (hitInfo.GetComponentInParent<EnemyAI>() != null) return;

        PlayerController player = hitInfo.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Debug.Log("Tesla elektrigi oyuncuyu vurdu!");
            Destroy(gameObject);
        }
        else if (!hitInfo.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
