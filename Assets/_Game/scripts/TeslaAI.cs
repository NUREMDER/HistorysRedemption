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
    public float attackAnimationDuration = 1.0f; // Animasyonun toplam süresi
    public float throwDelay = 0.3f; // Fırlatmanın gerçekleşeceği an
    public float throwAnimationDuration = 1.0f; // Fırlatma animasyonunun toplam süresi



    [Header("Defans Ayarlari")]
    public int blockProtectionDamage = 2;
    public float blockChance = 40f;
    public float blockDuration = 2.0f;
    public float hurtDuration = 3f;
    public float knockbackForce = 3f; // Hasar yediğinde geriye savrulma hızı
    public float knockbackDelay = 1.0f; // Savrulmanın ne kadar süre sonra başlayacağı

    [Header("Hitbox Ayarlari")]
    public Transform highAttackPoint;
    public Transform midAttackPoint;
    public Transform lowAttackPoint;
    public float attackRange = 0.8f;
    public LayerMask playerLayer;

    [Header("Menzilli Saldırı Ayarları (Yıldırım)")]
    public GameObject lightningPrefab;
    public Transform throwPoint;

    [Header("VFX & SFX Ayarlari")]
    public GameObject hitEffectPrefab;
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip blockSound;

    [Header("3D Model Ayarlari (FBX)")]
    public Animator modelAnimator; // FBX içindeki Animator buraya atanacak
    public SkinnedMeshRenderer[] meshRenderers; // Hasar yediğinde kızarması için 3D meshler

    [Header("Model Yön ve Ölçek Ayarlari")]
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

        // modelRotationOffset'i kullanarak sağ ve sol yön rotasyonlarını (Y ekseninde) hesapla
        rightFacingRotation = Quaternion.Euler(0f, modelRotationOffset, 0f);
        leftFacingRotation = Quaternion.Euler(0f, modelRotationOffset + 180f, 0f);

        // Oyun başlar başlamaz player'ın nerede olduğunu bulup doğru yöne dönmesi için
        if (player != null)
        {
            isFacingRight = player.position.x > transform.position.x;
        }

        // Başlangıç rotasyonunu SADECE 3D MODEL için ayarla (Fiziği bozmamak için)
        if (modelAnimator != null)
        {
            modelAnimator.transform.localRotation = isFacingRight ? rightFacingRotation : leftFacingRotation;
        }
    }

    void Update()
    {
        if (isDead || player == null) return;
        if (isHurt)
        {
            FacePlayer(); // Hasar yerken de player'a bak
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
        // Sadece 3D modelin yönünü çevir
        if (modelAnimator != null)
        {
            modelAnimator.transform.localRotation = isFacingRight ? rightFacingRotation : leftFacingRotation;
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        StopMoving();

        if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound);

        // 0 (yumruk), 1 (tekme), 2 (yıldırım) seçecek
        int randomAttack = Random.Range(0, 3); 

        if (modelAnimator != null)
        {
            if (randomAttack == 2)
            {
                // Senin Animator'da zaten hazır olan fırlatma Trigger'ını kullanıyoruz!
                modelAnimator.SetTrigger("ThrowObject");
            }
            else
            {
                // Yumruk (0) ve Tekme (1) için eski sistem
                modelAnimator.SetInteger("AttackType", randomAttack);
                modelAnimator.SetTrigger("AttackTrigger");
            }
        }

        lastAttackTime = Time.time;

        // Animation Event olmadan kod ile hasar vurma zamanlaması
        if (randomAttack == 2)
        {
            // Yıldırım için fırlatma gecikmesi
            StartCoroutine(PerformThrow());
        }
        else
        {
            // Yumruk/Tekme için hasar verme gecikmesi (0.5 saniye)
            StartCoroutine(PerformHit(randomAttack));
        }

        yield return new WaitForSeconds(attackAnimationDuration);

        isAttacking = false;
        if (modelAnimator != null)
        {
            modelAnimator.SetInteger("AttackType", 0);
        }
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

    IEnumerator PerformHit(int pointIndex)
    {
        // Vuruşun hedefe ulaşması için yarım saniye bekle
        yield return new WaitForSeconds(0.5f);

        if (isDead || isHurt) yield break; // Eğer o sırada hasar yemişse veya ölmüşse vurma iptal

        Transform selectedPoint = null;
        switch (pointIndex)
        {
            case 0: selectedPoint = lowAttackPoint; break;
            case 1: selectedPoint = midAttackPoint; break;
            case 2: selectedPoint = highAttackPoint; break;
            default: selectedPoint = midAttackPoint; break;
        }

        if (selectedPoint == null) yield break;

        float direction = isFacingRight ? 1f : -1f;
        float offsetX = Mathf.Abs(selectedPoint.position.x - transform.position.x);
        if (offsetX < 0.1f) offsetX = 1.0f; 

        Vector2 hitPosition = new Vector2(transform.position.x + (direction * offsetX), selectedPoint.position.y);

        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(hitPosition, attackRange, playerLayer);
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

    IEnumerator PerformThrow()
    {
        yield return new WaitForSeconds(throwDelay);

        if (isDead || isHurt) yield break;

        if (lightningPrefab != null && player != null)
        {
            // Senin isteğin üzerine ThrowPoint'i tamamen iptal ettik!
            // Artık elektriği tam o saniyede Player'ın bulunduğu yerin gökyüzünden (yüksekten) indiriyoruz.
            // X ekseni Player'ın X'i, Y ekseni ise Player'ın 10 birim yukarısı (gökyüzü)
            Vector2 spawnPos = new Vector2(player.position.x, player.position.y + 10f);
            
            // Yıldırımın tam aşağı (dümdüz) düşmesi için Z ekseninde -90 derece çeviriyoruz (Sağa doğru olan prefab aşağı doğru baksın diye)
            Quaternion spawnRot = Quaternion.Euler(0, 0, -90);
            
            Instantiate(lightningPrefab, spawnPos, spawnRot);
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
            
            float knockbackDir = player.position.x > transform.position.x ? -1f : 1f;

            if (hurtCoroutine != null) StopCoroutine(hurtCoroutine);
            hurtCoroutine = StartCoroutine(HurtStunRoutine(knockbackDir));
            
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

        // Fiziği tamamen kapatıyoruz ki ceset itilmesin veya takılmasın
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;

        StartCoroutine(DieRoutine());
    }

    IEnumerator DieRoutine()
    {
        // 3D modelin kendi pozisyonunu aşağı indiriyoruz ki havada asılı kalmasın
        float dropDuration = 1.0f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - new Vector3(0f, 1.8f, 0f); // 1.8 birim aşağı çek

        while (elapsed < dropDuration)
        {
            if (this != null) 
            {
                transform.position = Vector3.Lerp(startPos, endPos, elapsed / dropDuration);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);

        this.enabled = false;

        if (GameManager.instance != null)
        {
            GameManager.instance.EnemyDefeated(100, 200, 20);
        }
    }

    IEnumerator HurtStunRoutine(float knockbackDir)
    {
        isHurt = true;
        isAttacking = false;
        
        float timer = 0f;
        bool knocked = false;

        while (timer < hurtDuration)
        {
            if (isDead) yield break;
            
            // Belirlenen süre geçince (1 sn) savrulmayı uygula
            if (timer >= knockbackDelay && !knocked)
            {
                rb.velocity = new Vector2(knockbackDir * knockbackForce, rb.velocity.y);
                knocked = true;
            }

            // Savrulduktan sonra sürtünme etkisiyle durdur
            if (knocked)
            {
                rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, 0, Time.deltaTime * 5f), rb.velocity.y);
            }

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

        // Kırmızı vuruş çemberini Inspector'daki Pos_Mid'e göre çizdiriyoruz
        Gizmos.color = Color.red;
        if (midAttackPoint != null)
        {
            // Oyun başlamadan da (isFacingRight = true) pozisyonu görebilmen için
            float direction = Application.isPlaying ? (isFacingRight ? 1f : -1f) : 1f;
            float offsetX = Mathf.Abs(midAttackPoint.position.x - transform.position.x);
            if (offsetX < 0.1f) offsetX = 1.0f;
            
            Vector2 hitPos = new Vector2(transform.position.x + (direction * offsetX), midAttackPoint.position.y);
            Gizmos.DrawWireSphere(hitPos, attackRange);
        }
    }

    IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}

