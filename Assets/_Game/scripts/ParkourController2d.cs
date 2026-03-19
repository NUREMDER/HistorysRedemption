using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class ParkourController2d : MonoBehaviour
{
    [Header("Hareket")]
    public float runSpeed = 8f;
    public float jumpForce = 15f; // Gücü biraz artırdık
    public float slideSpeedMultiplier = 1.2f;
    public float speedRecoveryRate = 3f;
    public float parkourCooldown = 0.5f;
    public float slideDuration = 0.7f;

    [Header("Slide")]
    public float slideYOffset = 0.4f;

    private Rigidbody2D rb;
    private Animator anim;
    private CapsuleCollider2D col;

    private float currentSpeed;
    private float originalColHeight;
    private Vector2 originalColCenter;

    private bool isDead = false;
    private bool isGrounded = false;
    private bool isDoingParkour = false;
    private bool isSliding = false;

    // Bölgeler
    private bool inSlideZone = false;
    private bool inBigJumpZone = false;
    private bool inJumpOverZone = false;
    private bool inRunningJumpZone = false;

    public bool isParkourActive = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider2D>();

        rb.gravityScale = 3f; // Daha tok bir düşüş için artırdık
        rb.freezeRotation = true;

        originalColHeight = col.size.y;
        originalColCenter = col.offset;
        currentSpeed = runSpeed;
    }

    void Update()
    {
        if (isDead || !isParkourActive) return;

        CheckGround();
        HandleInput();

        anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        anim.SetBool("IsGrounded", isGrounded);

        if (!isSliding && currentSpeed < runSpeed)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, runSpeed, speedRecoveryRate * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (!isParkourActive || isDead) 
        {
            if(!isDead) rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }
        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);
    }

    private void CheckGround()
    {
        // En basit zıplama kontrolü: Karakterin ayak ucundan aşağı bir ışın atar
        float extraHeight = 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(col.bounds.center, Vector2.down, col.bounds.extents.y + extraHeight);
        
        // Eğer yere çok yakınsa isGrounded true olur
        isGrounded = hit.collider != null;
        
        // Debug için Scene ekranında yeşil/kırmızı çizgi çizer
        Color rayColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(col.bounds.center, Vector2.down * (col.bounds.extents.y + extraHeight), rayColor);
    }

    private void HandleInput()
    {
        if (isDoingParkour) return;

        // ZIPLAMA (W veya Yukarı)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (inBigJumpZone) StartCoroutine(DoParkourDelayed("doBigJump", jumpForce * 1.4f, 0.4f, 0.2f));
            else if (inJumpOverZone) StartCoroutine(DoParkourDelayed("doJumpOver", jumpForce * 0.8f, 0.6f, 0.1f));
            else if (isGrounded || inRunningJumpZone) DoParkour("doRunningJump", jumpForce);
        }
        // KAYMA (S veya Aşağı)
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            // isGrounded olmasa bile SlideZone içindeyse kaysın (Hata payını siler)
            if (inSlideZone || isGrounded) 
            {
                StartCoroutine(DoSlide());
            }
        }
    }

    private void DoParkour(string animTrigger, float upwardForce)
{
    isDoingParkour = true;
    anim.SetTrigger(animTrigger); // Animator'da bu isimde trigger olduğundan emin ol

    // Zıplatma garantisi
    rb.velocity = new Vector2(rb.velocity.x, upwardForce); 
    
    StartCoroutine(Cooldown(parkourCooldown));
}

    private IEnumerator DoParkourDelayed(string animTrigger, float upwardForce, float speedMult, float delay)
    {
        isDoingParkour = true;
        anim.SetTrigger(animTrigger);
        currentSpeed = runSpeed * speedMult;
        yield return new WaitForSeconds(delay);

        if (upwardForce > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * upwardForce, ForceMode2D.Impulse);
        }
        yield return new WaitForSeconds(parkourCooldown);
        isDoingParkour = false;
    }

    private IEnumerator DoSlide()
{
    isDoingParkour = true;
    isSliding = true;
    
    // Animator'da hata almamak için kontrol ekledik
    anim.SetTrigger("doSlide");

    currentSpeed = runSpeed * slideSpeedMultiplier;

    // Sadece collider'ı küçült, karakteri yerin dibine çekme (Glitch'i bu önler)
    col.size = new Vector2(col.size.x, originalColHeight * 0.3f);
    col.offset = new Vector2(originalColCenter.x, originalColCenter.y - (originalColHeight * 0.35f));

    yield return new WaitForSeconds(slideDuration);

    col.size = new Vector2(col.size.x, originalColHeight);
    col.offset = originalColCenter;

    isSliding = false;
    isDoingParkour = false;
}

    private IEnumerator Cooldown(float duration)
    {
        yield return new WaitForSeconds(duration);
        isDoingParkour = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {   Debug.Log("Bir şeye çarptım! Çarptığım objenin adı: " + other.gameObject.name + " Tag'i: " + other.tag);
        if (other.CompareTag("SlideZone")) inSlideZone = true;
        if (other.CompareTag("BigJumpZone")) inBigJumpZone = true;
        if (other.CompareTag("JumpOverZone")) inJumpOverZone = true;
        if (other.CompareTag("RunningJumpZone")) inRunningJumpZone = true;
        if (other.CompareTag("Obstacle") && !isDoingParkour) Die();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("SlideZone")) inSlideZone = false;
        if (other.CompareTag("BigJumpZone")) inBigJumpZone = false;
        if (other.CompareTag("JumpOverZone")) inJumpOverZone = false;
        if (other.CompareTag("RunningJumpZone")) inRunningJumpZone = false;
    }

    private void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        anim.SetTrigger("doDeath");
    }
}