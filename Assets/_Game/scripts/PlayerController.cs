using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Savaþ Ayarlarý")]
    public float attackRate = 0.4f;
    private float nextAttackTime = 0f;

    
    private Rigidbody2D rb;
    private Animator anim; 

    private bool isGrounded;
    private bool isFacingRight = true;
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
        moveInput = Input.GetAxisRaw("Horizontal");

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && isGrounded)
        {
            Jump();
        }

        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                PerformAttack("Punch"); 
                nextAttackTime = Time.time + attackRate;
            }
            else if (Input.GetMouseButtonDown(1))
            {
                PerformAttack("Kick");
                nextAttackTime = Time.time + attackRate;
            }
        }
    }

    void Move()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isGrounded = false;
        anim.SetBool("IsGrounded", false); 
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            anim.SetBool("IsGrounded", true); 
        }
    }

    
    void UpdateAnimations()
    {
        
        anim.SetFloat("Speed", Mathf.Abs(moveInput));

        
        anim.SetFloat("VerticalSpeed", rb.velocity.y);
    }

    void PerformAttack(string attackInputType)
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

        Debug.Log($"Saldýrý Tetiklendi: {dir} ({attackInputType}) - ID: {typeID}");
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
}