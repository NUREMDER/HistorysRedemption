using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(CapsuleCollider))]
public class ParkourController : MonoBehaviour
{
    [Header("Hareket")]
    public float runSpeed = 6f;
    public float jumpForce = 8f;
    public float slideSpeedMultiplier = 0.8f;
    public float speedRecoveryRate = 3f;
    public float parkourCooldown = 0.8f;
    public float obstacleIgnoreDuration = 1.5f;
    public float slideDuration = 0.7f;
    public float rollDuration = 0.7f;

    [Header("Can Sistemi")]
    public int maxHealth = 100;
    public int obstacleDamage = 10;
    public Image healthBarFill;
    private int currentHealth;

    [Header("Chapter Geçişi")]
    public string bossFightSceneName = "Tutorial_Scene";

    [Header("Slide")]
    public float slideYOffset = 0.8f;

    [Header("BigJump & JumpOver")]
    public float jumpDelay = 0.3f;
    public float bigJumpDelay = 0.3f;
    public float bigJumpSpeedMultiplier = 0.4f;
    public float jumpOverSpeedMultiplier = 0.6f;

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
    private bool inBigJumpZone = false;
    private bool inJumpOverZone = false;
    private bool inJumping1Zone = false;
    private bool inJumpingDownZone = false;
    private bool inJumpingDown1Zone = false;
    private bool inRunningJumpZone = false;

    private bool isIgnoringObstacles = false;
    private bool isStumbling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();

        anim.applyRootMotion = false;
        rb.useGravity = true;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        originalColHeight = col.height;
        originalColCenter = col.center;

        moveDirection = transform.forward;
        currentSpeed = runSpeed;

