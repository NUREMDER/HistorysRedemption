using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float jumpDelay = 0.2f;

    [Header("Savaþ Ayarlarý")]
    public float attackRate = 0.4f;
    public float damageDelay = 0.4f; 
    private float nextAttackTime = 0f;

    [Header("Hitbox Ayarlarý")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    private Rigidbody2D rb;
    private Animator anim;

    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isCrouching = false;
    private bool isJumping = false;
    private float moveInput;

    public enum AttackDirection { Neutral, Up, Down, Forward, Backward }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
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

    void ProcessInputs()
    {
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

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && isGrounded && !isCrouching && !isJumping)
        {
            StartCoroutine(JumpRoutine());
        }

        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(PerformAttackRoutine("Punch"));
                nextAttackTime = Time.time + attackRate;
            }
            else if (Input.GetMouseButtonDown(1))
            {
                StartCoroutine(PerformAttackRoutine("Kick"));
                nextAttackTime = Time.time + attackRate;
            }
        }
    }

    void Move()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
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

    
    IEnumerator PerformAttackRoutine(string attackInputType)
    {
        
        AttackDirection dir = AttackDirection.Neutral;

        if (Input.GetKey(KeyCode.W)) dir = AttackDirection.Up;
        else if (Input.GetKey(KeyCode.S)) dir = AttackDirection.Down;
        else if (moveInput != 0)
        {
            if ((isFacingRight && moveInput > 0) || (!isFacingRight && moveInput < 0))
                dir = AttackDirection.Forward;
            else
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

        
        yield return new WaitForSeconds(damageDelay);

       
        if (attackPoint != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.GetComponent<EnemyController>() != null)
                {
                    enemy.GetComponent<EnemyController>().TakeDamage(20);
                }
            }
        }
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
    }

    void CheckFlip()
    {
        if (isFacingRight && moveInput < 0) Flip();
        else if (!isFacingRight && moveInput > 0) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}