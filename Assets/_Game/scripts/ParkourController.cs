using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(CapsuleCollider))]
public class ParkourController : MonoBehaviour
{
    [Header("Move")]
    public float runSpeed = 6f;
    public float jumpForce = 8f;
    public float slideSpeedMultiplier = 0.8f;
    public float speedRecoveryRate = 3f;
    public float parkourCooldown = 0.8f;
    public float obstacleIgnoreDuration = 1.5f;
    public float slideDuration = 0.7f;
    public float rollDuration = 0.7f;

    [Header("Health System")]
    public int maxHealth = 100;
    public int obstacleDamage = 10;
    public Image healthBarFill;
    private int currentHealth;

    [Header("JumpOver")]
    public float jumpDelay = 0.3f;
    public float jumpOverSpeedMultiplier = 0.6f;

    [Header("Stumble ")]
    public float stumbleDuration = 1.0f;
    public float stumbleSpeedMultiplier = 0.3f;

    private Rigidbody rb;
    private Animator anim;
    private CapsuleCollider col;

    private Vector3 moveDirection;
    private float currentSpeed;
    private float originalColHeight;
    private Vector3 originalColCenter;

    private bool isDead = false;
    private bool isGrounded = false;
    private bool isDoingParkour = false;
    private bool isSliding = false;

    private bool inSlideZone = false;
    private bool inThrowZone = false;
    private bool inJumpOverZone = false;
    private bool inJumping1Zone = false;

    private bool isIgnoringObstacles = false;
    private bool isStumbling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();

        // Configure physics and animator components for stable dynamic movement
        anim.applyRootMotion = false;
        rb.useGravity = true;
        rb.freezeRotation = true; // Prevent physics collisions from tilting the player
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth out camera jitter during movement

        // Cache the default collider dimensions to restore them after sliding
        originalColHeight = col.height;
        originalColCenter = col.center;

        moveDirection = transform.forward;
        currentSpeed = runSpeed;

        // Start health System
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    void Update()
    {
        if (isDead) return;

        // Run detection loops and check for dynamic inputs every frame
        CheckGround();
        HandleInput();

        // Smoothly accelerate back to base run speed after a slide or stumble penalty ends
        if (!isSliding && currentSpeed < runSpeed)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, runSpeed, speedRecoveryRate * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        // Apply forward velocity on the XZ plane while preserving gravity physics on the Y axis
        Vector3 horizontalVelocity = moveDirection * currentSpeed;
        rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
    }

    private void CheckGround()
    {
        // Calculate the absolute bottom point of the capsule collider to cast the ground check sphere
        Vector3 spherePos = col.bounds.center - new Vector3(0, col.bounds.extents.y - 0.1f, 0);
        isGrounded = Physics.CheckSphere(spherePos, 0.2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    private void HandleInput()
    {
        // Block new movement or action inputs if a parkour animation sequence is active
        if (isDoingParkour) return;

        // Context-aware jump inputs based on the active trigger zone or ground state
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (inJumpOverZone) StartCoroutine(DoParkourDelayed("doJumpOver", jumpForce, jumpOverSpeedMultiplier, jumpDelay, true));
            else if (inJumping1Zone) DoParkour("doJumping1", jumpForce, true);
            else if (inThrowZone) DoParkour("doThrow", 0f, true);
            else if (isGrounded) DoParkour("doRunningJump", jumpForce, false);
        }
        // Context-aware slide or roll inputs based on the current environment layout
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (inSlideZone) StartCoroutine(DoSlide());
            else if (isGrounded) StartCoroutine(DoRolling(false));
        }
    }
    private void ClearTriggers()
    {
        // Reset all animator triggers to prevent unintended animation stacking or late queuing bugs
        anim.ResetTrigger("doRunningJump");
        anim.ResetTrigger("doSlide");
        anim.ResetTrigger("doJumpOver");
        anim.ResetTrigger("doJumping1");
        anim.ResetTrigger("doThrow");
        anim.ResetTrigger("doStumble");
        anim.ResetTrigger("doRolling");
    }

    private void DoParkour(string animTrigger, float upwardForce, bool ignoreObstacles = true)
    {
        ClearTriggers();
        anim.SetTrigger(animTrigger);
        
        // Temporarily ignore the obstacle physics layer if specified for the action
        if (ignoreObstacles) StartCoroutine(IgnoreObstacleLayer(obstacleIgnoreDuration));

        // Apply instant upward physical impulse force if the action requires a jump height
        if (upwardForce > 0f)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Reset Y velocity first for a clean jump bounce
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
        }

