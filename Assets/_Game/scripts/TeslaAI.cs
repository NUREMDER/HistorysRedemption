using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TeslaAI : MonoBehaviour
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

    [Header("Animation Durations")]
    public float attackAnimationDuration = 1.0f; // Total duration of attack animation
    public float throwDelay = 0.3f; // The exact moment the throw happens
    public float throwAnimationDuration = 1.0f; // Total duration of the throwing animation



    [Header("Defence Settings")]
    public int blockProtectionDamage = 2;
    public float blockChance = 40f;
    public float blockDuration = 2.0f;
    public float hurtDuration = 3f;
    public float knockbackForce = 3f; // Knockback speed when taking damage
    public float knockbackDelay = 1.0f; // Delay before the knockback starts

    [Header("Hitbox Settings")]
    public Transform highAttackPoint;
    public Transform midAttackPoint;
    public Transform lowAttackPoint;
    public float attackRange = 0.8f;
    public LayerMask playerLayer;

    [Header("Ranged Attack Settings (Lightning)")]
    public GameObject lightningPrefab;
    public Transform throwPoint;

    [Header("VFX & SFX Settingsi")]
    public GameObject hitEffectPrefab;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip blockSound;

    [Header("3D Model Settings (FBX)")]
    public Animator modelAnimator; // Animator inside the FBX model will be assigned here
    public SkinnedMeshRenderer[] meshRenderers; // 3D meshes to flash red when taking damage

    [Header("Model Direction and Scale Settings")]
    public float modelRotationOffset = -90f;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private bool isAttacking = false;
    private bool isBlocking = false;
    private bool isFacingRight = true;
    private bool isDead = false;
    private bool isHurt = false;
    private Coroutine hurtCoroutine;
    private bool isFlashing = false;
    private Quaternion rightFacingRotation;
    private Quaternion leftFacingRotation;

    void Start()
    {
        hurtDuration = 3f;
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        // Automatically find the Animator component in children if not assigned
        if (modelAnimator == null)
            modelAnimator = GetComponentInChildren<Animator>();

        // Auto-attach the AnimationEvent receiver to the Animator's child GameObject
        // so that events embedded in FBX clips (e.g., 'Martelo 2') have a valid receiver.
        // Without this, Unity throws "no receiver" errors that corrupt the Animator state machine.
        if (modelAnimator != null && modelAnimator.GetComponent<TeslaAnimEventReceiver>() == null)
        {
            modelAnimator.gameObject.AddComponent<TeslaAnimEventReceiver>();
        }

        currentHealth = maxHealth;

        // Reset health bar UI to full on start
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
        }

        // Dynamically find the player transform via tag
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Calculate right and left rotation states on the Y axis using the model's orientation offset
        rightFacingRotation = Quaternion.Euler(0f, modelRotationOffset, 0f);
        leftFacingRotation = Quaternion.Euler(0f, modelRotationOffset + 180f, 0f);

        // Check player's initial X position relative to this enemy to determine the starting direction
        if (player != null)
        {
            isFacingRight = player.position.x > transform.position.x;
        }

        // Apply initial rotation ONLY to the 3D model child transform to prevent messing up 2D physics collider directions
        if (modelAnimator != null)
        {
            modelAnimator.transform.localRotation = isFacingRight ? rightFacingRotation : leftFacingRotation;
        }
    }

    void Update()
    {
        // Exit early if the enemy is already dead or the player reference is missing
        if (isDead || player == null) return;
        
        // If currently in the hurt state, just face the player and skip the rest of the AI logic
        if (isHurt)
        {
            FacePlayer(); 
            return;
        }

        // Calculate the current distance between this enemy and the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Check if the player is inside the detection perimeter
        if (distanceToPlayer < detectionRange)
        {
            // Always rotate to look at the player when alerted
            FacePlayer();

            // If the enemy is actively blocking, freeze movement and don't attack
            if (isBlocking)
            {
                StopMoving();
                return;
            }

            // Move closer if the player is still far away and the enemy isn't already attacking
            if (distanceToPlayer > stopDistance && !isAttacking)
            {
                MoveTowardsPlayer();
            }
            else
            {
                // Stop moving since the enemy is close enough to the player
                StopMoving();

                // Trigger attack routine if within range, cooldown has expired, and not already attacking
                if (distanceToPlayer <= stopDistance && Time.time >= lastAttackTime + attackCooldown && !isAttacking)
                {
                    StartCoroutine(AttackRoutine());
                }
            }
        }
        else
        {
            // Stop tracking and freeze movement if the player leaves the detection zone
            StopMoving();
        }

        // Keep animator parameters in sync with the current physics and behavior states
        if (modelAnimator != null)
        {
            // Use horizontal velocity magnitude to drive the locomotion animation blend tree
            modelAnimator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            modelAnimator.SetBool("IsBlocking", isBlocking);
        }
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
        
        // Note: Walking animation is automatically handled in Update() via Rigidbody velocity
    }

    void StopMoving()
    {
        // Stop horizontal movement while preserving vertical gravity physics
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    void FacePlayer()
    {
        // Check player's position to determine if the enemy needs to flip direction
        if (player.position.x > transform.position.x && !isFacingRight)
        {
            Flip();
        }
        else if (player.position.x < transform.position.x && isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        
        // Rotate only the 3D model child transform to keep 2D physics intact
        if (modelAnimator != null)
        {
            modelAnimator.transform.localRotation = isFacingRight ? rightFacingRotation : leftFacingRotation;
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        StopMoving();

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        } 

        // Randomly choose an attack type: 0 = punch, 1 = kick, 2 = lightning projectile
        int randomAttack = Random.Range(0, 3); 

        if (modelAnimator != null)
        {
            if (randomAttack == 2)
            {
                // Trigger the ready-to-use throw animation inside the 3D model's animator
                modelAnimator.SetTrigger("ThrowObject");
            }
            else
            {
                // Set the specific melee attack type (0 or 1) and trigger the attack animation
                modelAnimator.SetInteger("AttackType", randomAttack);
                modelAnimator.SetTrigger("AttackTrigger");
            }
        }

        // Track the timestamp of the last executed attack for cooldown calculations
        lastAttackTime = Time.time;

        // Handle hit/projectile timing via code delay without relying on Animation Events
        if (randomAttack == 2)
        {
            // Start the throwing sequence for the lightning projectile
            StartCoroutine(PerformThrow());
        }
        else
        {
            // Start the melee hit sequence with a minor code delay (e.g., 0.5 seconds)
            StartCoroutine(PerformHit(randomAttack));
        }

        // Wait for the full duration of the attack animation to finish playing
        yield return new WaitForSeconds(attackAnimationDuration);

        // Reset attack states and animation integers back to default
        isAttacking = false;
        if (modelAnimator != null)
        {
            modelAnimator.SetInteger("AttackType", 0);
        }
    }

    IEnumerator BlockRoutine()
    {
        isBlocking = true;
        StopMoving();
        
        // Trigger the blocking stance animation loop
        if (modelAnimator != null) modelAnimator.SetBool("IsBlocking", true);

        // Keep the shield/block active for the specified duration
        yield return new WaitForSeconds(blockDuration);

        // Reset blocking states and turn off animation loop
        isBlocking = false;
        if (modelAnimator != null) modelAnimator.SetBool("IsBlocking", false);
    }

    IEnumerator PerformHit(int pointIndex)
    {
        // Wait 0.5 seconds for the physical animation swing to reach its peak impact frame
        yield return new WaitForSeconds(0.5f);

        // If the enemy got stunned (hurt) or died during the delay, cancel the hit execution entirely
        if (isDead || isHurt) yield break; 

        // Decide which spatial point to use depending on the chosen random attack type
        Transform selectedPoint = null;
        switch (pointIndex)
        {
            case 0: selectedPoint = lowAttackPoint; break;
            case 1: selectedPoint = midAttackPoint; break;
            case 2: selectedPoint = highAttackPoint; break;
            default: selectedPoint = midAttackPoint; break;
        }

        if (selectedPoint == null) yield break;

        // Calculate horizontal direction multiplier (1 for right, -1 for left)
        float direction = isFacingRight ? 1f : -1f;
        
        // Calculate the relative X offset distance between the enemy core and the target point
        float offsetX = Mathf.Abs(selectedPoint.position.x - transform.position.x);
        if (offsetX < 0.1f) offsetX = 1.0f; // Safe fallback to guarantee hit projection range

        // Build the dynamic 2D overlap check circle position in front of the enemy
        Vector2 hitPosition = new Vector2(transform.position.x + (direction * offsetX), selectedPoint.position.y);

        // Cast a 2D collision sphere to detect all colliders on the player layer
        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(hitPosition, attackRange, playerLayer);
        bool hasHit = false;

        // Loop through everything caught inside the hit radius and apply damage
        foreach (Collider2D p in hitPlayer)
        {
            PlayerController playerScript = p.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(10); // Standard attack damage value
                hasHit = true;
            }
        }

        // Trigger a tiny camera freeze effect or hitstop to enhance game feel on successful impact
        if (hasHit)
        {
            StartCoroutine(HitStopRoutine(0.05f));
        }
    }

    IEnumerator PerformThrow()
    {
        // Wait for the specific wind-up animation delay before spawning the strike
        yield return new WaitForSeconds(throwDelay);

        // Cancel casting if the boss is interrupted or killed during wind-up
        if (isDead || isHurt) yield break;

        if (lightningPrefab != null && player != null)
        {
            // DESIGN CHOICE: ThrowPoint is disabled. 
            // We lock onto the player's exact X coordinate and spawn the bolt 10 units straight up in the sky.
            Vector2 spawnPos = new Vector2(player.position.x, player.position.y + 10f);
            
            // Rotate the prefab -90 degrees on the Z axis to force the lightning sprite to drop straight downwards
            Quaternion spawnRot = Quaternion.Euler(0, 0, -90);
            
            // Instantiate the lightning strike object into the active scene
            Instantiate(lightningPrefab, spawnPos, spawnRot);
        }
    }
    /// <summary>
    /// Called by Animation Events embedded in attack animation clips (e.g., 'Martelo 2').
    /// TeslaAI handles hit detection via code-driven coroutines (PerformHit),
    /// so this method exists solely to prevent "no receiver" errors from breaking
    /// the Animator state machine transitions.
    /// </summary>
    public void TriggerAttackHit(int pointIndex)
    {
        // Hit detection is already handled by PerformHit coroutine.
        // This empty receiver prevents AnimationEvent errors that disrupt the Animator.
    }

    public void TakeDamage(int damage)
    {
        // Exit early if the enemy is already dead
        if (isDead) return;

        int finalDamage = damage;

        // CASE 1: Enemy is blocking the attack
        if (isBlocking)
        {
            // Reduce incoming damage to the fixed blocked protection value
            finalDamage = blockProtectionDamage;
            if (audioSource != null && blockSound != null) audioSource.PlayOneShot(blockSound);
        }
        // CASE 2: Enemy is unprotected and takes a direct hit
        else
        {
            // Cancel any ongoing attack sequences since the enemy got interrupted
            isAttacking = false;
            if (modelAnimator != null) modelAnimator.SetTrigger("Hurt");
            
            // Calculate knockback direction away from the player's current position
            float knockbackDir = player.position.x > transform.position.x ? -1f : 1f;

            // Stop any existing hurt coroutine to prevent hit-stun stacking bugs
            if (hurtCoroutine != null) StopCoroutine(hurtCoroutine);
            hurtCoroutine = StartCoroutine(HurtStunRoutine(knockbackDir));
            
            if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);

            // Spawn hit particle effect at the enemy's current position
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
        }

        // Apply final calculated damage to health pool
        currentHealth -= finalDamage;

        // Update the health bar UI fill ratio accurately
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }

        // Trigger the material color flash effect to visually indicate damage
        StartCoroutine(FlashRed());

        // Check for death condition
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        
        // Stop all running behaviors, attacks, or stun routines immediately
        StopAllCoroutines();
        if (modelAnimator != null) modelAnimator.SetTrigger("Die");

        // Completely disable physics to prevent the corpse from being pushed or bugging out layout boundaries
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // Prevent gravity and dynamic physical forces
        GetComponent<Collider2D>().enabled = false; // Stop player from colliding with the dead body

        // Start the fading or scene cleanup sequence for the corpse
        StartCoroutine(DieRoutine());
    }
    IEnumerator DieRoutine()
    {
        // Smoothly sink the 3D model into the ground so it doesn't look like it's floating after death
        float dropDuration = 1.0f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - new Vector3(0f, 1.8f, 0f); // Pull the position down by 1.8 units

        while (elapsed < dropDuration)
        {
            if (this != null) 
            {
                // Linearly interpolate the position over the drop duration
                transform.position = Vector3.Lerp(startPos, endPos, elapsed / dropDuration);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Wait a short bit before disabling the script component completely
        yield return new WaitForSeconds(1.5f);

        this.enabled = false;

        // Reward the player via GameManager with gold, XP, and reputation points
        if (GameManager.instance != null)
        {
            GameManager.instance.EnemyDefeated(100, 200, 20);
        }
    }

    IEnumerator HurtStunRoutine(float knockbackDir)
    {
        isHurt = true;
        isAttacking = false;
        
        float timer = 0f;
        bool knocked = false;

        while (timer < hurtDuration)
        {
            // Break early if the enemy dies while in the middle of being stunned
            if (isDead) yield break;
            
            // Apply the sharp knockback impulse force after the specified delay tracking window
            if (timer >= knockbackDelay && !knocked)
            {
                rb.velocity = new Vector2(knockbackDir * knockbackForce, rb.velocity.y);
                knocked = true;
            }

            // After the initial impulse, apply simulated physics friction to smoothly bring the enemy to a halt
            if (knocked)
            {
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, Time.deltaTime * 5f), rb.velocity.y);
            }

            timer += Time.deltaTime;
            yield return null;
        }
        
        isHurt = false;
        hurtCoroutine = null;

        // AI Decision: Roll a random chance to immediately enter a blocking state as a defensive recovery fallback
        if (!isDead && !isAttacking && Random.Range(0, 100) < blockChance)
        {
            StartCoroutine(BlockRoutine());
        }
    }

    IEnumerator FlashRed()
    {
        // Prevent overlapping flash routines from overriding each other
        if (isFlashing) yield break; 
        if (meshRenderers == null || meshRenderers.Length == 0) yield break;

        isFlashing = true;

        // Use MaterialPropertyBlock to modify renderer properties without duplicating the master material in memory
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        propBlock.SetColor("_Color", Color.red);

        // Apply the red tint override to all child mesh renderers instantly
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null)
                meshRenderers[i].SetPropertyBlock(propBlock);
        }

        // Hold the red indicator visual state for a tenth of a second
        yield return new WaitForSeconds(0.1f);

        // Clearing the property block automatically restores the default native material colors safely
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null)
                meshRenderers[i].SetPropertyBlock(null);
        }

        isFlashing = false;
    }

    void OnDrawGizmosSelected()
    {
        // Draw the yellow detection circle boundary in the Unity Editor Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw the red melee hit radius indicator anchored to the Mid Attack Point transform
        Gizmos.color = Color.red;
        if (midAttackPoint != null)
        {
            // Determine facing math so gizmos display properly both during runtime and inside the static editor layout
            float direction = Application.isPlaying ? (isFacingRight ? 1f : -1f) : 1f;
            float offsetX = Mathf.Abs(midAttackPoint.position.x - transform.position.x);
            if (offsetX < 0.1f) offsetX = 1.0f;
            
            Vector2 hitPos = new Vector2(transform.position.x + (direction * offsetX), midAttackPoint.position.y);
            Gizmos.DrawWireSphere(hitPos, attackRange);
        }
    }

    IEnumerator HitStopRoutine(float duration)
    {
        // Freeze global time execution to add immense impact feedback weight ("juice") on clean hits
        Time.timeScale = 0f;
        
        // Use Realtime waiting since standard DeltaTime calculations are frozen at 0
        yield return new WaitForSecondsRealtime(duration);
        
        // Restore normal time scale flow after the hitstop frame ends
        Time.timeScale = 1f;
    }
}