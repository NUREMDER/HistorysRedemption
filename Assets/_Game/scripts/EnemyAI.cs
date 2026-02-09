using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Hedef Ayarlarý")]
    public Transform player;
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float attackRange = 1.5f;

    [Header("Saldýrý Ayarlarý")]
    public float attackCooldown = 1.5f;
    public int attackDamage = 10; 
    private float nextAttackTime = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRange)
        {
            FacePlayer();

            if (distanceToPlayer > attackRange)
            {
                MoveTowardsPlayer();
            }
            else
            {
                StopMoving();

                if (Time.time >= nextAttackTime)
                {
                    AttackPlayer();
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
        }
        else
        {
            StopMoving();
        }
    }

    void MoveTowardsPlayer()
    {
        Vector2 targetPosition = new Vector2(player.position.x, rb.position.y);
        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime);
        rb.MovePosition(newPos);
    }

    void StopMoving()
    {
        rb.velocity = Vector2.zero;
    }

    void FacePlayer()
    {
        if (player.position.x > transform.position.x)
        {
            sr.flipX = false;
        }
        else
        {
            sr.flipX = true;
        }
    }

    void AttackPlayer()
    {
        
        PlayerController playerScript = player.GetComponent<PlayerController>();
        if (playerScript != null)
        {
            playerScript.TakeDamage(attackDamage);
        }

        
        StartCoroutine(FlashAttackEffect());
    }

    System.Collections.IEnumerator FlashAttackEffect()
    {
        Color originalColor = sr.color;
        sr.color = Color.black;
        yield return new WaitForSeconds(0.2f);
        sr.color = originalColor;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}