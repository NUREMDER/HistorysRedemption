using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;
    public Image healthBarFill;

    [Header("Target Settings")]
    public Transform player;
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float stopDistance = 1.2f;

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;
    private float lastAttackTime = 0f;

    [Header("Defense Settings")]
    public int blockProtectionDamage = 2;
    public float blockChance = 40f;
    public float blockDuration = 2.0f;
    public float hurtDuration = 3f;

    [Header("Hitbox Settings")]
    public Transform highAttackPoint;
    public Transform midAttackPoint;
    public Transform lowAttackPoint;
    public float attackRange = 0.8f;
    public LayerMask playerLayer;

    [Header("VFX Settings")]
    public GameObject hitEffectPrefab;

    [Header("SFX Settings")]
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip blockSound;

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private AudioSource audioSource;
    private bool isAttacking = false;
    private bool isBlocking = false;
    private bool isFacingRight = true;
    private bool isDead = false;
    private bool isHurt = false;
    private Coroutine hurtCoroutine;

    void Start()
    {
        hurtDuration = 3f; // Enforces 3 seconds regardless of Inspector value
        //Components references
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        //Equals current healht to healt number from inspector
        currentHealth = maxHealth;

        // Health bar is full at the beginning
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
        }
        //If there is no player, finds object with Player tag
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            // Gets the Transform component of the found player and assigns it to the player variable
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {   //If enemy is dead or there is no player quit function
        if (isDead || player == null)
        {
           return; 
        } 
        //If enemy is attacked stop move for stun animation
        if (isHurt)
        {
            StopMoving();
            return;
        }

        //Calculates distance between player and enemy and assigns it to variable
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        //If player is in detection range of enemy, enemy turn to players side
        if (distanceToPlayer < detectionRange)
        {
             FacePlayer();

            //Enemy can not move while blocking
            if (isBlocking){
                StopMoving();
                return;
            }
            //If distanceToPlayer greater than stopDistance and enemy does not attacking enemy walks toward player
            if (distanceToPlayer > stopDistance && !isAttacking)
            {
                MoveTowardsPlayer();
            }else
            {   //Player is not far stop for attacking
                StopMoving();

                // If the player is within range and the attack cooldown has expired, initiate the attack.
                if (distanceToPlayer <= stopDistance && Time.time >= lastAttackTime + attackCooldown && !isAttacking){
                    StartCoroutine(AttackRoutine());
                }
            }
        }else
        {
            //If player out of enemys detection range enemy stops
            StopMoving();
        }
        // Sends vertical velocity to animator to control falling/jumping animations
        anim.SetFloat("VerticalSpeed", rb.velocity.y);
        // Tells the animator whether the enemy is currently blocking
        anim.SetBool("IsBlocking", isBlocking);
    }

    void MoveTowardsPlayer()
    {
        float direction;

        if (player.position.x > transform.position.x)
        {   // Player is to the right, set direction to positive
            direction = 1f;
        }
        else
        {   // Player is to the left, set direction to negative
            direction = -1f;
        }
        // Apply horizontal movement while preserving vertical physics velocity
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        // Trigger the walking animation in the animator
        anim.SetFloat("Speed", 1);
    }

    void StopMoving()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        anim.SetFloat("Speed", 0);
    }

    void FacePlayer()
    {   //Do not change the facing direction while ishurt isAttacking isBlocking true
        if (isHurt || isAttacking || isBlocking)
        {
           return; 
        } 
        // If player is to the right and enemy is facing left, flip right
        if (player.position.x > transform.position.x && !isFacingRight){
            Flip();
        }
        // If player is to the left and enemy is facing right, flip left
        else if (player.position.x < transform.position.x && isFacingRight)
        {
            Flip();
        }
    }

    //Changes facing direction of enemy
    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    IEnumerator AttackRoutine()
    {   // Set attacking to true and stop any movement
        isAttacking = true;
        StopMoving();

        // Play the attack sound effect if references are assigned
        if (audioSource != null && attackSound != null)
        {
           audioSource.PlayOneShot(attackSound); 
        } 
        // Choose a random attack type between 0 and 2 (low,mid,high)
        int randomAttack = Random.Range(0, 3);

        // Pass the attack type to the animator and trigger the animation
        anim.SetInteger("AttackType", randomAttack);
        anim.SetTrigger("AttackTrigger");

        // Record the current timestamp to track attack cooldown
        lastAttackTime = Time.time;

        // Wait for 0.5 seconds for the attack animation to process
        yield return new WaitForSeconds(0.5f);

        // Reset attacking state and clear the attack type in the animator
        isAttacking = false;
        anim.SetInteger("AttackType", 0);
    }

    IEnumerator BlockRoutine()
    {   // Set attacking to true and stop any movement
        isBlocking = true;
        StopMoving();
        // Tells the animator whether the enemy is currently blocking
        anim.SetBool("IsBlocking", true);
        // Wait for block duration
        yield return new WaitForSeconds(blockDuration);

        //Reset blocking state and clear it in the animator
        isBlocking = false;
        anim.SetBool("IsBlocking", false);
    }

    public void TriggerAttackHit(int pointIndex)
    {   
        // Define a reference variable to hold the selected attack hitpoint
        Transform selectedPoint = null;

        // Determine which hitbox to use based on the animation event index
        switch (pointIndex)
        {
            case 0: selectedPoint = lowAttackPoint; break;
            case 1: selectedPoint = midAttackPoint; break;
            case 2: selectedPoint = highAttackPoint; break;
            default: selectedPoint = midAttackPoint; break;
        }

        // Safety check to ensure a valid attack point was selected
        if (selectedPoint == null)
        {
           return; 
        } 
        // Detect all colliders within the attack radius on the specified player layer
        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(selectedPoint.position, attackRange, playerLayer);

        bool hasHit = false;

        // Iterate through all detected colliders in the attack area
        foreach (Collider2D p in hitPlayer)
        {   
            // Check if the hit object has a PlayerController component
            PlayerController playerScript = p.GetComponent<PlayerController>();
            
            if (playerScript != null){
                // Apply damage to the player and set hit flag to true
                playerScript.TakeDamage(10);
                hasHit = true;
            }
        }
        // Trigger hit stop effect if the attack successfully connected
        if (hasHit)
        {
            StartCoroutine(HitStopRoutine(0.05f));//stop scene for hit effect
        }
    }

    public void TakeDamage(int damage)
    {   // If the enemy is already dead, ignore any incoming damage
        if (isDead)
        {
            return;
        } 

        int finalDamage = damage;

        // Check if the enemy is currently in a blocking state
        if (isBlocking)
        {  
             // Reduce damage to the blocked protection value and play the block sound
            finalDamage = blockProtectionDamage;
            if (audioSource != null && blockSound != null)
            {
               audioSource.PlayOneShot(blockSound); 
            } 
        }
        else
        {   // If not blocking, interrupt any active attack and trigger the hurt animation
            isAttacking = false;
            anim.SetTrigger("Hurt");
            
            // Restart the hurt stun coroutine to reset the stun duration safely
            if (hurtCoroutine != null)
            {
                StopCoroutine(hurtCoroutine);
            } 
            hurtCoroutine = StartCoroutine(HurtStunRoutine());
            // Play hit audio and instantiate the visual hit effect particle
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            } 

            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
        }
        // Deduct the calculated final damage from current health
        currentHealth -= finalDamage;

        // Update the UI health bar fill amount based on current health percentage
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
        // Trigger the red flash visual feedback on the sprite renderer
        StartCoroutine(FlashRed());

        // Check if health dropped to zero or below, then trigger die()
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {   // Mark the enemy as dead to halt any update logic
        isDead = true;
        // Stop all active timers and behaviors
        StopAllCoroutines();
        // Trigger the death animation in the animator
        anim.SetTrigger("Die");

        // Reset physical velocity and switch rigidbody to Kinematic so it doesn't fall or move
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Disable the collider so the player and other objects can pass through the body
        GetComponent<Collider2D>().enabled = false;

        // Disable this entire script component to stop running the Update method
        this.enabled = false;

        // Notify the GameManager to give awards
        GameManager.instance.EnemyDefeated(50, 100, 10);
    }

    IEnumerator HurtStunRoutine()
    {   // Stun the enemy and stop their movement
        isHurt = true;
        isAttacking = false;
        StopMoving();
        
        float timer = 0f;
        // Wait until the hurt duration ends
        while (timer < hurtDuration)
        {   // If the enemy dies during this time, stop the coroutine immediately
            if (isDead) yield break;

            StopMoving();
            timer += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        
        // Stun is over, reset the variables
        isHurt = false;
        hurtCoroutine = null;

        // If the enemy is alive, roll a chance to start blocking
        if (!isDead && !isAttacking && Random.Range(0, 100) < blockChance)
        {
            StartCoroutine(BlockRoutine());
        }
    }

    // Flashes the sprite red for a moment to show damage
    IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    // Does not run during the game, it is only for seeing the ranges in the Scene view
    void OnDrawGizmosSelected()
    {   
        // Draw a yellow circle for detection range in Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw a red circle for attack range in Scene view
        Gizmos.color = Color.red;
        if (midAttackPoint != null) Gizmos.DrawWireSphere(midAttackPoint.position, attackRange);
    }

    // Pauses the game shortly to create a nice hit effect
    IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}