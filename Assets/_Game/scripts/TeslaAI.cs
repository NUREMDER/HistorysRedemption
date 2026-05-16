using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TeslaAI : MonoBehaviour
{
    [Header("Can Ayarlari")]
    public int maxHealth = 100;
    private int currentHealth;
    public Image healthBarFill;

    [Header("Hedef Ayarlari")]
    public Transform player;
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float stopDistance = 1.2f;

    [Header("Saldiri Ayarlari")]
    public float attackCooldown = 1.5f;
    private float lastAttackTime = 0f;

    [Header("Animasyon Süreleri")]
    public float attackHitDelay = 0.3f; // Vuruşun gerçekleşeceği an
    public float attackAnimationDuration = 1.0f; // Animasyonun toplam süresi
    public float throwDelay = 0.3f; // Fırlatmanın gerçekleşeceği an
    public float throwAnimationDuration = 1.0f; // Fırlatma animasyonunun toplam süresi

    [Header("Menzilli Saldiri (Elektrik)")]
    public bool enableRangedAttack = true;
    public GameObject electricityPrefab; // Tesla'nin kendi elektrik prefab'i (TeslaProjectile scripti olan)
    public Transform throwPoint;
    public float rangedAttackRange = 8f;
    public float minRangedDistance = 3f;
    public float rangedAttackCooldown = 3f;
    private float lastRangedAttackTime = 0f;

    [Header("Defans Ayarlari")]
    public int blockProtectionDamage = 2;
    public float blockChance = 40f;
    public float blockDuration = 2.0f;
    public float hurtDuration = 3f;

    [Header("Hitbox Ayarlari")]
    public Transform highAttackPoint;
    public Transform midAttackPoint;
    public Transform lowAttackPoint;
    public float attackRange = 0.8f;
    public LayerMask playerLayer;

    [Header("VFX & SFX Ayarlari")]
    public GameObject hitEffectPrefab;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip blockSound;

    [Header("3D Model Ayarlari (FBX)")]
    public Animator modelAnimator; // FBX içindeki Animator buraya atanacak
    public SkinnedMeshRenderer[] meshRenderers; // Hasar yediğinde kızarması için 3D meshler

    [Header("Model Yön Ayarlari")]
    [Tooltip("FBX modelin sağa baktığındaki Y rotasyon değeri (derece). Inspector'dan değil, koddan ayarlanır.")]
    public float modelRotationOffset = -90f;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private bool isAttacking = false;
    private bool isBlocking = false;
    private bool isFacingRight = true;
    private bool isDead = false;
    private bool isHurt = false;
    private Coroutine hurtCoroutine;
    private bool isFlashing = false;
    private Quaternion rightFacingRotation;
    private Quaternion leftFacingRotation;

    void Start()
    {
        hurtDuration = 3f;
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        if (modelAnimator == null)
            modelAnimator = GetComponentInChildren<Animator>();

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

        // Sahne rotasyonunu kaydet ve flip için kullan (Quaternion = gimbal lock yok)
        rightFacingRotation = transform.rotation;
        leftFacingRotation = transform.rotation * Quaternion.Euler(0f, 180f, 0f);
    }

    void Update()
    {
        if (isDead || player == null) return;
        if (isHurt)
        {
            FacePlayer(); // Hasar yerken de player'a bak
            StopMoving();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRange)
        {
            FacePlayer();

            if (isBlocking)
            {
                StopMoving();
                return;
            }

            // Menzilli Saldırı Kontrolü
            if (enableRangedAttack && distanceToPlayer > minRangedDistance && distanceToPlayer < rangedAttackRange && !isAttacking)
            {
                if (Time.time >= lastRangedAttackTime + rangedAttackCooldown)
                {
                    StartCoroutine(ThrowKnifeRoutine());
                    return; // Eğer fırlatıyorsa yürümeye çalışmasın
                }
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

        // Animator parametrelerini güncelle
        if (modelAnimator != null)
        {
            modelAnimator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            modelAnimator.SetBool("IsBlocking", isBlocking);
        }
    }

    void MoveTowardsPlayer()
    {
        float direction = player.position.x > transform.position.x ? 1 : -1;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
    }

    void StopMoving()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    void FacePlayer()
    {
        // Tüm animasyonlarda (Hurt, Block, Attack vs.) Tesla her zaman player'a baksın

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
        // Quaternion ile flip (gimbal lock olmaz, X ve Z asla bozulmaz)
        transform.rotation = isFacingRight ? rightFacingRotation : leftFacingRotation;
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        StopMoving();

        if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound);

        int randomAttack = Random.Range(0, 3);

        if (modelAnimator != null)
        {
            modelAnimator.SetInteger("AttackType", randomAttack);
            modelAnimator.SetTrigger("AttackTrigger");
        }

        lastAttackTime = Time.time;

        // Vuruş anı (Animasyonun tam vurma hissiyatına denk gelen zaman)
        yield return new WaitForSeconds(attackHitDelay);
        
        // Animasyon eventi kullanmadan hasarı doğrudan script içinden vuruyoruz
        TriggerAttackHit(randomAttack);

        // Animasyonun bitişi için geri kalan süreyi bekle
        float remainingTime = Mathf.Max(0, attackAnimationDuration - attackHitDelay);
        yield return new WaitForSeconds(remainingTime);

        isAttacking = false;
        if (modelAnimator != null)
        {
            modelAnimator.SetInteger("AttackType", 0);
        }
    }

    IEnumerator ThrowKnifeRoutine()
    {
        isAttacking = true;
        StopMoving();

        if (modelAnimator != null)
        {
            // Tesla'nın atış animasyonunu tetikle
            modelAnimator.SetTrigger("ThrowObject");
        }

        lastRangedAttackTime = Time.time;

        // Fırlatma anına kadar bekle (Throw Delay süresi)
        yield return new WaitForSeconds(throwDelay);

        InstantiateKnife();

        // Animasyonun bitmesini bekle
        float remainingTime = Mathf.Max(0, throwAnimationDuration - throwDelay);
        yield return new WaitForSeconds(remainingTime);

        isAttacking = false;
    }

    public void InstantiateKnife()
    {
        if (throwPoint == null) { Debug.LogError("TeslaAI: throwPoint is NULL! Lütfen Inspector'dan Throw Point atayın."); return; }
        if (electricityPrefab == null) { Debug.LogError("TeslaAI: electricityPrefab atanmamış! Lütfen Inspector'dan elektrik prefab'ini atayın."); return; }

        // Z eksenini karakterin Z'sine sabitliyoruz (3D model derinlik sorunu için)
        Vector3 spawnPos = throwPoint.position;
        spawnPos.z = transform.position.z;

        // Player'ın tam konumuna doğru fırlatması için yön hesapla
        Vector2 shootDir = (player.position - throwPoint.position).normalized;

        // Rotasyonu hesapla (transform.right bu açıya bakacak)
        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject spawned = Instantiate(electricityPrefab, spawnPos, rotation);
        Debug.Log("Tesla elektrik fırlattı! Yön: " + shootDir + " Açı: " + angle);
    }

    IEnumerator BlockRoutine()
    {
        isBlocking = true;
        StopMoving();
        if (modelAnimator != null) modelAnimator.SetBool("IsBlocking", true);

        yield return new WaitForSeconds(blockDuration);

        isBlocking = false;
        if (modelAnimator != null) modelAnimator.SetBool("IsBlocking", false);
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
                playerScript.TakeDamage(10); // Saldırı gücü
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
            isAttacking = false;
            if (modelAnimator != null) modelAnimator.SetTrigger("Hurt");
            
            if (hurtCoroutine != null) StopCoroutine(hurtCoroutine);
            hurtCoroutine = StartCoroutine(HurtStunRoutine());
            
            if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);

            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
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
        StopAllCoroutines();
        if (modelAnimator != null) modelAnimator.SetTrigger("Die");

        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;

        StartCoroutine(DieRoutine());
    }

    IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        this.enabled = false;

        if (GameManager.instance != null)
        {
            GameManager.instance.EnemyDefeated(100, 200, 20);
        }
    }

    IEnumerator HurtStunRoutine()
    {
        isHurt = true;
        isAttacking = false;
        StopMoving();
        
        float timer = 0f;
        while (timer < hurtDuration)
        {
            if (isDead) yield break;
            StopMoving();
            timer += Time.deltaTime;
            yield return null;
        }
        
        isHurt = false;
        hurtCoroutine = null;

        if (!isDead && !isAttacking && Random.Range(0, 100) < blockChance)
        {
            StartCoroutine(BlockRoutine());
        }
    }

    IEnumerator FlashRed()
    {
        if (isFlashing) yield break; // Üst üste binen flash'ları engelle
        if (meshRenderers == null || meshRenderers.Length == 0) yield break;

        isFlashing = true;

        // MaterialPropertyBlock kullanarak orijinal materyallere dokunmadan renk değiştir
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        propBlock.SetColor("_Color", Color.red);

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null)
                meshRenderers[i].SetPropertyBlock(propBlock);
        }

        yield return new WaitForSeconds(0.1f);

        // PropertyBlock'u temizle - orijinal materyal otomatik geri gelir
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null)
                meshRenderers[i].SetPropertyBlock(null);
        }

        isFlashing = false;
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

