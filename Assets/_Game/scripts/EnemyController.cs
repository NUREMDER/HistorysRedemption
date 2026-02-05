using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int maxHealth = 100;
    int currentHealth;

    
    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        
        Debug.Log("Düþman hasar aldý! Kalan Can: " + currentHealth);

        
        StartCoroutine(FlashColor());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Düþman Öldü!");
        
        transform.Rotate(0, 0, 90);
        GetComponent<Collider2D>().enabled = false; 
        this.enabled = false; 
    }

    System.Collections.IEnumerator FlashColor()
    {
        sr.color = Color.white; 
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor; 
    }
}