using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(CapsuleCollider))]
public class ParkourController : MonoBehaviour
{
    [Header("Hareket")]
    public float runSpeed = 6f;
    public float jumpForce = 8f;
    public float slideSpeedMultiplier = 0.8f;
    public float speedRecoveryRate = 3f;
    public float parkourCooldown = 0.8f;
    public float slideDuration = 0.7f;

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
            if (inBigJumpZone) StartCoroutine(DoParkourDelayed("doBigJump", jumpForce * 1.5f, bigJumpSpeedMultiplier, bigJumpDelay));
            else if (inJumpOverZone) StartCoroutine(DoParkourDelayed("doJumpOver", jumpForce, jumpOverSpeedMultiplier, jumpDelay));
            else if (inRunningJumpZone) DoParkour("doRunningJump", jumpForce);
            else if (inJumping1Zone) DoParkour("doJumping1", jumpForce);
            else if (inThrowZone) DoParkour("doThrow", 0f);
            else if (isGrounded) DoParkour("doRunningJump", jumpForce);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (inSlideZone || isGrounded) StartCoroutine(DoSlide());
            else if (inJumpingDownZone) DoParkour("doJumpingDown", 0f);
            else if (inJumpingDown1Zone) DoParkour("doJumpingDown1", 0f);
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
    }

    private void DoParkour(string animTrigger, float upwardForce)
    {
        ClearTriggers();
        anim.SetTrigger(animTrigger);

        if (upwardForce > 0f)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
        }

        StartCoroutine(Cooldown(parkourCooldown));
    }

    private IEnumerator DoParkourDelayed(string animTrigger, float upwardForce, float speedMult, float delay)
    {
        isDoingParkour = true;
        ClearTriggers();
        anim.SetTrigger(animTrigger);

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

        currentSpeed = runSpeed * slideSpeedMultiplier;

        col.height = originalColHeight * 0.5f;
        col.center = new Vector3(originalColCenter.x, originalColCenter.y - originalColHeight * 0.25f, originalColCenter.z);

        Vector3 pos = transform.position;
        pos.y -= slideYOffset;
        transform.position = pos;

        yield return new WaitForSeconds(slideDuration);

        col.height = originalColHeight;
        col.center = originalColCenter;

        isSliding = false;
        isDoingParkour = false;
    }

    private IEnumerator Cooldown(float duration)
    {
        isDoingParkour = true;
        yield return new WaitForSeconds(duration);
        isDoingParkour = false;
    }

    void OnTriggerEnter(Collider other)
    {
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

    private void OnDrawGizmosSelected()
    {
        if (col == null) col = GetComponent<CapsuleCollider>();
        if (col == null) return;

        Vector3 spherePos = col.bounds.center - new Vector3(0, col.bounds.extents.y - 0.1f, 0);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(spherePos, 0.2f);
    }
}