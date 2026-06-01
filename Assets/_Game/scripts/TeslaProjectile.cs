using UnityEngine;

public class TeslaProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 15f;
    public int damage = 25;
    public float lifetime = 3f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Ensure the collider is set to trigger so it doesn't cause rigid physics crashes
        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.isTrigger = true;

        if (rb != null)
        {
            rb.gravityScale = 0f; // Disable gravity so it flies perfectly straight
            
            // Set the velocity downwards since the lightning bolt drops straight from the sky
            rb.velocity = Vector2.down * speed;
        }

        // Self-destruct after lifetime ends to save memory if it misses the player
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Ignore collisions with the Tesla boss itself or any other enemy objects
        if (hitInfo.CompareTag("Enemy")) return;
        if (hitInfo.GetComponentInParent<TeslaAI>() != null) return;
        if (hitInfo.GetComponentInParent<EnemyAI>() != null) return;

        // Check if the projectile hit the player
        PlayerController player = hitInfo.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Debug.Log("Tesla lightning struck the player!");
            Destroy(gameObject); // Destroy projectile on impact
        }
        // If it hits solid environment layout (like ground/walls) and it's not another trigger, destroy it
        else if (!hitInfo.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}