        // Start the input lock cooldown window
        StartCoroutine(Cooldown(parkourCooldown));
    }

    private IEnumerator DoParkourDelayed(string animTrigger, float upwardForce, float speedMult, float delay, bool ignoreObstacles = true)
    {
        isDoingParkour = true;
        ClearTriggers();
        anim.SetTrigger(animTrigger);
        if (ignoreObstacles) StartCoroutine(IgnoreObstacleLayer(obstacleIgnoreDuration));

        // Temporarily adjust forward speed during the special parkour move wind-up phase
        currentSpeed = runSpeed * speedMult;

        // Wait for the specific visual delay frame before applying physical force (e.g., waiting to reach a rail)
        yield return new WaitForSeconds(delay);

        if (upwardForce > 0f)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
        }

        // Keep the movement input locked until the full recovery cooldown completes
        yield return new WaitForSeconds(parkourCooldown);
        isDoingParkour = false;
    }

    private IEnumerator DoSlide()
    {
        isDoingParkour = true;
        isSliding = true;

        ClearTriggers();
        anim.SetTrigger("doSlide");
        StartCoroutine(IgnoreObstacleLayer(obstacleIgnoreDuration));

        // Slightly decrease forward momentum while friction or slide state is active
        currentSpeed = runSpeed * slideSpeedMultiplier;

        // Shrink the capsule collider dimensions to let the 3D model pass cleanly beneath low ceiling boundaries
        col.height = originalColHeight * 0.5f;
        col.center = new Vector3(originalColCenter.x, originalColCenter.y - originalColHeight * 0.25f, originalColCenter.z);

        // Maintain the crouched/slid physics stance for the specified duration
        yield return new WaitForSeconds(slideDuration);

        // Snap the collider configuration safely back to default scale values after slide ends
        col.height = originalColHeight;
        col.center = originalColCenter;

        isSliding = false;
        isDoingParkour = false;
    }

    private IEnumerator DoRolling(bool ignoreObstacles = true)
    {
        isDoingParkour = true;
        isSliding = true;

        ClearTriggers();
        anim.SetTrigger("doRolling");
        if (ignoreObstacles) StartCoroutine(IgnoreObstacleLayer(obstacleIgnoreDuration));

        // Apply slide-like speed multiplier for consistency during the tactical roll phase
        currentSpeed = runSpeed * slideSpeedMultiplier;

        // Hold lock states for the total duration of the roll execution loop
        yield return new WaitForSeconds(rollDuration);

        isSliding = false;
        isDoingParkour = false;
    }    
    private IEnumerator DoStumble(Collider obstacleCollider)
    {
        // Prevent overlapping stumble routines from triggering simultaneously
        if (isStumbling) yield break;
        isStumbling = true;
        isDoingParkour = true;
        
        ClearTriggers();
        anim.SetTrigger("doStumble");

        // Ok (Arrow) engeline çarptıysak oku ve tüm child'larını sahnede tamamen yok et
        // Collider child objede (Square, Triangle) olabilir, parent'a doğru yukarı çıkarak Arrow'u bul
        if (obstacleCollider != null)
        {
            Transform current = obstacleCollider.transform;
            while (current != null)
            {
                if (current.name.Contains("Arrow"))
                {
                    Destroy(current.gameObject);
                    break;
                }
                current = current.parent;
            }
        }

        // Reduce player health pool upon hitting the obstacle
        TakeDamage(obstacleDamage);
        if (isDead) 
        {
            isStumbling = false;
            yield break; // Stop execution immediately if the hit was fatal
        }

        // Temporarily disable physics collisions between player and obstacles so the character passes through smoothly
        int playerLayer = gameObject.layer;
        int objectLayer = LayerMask.NameToLayer("ObstacleLayer");
        Physics.IgnoreLayerCollision(playerLayer, objectLayer, true);

        // Slow down the forward movement speed during the stumble penalty state
        currentSpeed = runSpeed * stumbleSpeedMultiplier;

        // Wait for the full duration of the stumble animation sequence to finish
        yield return new WaitForSeconds(stumbleDuration);

        // Restore the character's movement speed back to standard running velocity
        currentSpeed = runSpeed;

        // Re-enable physics layers collision matrix to normal state
        Physics.IgnoreLayerCollision(playerLayer, objectLayer, false);

        isDoingParkour = false;
        isStumbling = false;
    }   
    private IEnumerator IgnoreObstacleLayer(float duration)
    {
        isIgnoringObstacles = true;
        int playerLayer = gameObject.layer;
        int objectLayer = LayerMask.NameToLayer("ObstacleLayer");
        
        // Globally disable physics collision matrix between the player and obstacles during actions
        Physics.IgnoreLayerCollision(playerLayer, objectLayer, true);
        
        yield return new WaitForSeconds(duration);
        
        // Safely restore physical obstacle collisions after the specified duration window ends
        Physics.IgnoreLayerCollision(playerLayer, objectLayer, false);
        isIgnoringObstacles = false;
    }

    private IEnumerator Cooldown(float duration)
    {
        isDoingParkour = true;
        yield return new WaitForSeconds(duration);
        isDoingParkour = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // Trigger a stumble sequence if the player hits an obstacle layer without any active ignore/stumble flags
        if (other.gameObject.layer == LayerMask.NameToLayer("ObstacleLayer") && !isIgnoringObstacles && !isStumbling)
        {
            StartCoroutine(DoStumble(other));
        }

        string tag = other.tag;

        // Verify and cache entry states for different zone trigger regions (handles case sensitivity safely)
        if (tag == "SlideZone") inSlideZone = true;
        else if (tag == "ThrowZone") inThrowZone = true;
        else if (tag == "JumpOverZone") inJumpOverZone = true;
        else if (tag == "Jumping1Zone") inJumping1Zone = true;

        // Instant death if the player hits a critical obstacle without performing a parkour move
        if (tag == "Obstacle" && !isDoingParkour)
        {
            Die();
        }

        // End of parkour pathing - start the transition process to the boss fight arena
        if (tag == "ParkourEndZone")
        {
            TransitionToBossFight();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Fallback physical collision check for obstacles to guarantee a stumble sequence triggers correctly
        if (collision.gameObject.layer == LayerMask.NameToLayer("ObstacleLayer") && !isIgnoringObstacles && !isStumbling)
        {
            StartCoroutine(DoStumble(collision.collider));
        }
    }

    void OnTriggerExit(Collider other)
    {
        string tag = other.tag;

        // Reset region flags instantly when exiting the specific trigger zones
        if (tag == "SlideZone") inSlideZone = false;
        else if (tag == "ThrowZone") inThrowZone = false;
        else if (tag == "JumpOverZone") inJumpOverZone = false;
        else if (tag == "Jumping1Zone") inJumping1Zone = false;
    }

    private void Die()
    {
        isDead = true;
        rb.velocity = Vector3.zero; // Freeze physical velocity tracking instantly
        anim.SetTrigger("doDeath");
    }

    // ─── Health System ───
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Clamp health to zero to prevent negative integer display bugs
        UpdateHealthBar();

        // Floating damage number
        DamagePopup.Create(transform.position + Vector3.up * 1.5f, damage);

        Debug.Log("Shadow1 took damage! Remaining health: " + currentHealth + "/" + maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        // Update the health bar UI component fill ratio dynamically
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // ─── Freeze and Unfreeze for Dialogue Sequences ───
    public void PauseParkour()
    {
        currentSpeed = 0f;
        isDoingParkour = true; // Locks jump triggers and movement inputs during dialogue
        rb.velocity = Vector3.zero;
    }

    public void ResumeParkour()
    {
        isDoingParkour = false;
        currentSpeed = runSpeed;
    }

    // ─── Transition to Boss Fight Scene ───
    private void TransitionToBossFight()
    {
        isDead = true; // Freeze all player movements
        rb.velocity = Vector3.zero;

        // Save remaining health to persistent storage so the Player starts the Boss Fight with this exact health state
        if (GameManager.instance != null)
        {
            PlayerPrefs.SetInt("ParkourRemainingHealth", currentHealth);
            PlayerPrefs.SetInt("ParkourMaxHealth", maxHealth);
            
            GameManager.instance.SaveProgress();
           Debug.Log("Parkour is finished! Our remaining health is saved as " + currentHealth + ", EndZoneTrigger will handle the transition!");
        }

        // NOTE: SceneManager.LoadScene was removed from here to follow separation of concerns.
        // The actual scene shifting and character swapping will be executed by the "EndZoneTrigger" script on the trigger box!
    }

    private void OnDrawGizmosSelected()
    {
        if (col == null) col = GetComponent<CapsuleCollider>();
        if (col == null) return;

        // Calculate the bottom point of the capsule collider to accurately project the ground check sphere
        Vector3 spherePos = col.bounds.center - new Vector3(0, col.bounds.extents.y - 0.1f, 0);
        
        // Green means grounded, Red means airborne inside the Unity Editor scene view
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(spherePos, 0.2f);
    }
}