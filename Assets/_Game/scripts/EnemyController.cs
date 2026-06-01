using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // Health variables for the enemy
    public int maxHealth = 100;
    int currentHealth;

    // References for handling visual damage feedback
    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        // Set initial health and get the SpriteRenderer component
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        
        // Save the original color of the enemy to reset it after flashing
        originalColor = sr.color;
    }

    public void TakeDamage(int damage)
    {
        // Decrease current health by damage amount
        currentHealth -= damage;

        // Log enemy hit and remaining health
        Debug.Log("EnemyController: Enemy hit! Remaining Health: " + currentHealth);

        // Start flash color effect
        StartCoroutine(FlashColor());

        // If health drops to zero or below, trigger death logic
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Log enemy death
        Debug.Log("EnemyController: Enemy dead!");
        
        // Rotate the body 90 degrees to lay down on the ground
        transform.Rotate(0, 0, 90);
        
        // Disable the collider so other objects don't bump into the corpse
        GetComponent<Collider2D>().enabled = false; 
        
        // Disable this script so the enemy stops acting
        this.enabled = false; 
    }

    System.Collections.IEnumerator FlashColor()
    {
        // Change color to white to show a hit flash effect
        sr.color = Color.white; 
        
        // Wait for 0.1 seconds
        yield return new WaitForSeconds(0.1f);
        
        // Restore the original color of the enemy
        sr.color = originalColor; 
    }
}