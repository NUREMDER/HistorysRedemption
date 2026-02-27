using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    [Header("Can Ayarlarý")]
    public int maxHealth = 100;
    private int currentHealth;
    public Image healthBarFill;

    [Header("Hedef Ayarlarý")]
    public Transform player;
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float stopDistance = 1.2f;

    [Header("Saldýrý Ayarlarý")]
    public float attackCooldown = 1.5f;
    private float lastAttackTime = 0f;

    [Header("Defans Ayarlarý")]
    public int blockProtectionDamage = 2;
    public float blockChance = 40f;
    public float blockDuration = 2.0f;

    [Header("Hitbox Ayarlarý")]
    public Transform highAttackPoint;
    public Transform midAttackPoint;
    public Transform lowAttackPoint;
    public float attackRange = 0.8f;
    public LayerMask playerLayer;

    [Header("VFX Ayarlarý")]
    public GameObject hitEffectPrefab;

    [Header("SFX Ayarlarý")]
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

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        currentHealth = maxHealth;

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRange)
        {
            FacePlayer();

            if (isBlocking)
            {
                StopMoving();
                return;
            }

            if (distanceToPlayer > stopDistance && !isAttacking)
            {
                MoveTowardsPlayer();
            }
            else
            {
                StopMoving();

                if (distanceToPlayer <= stopDistance && Time.time >= lastAttackTime + attackCooldown && !isAttacking)
                {
                    StartCoroutine(AttackRoutine());
                }
            }
        }
        else
        {
            StopMoving();
        }

        anim.SetFloat("VerticalSpeed", rb.velocity.y);
        anim.SetBool("IsBlocking", isBlocking);
    }

    void MoveTowardsPlayer()
    {
        float direction = player.position.x > transform.position.x ? 1 : -1;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        anim.SetFloat("Speed", 1);
    }

    void StopMoving()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        anim.SetFloat("Speed", 0);
    }

    void FacePlayer()
    {
        if (isAttacking || isBlocking) return;

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
        transform.Rotate(0f, 180f, 0f);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        StopMoving();

        if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound);

        int randomAttack = Random.Range(0, 3);

        anim.SetInteger("AttackType", randomAttack);
        anim.SetTrigger("AttackTrigger");

        lastAttackTime = Time.time;

        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
        anim.SetInteger("AttackType", 0);
    }

    IEnumerator BlockRoutine()
    {
        isBlocking = true;
        StopMoving();
        anim.SetBool("IsBlocking", true);

        yield return new WaitForSeconds(blockDuration);

        isBlocking = false;
        anim.SetBool("IsBlocking", false);
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

        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(selectedPoint.position, attackRange, playerLayer);

        bool hasHit = false;

        foreach (Collider2D p in hitPlayer)
        {
            PlayerController playerScript = p.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(10);
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
        if (isDead) return;

        int finalDamage = damage;

        if (isBlocking)
        {
            finalDamage = blockProtectionDamage;
            if (audioSource != null && blockSound != null) audioSource.PlayOneShot(blockSound);
        }
        else
        {
            anim.SetTrigger("Hurt");
            if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);

            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            if (!isAttacking && Random.Range(0, 100) < blockChance)
            {
                StartCoroutine(BlockRoutine());
            }
        }

        currentHealth -= finalDamage;

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }

        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Die");

        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;

        this.enabled = false;

        GameManager.instance.EnemyDefeated(50, 10);
    }

    IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        if (midAttackPoint != null) Gizmos.DrawWireSphere(midAttackPoint.position, attackRange);
    }

    IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}