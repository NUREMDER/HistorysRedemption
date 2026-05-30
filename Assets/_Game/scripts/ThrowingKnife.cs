using UnityEngine;

public class ThrowingKnife : MonoBehaviour
{
    [Header("Knife Settings")]
    public float speed = 15f;
    public int damage = 25;
    public float lifetime = 2f; 
    public bool isEnemyProjectile = false; // Checked if this knife was thrown by an enemy

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Ensure the collider is set to trigger so it doesn't bounce off things physically
        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.isTrigger = true;

        if (rb != null)
        {
            rb.gravityScale = 0f; // Disable gravity so the knife flies perfectly straight
            
            // Set velocity towards the right direction of the instantiated object
            rb.velocity = transform.right * speed;
        }

        // Flip the Y rotation 180 degrees to visually align the sprite direction
        transform.Rotate(0f, 180f, 0f);

        // Turn on trail effect if available
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.enabled = true;
        
        // Self-destruct after lifetime ends to preserve memory performance
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // CASE 1: If the knife belongs to an enemy
        if (isEnemyProjectile)
        {
            // Ignore collisions with the shooter itself or other enemy structures
            if (hitInfo.CompareTag("Enemy") || hitInfo.GetComponentInParent<EnemyAI>() != null || hitInfo.GetComponentInParent<TeslaAI>() != null) return;

            PlayerController player = hitInfo.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("Enemy projectile struck the player!");
                Destroy(gameObject);
            }
            else if (!hitInfo.isTrigger)
            {
                Debug.Log("Enemy projectile destroyed on impact with solid layout: " + hitInfo.name);
                Destroy(gameObject);
            }
        }
        // CASE 2: If the knife belongs to the player
        else
        {
            // Ignore collisions with the player character itself
            if (hitInfo.CompareTag("Player")) return;

            // Check if the knife hit a regular enemy or a Tesla boss enemy
            EnemyAI enemy = hitInfo.GetComponentInParent<EnemyAI>();
            TeslaAI teslaEnemy = hitInfo.GetComponentInParent<TeslaAI>();
            
            if (enemy != null || teslaEnemy != null)
            {
                // Calculate final damage including bought shop upgrades from GameManager
                int totalDamage = damage;
                if (GameManager.instance != null)
                {
                    totalDamage += GameManager.instance.bonusDamage;
                }
                
                if (enemy != null)
                {
                    enemy.TakeDamage(totalDamage);
                    Debug.Log("Knife hit normal enemy: " + enemy.gameObject.name);
                }
                else if (teslaEnemy != null)
                {
                    teslaEnemy.TakeDamage(totalDamage);
                    Debug.Log("Knife hit Tesla boss: " + teslaEnemy.gameObject.name);
                }

                Destroy(gameObject); // Destroy the knife on damage impact
            }
            else
            {
                // Destroy knife if it hits solid background layout (ground/walls) and it's not another trigger area
                if (!hitInfo.isTrigger)
                {
                    Debug.Log("Player knife destroyed on hitting solid layout: " + hitInfo.name);
                    Destroy(gameObject);
                }
            }
        }
    }
}