        // Can sistemini başlat
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    void Update()
    {
        if (isDead) return;

        CheckGround();
        HandleInput();

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

        Vector3 horizontalVelocity = moveDirection * currentSpeed;
        rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);
    }

    private void CheckGround()
    {
        Vector3 spherePos = col.bounds.center - new Vector3(0, col.bounds.extents.y - 0.1f, 0);
        isGrounded = Physics.CheckSphere(spherePos, 0.2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    private void HandleInput()
    {
        if (isDoingParkour) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (inBigJumpZone) StartCoroutine(DoParkourDelayed("doBigJump", jumpForce * 1.5f, bigJumpSpeedMultiplier, bigJumpDelay, true));
            else if (inJumpOverZone) StartCoroutine(DoParkourDelayed("doJumpOver", jumpForce, jumpOverSpeedMultiplier, jumpDelay, true));
            else if (inRunningJumpZone) DoParkour("doRunningJump", jumpForce, true);
            else if (inJumping1Zone) DoParkour("doJumping1", jumpForce, true);
            else if (inThrowZone) DoParkour("doThrow", 0f, true);
            else if (isGrounded) DoParkour("doRunningJump", jumpForce, false);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (inSlideZone) StartCoroutine(DoSlide());
            else if (isGrounded) StartCoroutine(DoRolling(false));
            else if (inJumpingDownZone) DoParkour("doJumpingDown", 0f, true);
            else if (inJumpingDown1Zone) DoParkour("doJumpingDown1", 0f, true);
        }
    }

    private void ClearTriggers()
    {
        anim.ResetTrigger("doRunningJump");
        anim.ResetTrigger("doSlide");
        anim.ResetTrigger("doBigJump");
        anim.ResetTrigger("doJumpOver");
        anim.ResetTrigger("doJumping1");
        anim.ResetTrigger("doJumpingDown");
        anim.ResetTrigger("doJumpingDown1");
        anim.ResetTrigger("doThrow");
        anim.ResetTrigger("doStumble");
        anim.ResetTrigger("doRolling");
    }

    private void DoParkour(string animTrigger, float upwardForce, bool ignoreObstacles = true)
    {
        ClearTriggers();
        anim.SetTrigger(animTrigger);
        if (ignoreObstacles) StartCoroutine(IgnoreObstacleLayer(obstacleIgnoreDuration));

        if (upwardForce > 0f)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
        }

        StartCoroutine(Cooldown(parkourCooldown));
    }

    private IEnumerator DoParkourDelayed(string animTrigger, float upwardForce, float speedMult, float delay, bool ignoreObstacles = true)
    {
        isDoingParkour = true;
        ClearTriggers();
        anim.SetTrigger(animTrigger);
        if (ignoreObstacles) StartCoroutine(IgnoreObstacleLayer(obstacleIgnoreDuration));

        currentSpeed = runSpeed * speedMult;

        yield return new WaitForSeconds(delay);

        if (upwardForce > 0f)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
        }

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

        currentSpeed = runSpeed * slideSpeedMultiplier;

        col.height = originalColHeight * 0.5f;
        col.center = new Vector3(originalColCenter.x, originalColCenter.y - originalColHeight * 0.25f, originalColCenter.z);

        yield return new WaitForSeconds(slideDuration);

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

        currentSpeed = runSpeed * slideSpeedMultiplier;

        yield return new WaitForSeconds(rollDuration);

        isSliding = false;
        isDoingParkour = false;
    }

    private IEnumerator DoStumble(Collider obstacleCollider)
    {
        if (isStumbling) yield break;
        isStumbling = true;
        isDoingParkour = true;
        ClearTriggers();
        anim.SetTrigger("doStumble");

        // Engele çarpınca can azalt
        TakeDamage(obstacleDamage);
        if (isDead) 
        {
            isStumbling = false;
            yield break;
        }
        
        int playerLayer = gameObject.layer;
        int objectLayer = LayerMask.NameToLayer("ObstacleLayer");
        
        // Fiziği kapat ve sadece engellerle çarpışmayı iptal et (Vector hissiyatı için)
        rb.isKinematic = true;
        Physics.IgnoreLayerCollision(playerLayer, objectLayer, true);

        // Engelin çapını alıp üstünden atlanacak mesafeyi belirliyoruz
        float obstacleDepth = Mathf.Max(obstacleCollider.bounds.size.z, obstacleCollider.bounds.size.x);
        float jumpDistance = obstacleDepth + 0.8f;
        
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (moveDirection.normalized * jumpDistance);
        
        float jumpHeight = obstacleCollider.bounds.size.y + 0.2f; 
        
        float duration = 0.6f;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;

            float yOffset = Mathf.Sin(progress * Mathf.PI) * jumpHeight;

            Vector3 currentPos = Vector3.Lerp(startPos, new Vector3(endPos.x, startPos.y, endPos.z), progress);
            currentPos.y += yOffset;
            
            transform.position = currentPos;

            yield return null;
        }

        transform.position = new Vector3(endPos.x, startPos.y + 0.1f, endPos.z);

        rb.isKinematic = false;
        Physics.IgnoreLayerCollision(playerLayer, objectLayer, false);
        
        StartCoroutine(Cooldown(0.1f)); 
        isStumbling = false;
    }

    private IEnumerator IgnoreObstacleLayer(float duration)
    {
        isIgnoringObstacles = true;
        int playerLayer = gameObject.layer;
        int objectLayer = LayerMask.NameToLayer("ObstacleLayer");
        
        Physics.IgnoreLayerCollision(playerLayer, objectLayer, true);
        
        yield return new WaitForSeconds(duration);
        
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
        if (other.gameObject.layer == LayerMask.NameToLayer("ObstacleLayer") && !isIgnoringObstacles && !isStumbling)
        {
            StartCoroutine(DoStumble(other));
        }

        string tag = other.tag;

        if (tag == "SlideZone" || tag == "slideZone") inSlideZone = true;
        else if (tag == "ThrowZone" || tag == "throwZone") inThrowZone = true;
        else if (tag == "BigJumpZone" || tag == "bigJumpZone") inBigJumpZone = true;
        else if (tag == "JumpOverZone" || tag == "jumpOverZone") inJumpOverZone = true;
        else if (tag == "Jumping1Zone" || tag == "jumpingZone" || tag == "jumping1Zone") inJumping1Zone = true;
        else if (tag == "JumpingDownZone" || tag == "jumpingDownZone") inJumpingDownZone = true;
        else if (tag == "JumpingDown1Zone" || tag == "jumpingDown1Zone") inJumpingDown1Zone = true;
        else if (tag == "RunningJumpZone" || tag == "runningJumpZone") inRunningJumpZone = true;

        if (tag == "Obstacle" && !isDoingParkour)
        {
            Die();
        }

        // Parkur bitiş zone'u - BossFight sahnesine geçiş
        if (tag == "ParkourEndZone")
        {
            TransitionToBossFight();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("ObstacleLayer") && !isIgnoringObstacles && !isStumbling)
        {
            StartCoroutine(DoStumble(collision.collider));
        }
    }

    void OnTriggerExit(Collider other)
    {
        string tag = other.tag;

        if (tag == "SlideZone" || tag == "slideZone") inSlideZone = false;
        else if (tag == "ThrowZone" || tag == "throwZone") inThrowZone = false;
        else if (tag == "BigJumpZone" || tag == "bigJumpZone") inBigJumpZone = false;
        else if (tag == "JumpOverZone" || tag == "jumpOverZone") inJumpOverZone = false;
        else if (tag == "Jumping1Zone" || tag == "jumpingZone" || tag == "jumping1Zone") inJumping1Zone = false;
        else if (tag == "JumpingDownZone" || tag == "jumpingDownZone") inJumpingDownZone = false;
        else if (tag == "JumpingDown1Zone" || tag == "jumpingDown1Zone") inJumpingDown1Zone = false;
        else if (tag == "RunningJumpZone" || tag == "runningJumpZone") inRunningJumpZone = false;
    }

    private void Die()
    {
        isDead = true;
        rb.velocity = Vector3.zero;
        anim.SetTrigger("doDeath");
    }

    // ─── Can Sistemi ───
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateHealthBar();

        Debug.Log($"Shadow1 hasar aldı! Kalan can: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // ─── Diyalog İçin Dondurma ve Çözme ───
    public void PauseParkour()
    {
        currentSpeed = 0f;
        isDoingParkour = true; // Zıplamayı ve hareket inputunu kilitler
        rb.velocity = Vector3.zero;
    }

    public void ResumeParkour()
    {
        isDoingParkour = false;
        currentSpeed = runSpeed;
    }

    // ─── BossFight Sahnesine Geçiş ───
    private void TransitionToBossFight()
    {
        isDead = true; // Hareketi durdur
        rb.velocity = Vector3.zero;

        // Kalan canı GameManager'a kaydet (BossFight'ta Player bu canla başlasın)
        if (GameManager.instance != null)
        {
            // Parkurdan kalan canı geçici olarak saklıyoruz
            PlayerPrefs.SetInt("ParkourRemainingHealth", currentHealth);
            PlayerPrefs.SetInt("ParkourMaxHealth", maxHealth);
            PlayerPrefs.Save();
            Debug.Log($"Parkur bitti! Kalan can {currentHealth} ile BossFight'a geçiliyor...");
        }

        // BossFight sahnesine geçiş (Unity Inspector hatasını önlemek için doğrudan yazıldı)
        SceneManager.LoadScene("Tutorial_Scene");
    }

    private void OnDrawGizmosSelected()
    {
        if (col == null) col = GetComponent<CapsuleCollider>();
        if (col == null) return;

        Vector3 spherePos = col.bounds.center - new Vector3(0, col.bounds.extents.y - 0.1f, 0);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(spherePos, 0.2f);
    }
}