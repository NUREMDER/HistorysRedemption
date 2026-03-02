using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Can Ayarlarý")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI Ayarlarý")]
    public Image healthBarFill;

    [Header("Hareket Ayarlarý")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float jumpDelay = 0.2f;

    [Header("Savaþ Ayarlarý")]
    public int attackDamage = 20;
    public float attackRate = 0.4f;
    private float nextAttackTime = 0f;
    public int blockProtectionDamage = 2;

    [Header("Hitbox Ayarlarý")]
    public Transform highAttackPoint;
    public Transform midAttackPoint;
    public Transform lowAttackPoint;
    public float attackRange = 0.8f;
    public LayerMask enemyLayers;

    [Header("VFX Ayarlarý")]
    public GameObject hitEffectPrefab;

    [Header("SFX Ayarlarý")]
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
    private float moveInput;

    public enum AttackDirection { Neutral, Up, Down, Forward, Backward }

    void Start()
    {
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
            StartCoroutine(JumpRoutine());
        }

        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(PerformAttackRoutine());
                nextAttackTime = Time.time + attackRate;
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                StartCoroutine(PerformAttackRoutine());
                nextAttackTime = Time.time + attackRate;
            }
        }
    }

    void Move()
    {
        if (isBlocking || isAttacking)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }
    }

    IEnumerator JumpRoutine()
    {
        isJumping = true;
        anim.SetBool("IsGrounded", false);
        yield return new WaitForSeconds(jumpDelay);
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        yield return new WaitForSeconds(0.1f);
        isJumping = false;
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
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !isJumping)
        {
            isGrounded = true;
            anim.SetBool("IsGrounded", true);
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
        if (isBlocking || isAttacking) return;
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