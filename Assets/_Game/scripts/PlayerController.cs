using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Can Ayarlar�")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI Ayarlar�")]
    public Image healthBarFill;

    [Header("Hareket Ayarlar")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float jumpDelay = 0.2f;

    [Header("Sava Ayarlar")]
    public int attackDamage = 20;
    public float attackRate = 0.4f;
    private float nextAttackTime = 0f;
    public float throwRate = 0.5f;
    private float nextThrowTime = 0f;
    public int blockProtectionDamage = 2;
    public float hurtDuration = 3f;

    [Header("Bıçak Fırlatma Ayarları")]
    public GameObject[] knifePrefabs; // 3 tip bıçak prefab'i buraya atanacak
    public Transform throwPoint;
    public float throwSpawnDelay = 0.3f;

    [Header("Hitbox Ayarlar")]
    public Transform highAttackPoint;
    public Transform midAttackPoint;
    public Transform lowAttackPoint;
    public float attackRange = 0.8f;
    public LayerMask enemyLayers;

    [Header("VFX Ayarlar�")]
    public GameObject hitEffectPrefab;

    [Header("SFX Ayarlar�")]
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip blockSound;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private AudioSource audioSource;

    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isCrouching = false;
    private bool isJumping = false;
    private bool isBlocking = false;
    private bool isAttacking = false;
    private bool isHurt = false;
    private Coroutine hurtCoroutine;
    private Coroutine jumpCoroutine;
    private Coroutine attackCoroutine;
    private float moveInput;

    public enum AttackDirection { Neutral, Up, Down, Forward, Backward }

    void Start()
    {
        hurtDuration = 3f; // Enforces 3 seconds regardless of Inspector value
        
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (GameManager.instance != null)
        {
            maxHealth += GameManager.instance.bonusMaxHealth;
        }

        currentHealth = maxHealth;

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
        }
    }

    void Update()
    {
        ProcessInputs();
        CheckFlip();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        Move();
    }

    public void TriggerAttackHit(int pointIndex)
    {
        Transform selectedPoint = null;

        switch (pointIndex)
        {
            case 0: selectedPoint = lowAttackPoint; break;
            case 1: selectedPoint = midAttackPoint; break;
            case 2: selectedPoint = highAttackPoint; break;
            default: selectedPoint = midAttackPoint; break;
        }

        if (selectedPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(selectedPoint.position, attackRange, enemyLayers);

        bool hasHit = false;

        int totalDamage = attackDamage;
        if (GameManager.instance != null)
        {
            totalDamage += GameManager.instance.bonusDamage;
        }

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(totalDamage);
                hasHit = true;
            }
        }

        if (hasHit)
        {
            StartCoroutine(HitStopRoutine(0.05f));
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        int finalDamage = damage;

        if (isBlocking)
        {
            finalDamage = blockProtectionDamage;
            if (audioSource != null && blockSound != null) audioSource.PlayOneShot(blockSound);
        }
        else
        {
            if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);
        }

        currentHealth -= finalDamage;

        if (currentHealth > 0 && currentHealth <= (maxHealth * 0.2f))
        {
            GameManager.instance.ShowFleeOption();
        }

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }

        if (currentHealth <= 0)
        {
            if (healthBarFill != null) healthBarFill.fillAmount = 0;
            Die();
        }
        else
        {
            if (!isBlocking)
            {
                isAttacking = false;
                anim.SetTrigger("Hurt");
                
                if (hurtCoroutine != null) StopCoroutine(hurtCoroutine);
                hurtCoroutine = StartCoroutine(HurtStunRoutine());

                if (jumpCoroutine != null) { StopCoroutine(jumpCoroutine); jumpCoroutine = null; isJumping = false; }
                if (attackCoroutine != null) { StopCoroutine(attackCoroutine); attackCoroutine = null; isAttacking = false; }

                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                }

                StartCoroutine(FlashColor());
            }
        }
    }

    void Die()
    {
        StopAllCoroutines();
        anim.SetTrigger("Die");
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        GameManager.instance.PlayerDefeated();
    }

    IEnumerator FlashColor()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    void ProcessInputs()
    {
        if (Input.GetMouseButton(1) && isGrounded && !isJumping && !isAttacking)
        {
            isBlocking = true;
            moveInput = 0;
        }
        else
        {
            isBlocking = false;
        }

        if (isHurt)
        {
            moveInput = 0;
            return;
        }

        if (isBlocking || isAttacking)
        {
            moveInput = 0;
            return;
        }

        if (isGrounded && (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)))
        {
            isCrouching = true;
            moveInput = 0;
        }
        else
        {
            isCrouching = false;
            moveInput = Input.GetAxisRaw("Horizontal");
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isCrouching && !isJumping)
        {
            if (jumpCoroutine != null) StopCoroutine(jumpCoroutine);
            jumpCoroutine = StartCoroutine(JumpRoutine());
        }

        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                attackCoroutine = StartCoroutine(PerformAttackRoutine());
                nextAttackTime = Time.time + attackRate;
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                attackCoroutine = StartCoroutine(PerformAttackRoutine());
                nextAttackTime = Time.time + attackRate;
            }
        }

        if (Time.time >= nextThrowTime)
        {
            if (Input.GetKeyDown(KeyCode.Q) && isGrounded && !isAttacking && !isBlocking && !isCrouching && !isJumping)
            {
                if (GameManager.instance != null && GameManager.instance.playerKnives > 0 && GameManager.instance.unlockedKnifeLevel > 0)
                {
                    if (attackCoroutine != null) StopCoroutine(attackCoroutine);
                    attackCoroutine = StartCoroutine(PerformThrowRoutine());
                    nextThrowTime = Time.time + throwRate;
                }
            }
        }
    }

    void Move()
    {
        if (isHurt || isBlocking || isAttacking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }
    }

    IEnumerator HurtStunRoutine()
    {
        isHurt = true;
        isAttacking = false;
        isJumping = false;
        moveInput = 0;
        rb.velocity = new Vector2(0, rb.velocity.y);
        
        float timer = 0f;
        while (timer < hurtDuration)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            timer += Time.deltaTime;
            yield return null;
        }
        
        isHurt = false;
        hurtCoroutine = null;
    }

    IEnumerator JumpRoutine()
    {
        isJumping = true;
        anim.SetBool("IsGrounded", false);
        yield return new WaitForSeconds(jumpDelay);
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        yield return new WaitForSeconds(0.1f);
        isJumping = false;
        jumpCoroutine = null;
    }

    IEnumerator PerformThrowRoutine()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        if (GameManager.instance != null)
        {
            GameManager.instance.playerKnives--;
            GameManager.instance.SaveProgress();
        }

        anim.SetTrigger("doThrowKnife");

        yield return new WaitForSeconds(throwSpawnDelay);

        InstantiateKnife();

        float remainingTime = throwRate - throwSpawnDelay;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        isAttacking = false;
        attackCoroutine = null;
    }

    public void InstantiateKnife()
    {
        if (GameManager.instance == null) { Debug.LogError("GameManager.instance is null!"); return; }
        if (throwPoint == null) { Debug.LogError("throwPoint is NULL! Lütfen Inspector'dan Throw Point atayın."); return; }
        if (knifePrefabs == null || knifePrefabs.Length == 0) { Debug.LogError("knifePrefabs dizisi BOŞ! Lütfen Inspector'dan Bıçak Prefab'larını atayın."); return; }

        int level = GameManager.instance.unlockedKnifeLevel;
        if (level > 0 && level <= knifePrefabs.Length)
        {
            GameObject selectedKnife = knifePrefabs[level - 1]; 
            if (selectedKnife != null)
            {
                GameObject spawned = Instantiate(selectedKnife, throwPoint.position, transform.rotation);
                Debug.Log("Bıçak başarıyla fırlatıldı! Yönü: " + transform.right);
            }
            else
            {
                Debug.LogError("Knife Prefabs dizisindeki " + (level - 1) + ". eleman NULL! Lütfen prefab atayın.");
            }
        }
        else
        {
            Debug.LogError("Geçersiz bıçak seviyesi: " + level + ". Lütfen marketten yükseltme yapıldığından veya GameManager'daki dizi boyutuyla uyuştuğundan emin olun.");
        }
    }

    IEnumerator PerformAttackRoutine()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound);

        AttackDirection dir = AttackDirection.Neutral;

        if (Input.GetKey(KeyCode.W)) dir = AttackDirection.Up;
        else if (Input.GetKey(KeyCode.S)) dir = AttackDirection.Down;
        else if (moveInput != 0)
        {
            if ((isFacingRight && Input.GetAxisRaw("Horizontal") > 0) || (!isFacingRight && Input.GetAxisRaw("Horizontal") < 0))
                dir = AttackDirection.Forward;
            else if (Input.GetAxisRaw("Horizontal") != 0)
                dir = AttackDirection.Backward;
        }

        int typeID = 0;

        switch (dir)
        {
            case AttackDirection.Neutral: typeID = 0; break;
            case AttackDirection.Up: typeID = 1; break;
            case AttackDirection.Down: typeID = 2; break;
            case AttackDirection.Forward: typeID = 3; break;
            case AttackDirection.Backward: typeID = 4; break;
        }

        anim.SetInteger("AttackType", typeID);
        anim.SetTrigger("AttackTrigger");

        yield return new WaitForSeconds(attackRate);

        isAttacking = false;
        anim.SetInteger("AttackType", 0);
        attackCoroutine = null;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !isJumping)
        {
            isGrounded = true;
            if (anim != null) 
            {
                anim.SetBool("IsGrounded", true);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            if (anim != null)
            {
                anim.SetBool("IsGrounded", false);
            }
        }
    }

    void UpdateAnimations()
    {
        anim.SetFloat("Speed", Mathf.Abs(moveInput));
        anim.SetFloat("VerticalSpeed", rb.velocity.y);
        anim.SetBool("IsCrouching", isCrouching);
        anim.SetBool("IsBlocking", isBlocking);
    }

    void CheckFlip()
    {
        if (isHurt || isBlocking || isAttacking) return;
        float inputX = Input.GetAxisRaw("Horizontal");

        if (isFacingRight && inputX < 0) Flip();
        else if (!isFacingRight && inputX > 0) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (midAttackPoint != null) Gizmos.DrawWireSphere(midAttackPoint.position, attackRange);
        Gizmos.color = Color.yellow;
        if (highAttackPoint != null) Gizmos.DrawWireSphere(highAttackPoint.position, attackRange);
        Gizmos.color = Color.blue;
        if (lowAttackPoint != null) Gizmos.DrawWireSphere(lowAttackPoint.position, attackRange);
    }

    IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}