using UnityEngine;
using System.Collections;

/// <summary>
/// Creates orbiting electric spark particles around Edison (EnemyFighter) as a child component.
/// Deals small periodic damage (3 HP) when the player gets too close.
/// Attach this script to a child GameObject of the EnemyFighter.
/// </summary>
public class ElectricAura : MonoBehaviour
{
    [Header("Orbit Settings")]
    public float orbitRadius = 1.5f;
    public float orbitSpeed = 150f; // Degrees per second
    public int particleCount = 4;
    public float orbitYOffset = -0.8f; // Negative = lower toward legs

    [Header("Damage Settings")]
    public int damagePerTick = 3;
    public float damageInterval = 4.0f;
    public float damageRadius = 1.8f;
    public LayerMask playerLayer;

    [Header("Visual Settings")]
    public Color sparkColor = new Color(0.4f, 0.75f, 1f, 0.9f); // Electric blue
    public Color glowColor = new Color(0.2f, 0.5f, 1f, 0.4f);   // Softer glow
    public float sparkSize = 0.2f;

    private GameObject[] sparks;
    private float[] sparkAngles;
    private float[] sparkRadiusOffsets;
    private float lastDamageTime;
    private SpriteRenderer[] sparkRenderers;
    private bool isActive = true;
    private EnemyAI parentEnemy;

    void Start()
    {
        CreateSparks();
        lastDamageTime = -damageInterval; // Allow immediate first tick

        // Cache the parent EnemyAI to detect death
        parentEnemy = GetComponentInParent<EnemyAI>();
    }

    void Update()
    {
        if (!isActive || sparks == null) return;

        // Auto-disable aura when the enemy dies
        if (parentEnemy != null && parentEnemy.isDead)
        {
            DeactivateAura();
            return;
        }

        RotateSparks();
        AnimateSparks();
        CheckDamage();
    }

    /// <summary>
    /// Hides all spark visuals and stops damage when the enemy dies.
    /// </summary>
    private void DeactivateAura()
    {
        isActive = false;
        for (int i = 0; i < particleCount; i++)
        {
            if (sparks[i] != null)
                sparks[i].SetActive(false);
        }
    }

    /// <summary>
    /// Programmatically creates spark GameObjects with SpriteRenderers in orbit positions.
    /// No external sprite assets needed — uses a procedural white circle texture.
    /// </summary>
    private void CreateSparks()
    {
        sparks = new GameObject[particleCount];
        sparkAngles = new float[particleCount];
        sparkRadiusOffsets = new float[particleCount];
        sparkRenderers = new SpriteRenderer[particleCount];

        // Create a small procedural circle texture for sparks
        Texture2D sparkTex = CreateCircleTexture(32);
        Sprite sparkSprite = Sprite.Create(sparkTex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 100f);

        float angleStep = 360f / particleCount;

        for (int i = 0; i < particleCount; i++)
        {
            // Create the spark child object
            GameObject spark = new GameObject("ElectricSpark_" + i);
            spark.transform.SetParent(transform);
            spark.transform.localPosition = Vector3.zero;

            // Add SpriteRenderer with the procedural sprite
            SpriteRenderer sr = spark.AddComponent<SpriteRenderer>();
            sr.sprite = sparkSprite;
            sr.color = sparkColor;
            sr.sortingOrder = 10; // Render above the character
            sr.material = new Material(Shader.Find("Sprites/Default"));

            // Set the initial orbit angle evenly spaced
            sparkAngles[i] = angleStep * i;

            // Add slight random radius variation for organic look
            sparkRadiusOffsets[i] = Random.Range(-0.15f, 0.15f);

            // Set the initial scale
            spark.transform.localScale = Vector3.one * sparkSize;

            sparks[i] = spark;
            sparkRenderers[i] = sr;

            // Create a glow child behind the main spark for a subtle bloom effect
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(spark.transform);
            glow.transform.localPosition = Vector3.zero;
            glow.transform.localScale = Vector3.one * 2.5f; // Glow is bigger than spark

            SpriteRenderer glowSr = glow.AddComponent<SpriteRenderer>();
            glowSr.sprite = sparkSprite;
            glowSr.color = glowColor;
            glowSr.sortingOrder = 9; // Behind main spark
        }
    }

    /// <summary>
    /// Smoothly rotates all sparks around the parent's position in orbit.
    /// Each spark has a slight radius offset for natural variation.
    /// </summary>
    private void RotateSparks()
    {
        for (int i = 0; i < particleCount; i++)
        {
            if (sparks[i] == null) continue;

            // Advance the angle continuously
            sparkAngles[i] += orbitSpeed * Time.deltaTime;
            if (sparkAngles[i] >= 360f) sparkAngles[i] -= 360f;

            // Calculate position on the 2D orbit circle
            float currentRadius = orbitRadius + sparkRadiusOffsets[i];
            float rad = sparkAngles[i] * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad) * currentRadius;
            float y = Mathf.Sin(rad) * currentRadius;

            sparks[i].transform.localPosition = new Vector3(x, y + orbitYOffset, 0f);
        }
    }

    /// <summary>
    /// Adds flickering brightness and subtle size pulsing to create a living electric effect.
    /// </summary>
    private void AnimateSparks()
    {
        for (int i = 0; i < particleCount; i++)
        {
            if (sparkRenderers[i] == null) continue;

            // Flicker the alpha between 0.5 and 1.0 using Perlin noise for organic randomness
            float noise = Mathf.PerlinNoise(Time.time * 8f + i * 100f, i * 50f);
            float alpha = Mathf.Lerp(0.5f, 1f, noise);
            Color c = sparkColor;
            c.a = alpha;
            sparkRenderers[i].color = c;

            // Subtle size pulse
            float scale = sparkSize * Mathf.Lerp(0.8f, 1.3f, noise);
            sparks[i].transform.localScale = Vector3.one * scale;

            // Slight random radius wobble for electric chaos effect
            sparkRadiusOffsets[i] = Mathf.Lerp(sparkRadiusOffsets[i],
                Random.Range(-0.2f, 0.2f), Time.deltaTime * 3f);
        }
    }

    /// <summary>
    /// Checks if the player is within the damage radius and applies periodic small damage.
    /// </summary>
    private void CheckDamage()
    {
        if (Time.time < lastDamageTime + damageInterval) return;

        // Use the parent's position (EnemyFighter) as the center of the damage zone
        Vector2 center = transform.parent != null ? (Vector2)transform.parent.position : (Vector2)transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, damageRadius, playerLayer);

        foreach (Collider2D hit in hits)
        {
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damagePerTick);
                lastDamageTime = Time.time;

                // Brief bright flash on all sparks when dealing damage
                StartCoroutine(DamageFlash());
                break; // Only damage once per tick
            }
        }
    }

    /// <summary>
    /// Quick white flash on all sparks when damage is dealt, then fade back to normal color.
    /// </summary>
    private IEnumerator DamageFlash()
    {
        // Flash white
        for (int i = 0; i < particleCount; i++)
        {
            if (sparkRenderers[i] != null)
            {
                sparkRenderers[i].color = Color.white;
                sparks[i].transform.localScale = Vector3.one * sparkSize * 2f;
            }
        }

        yield return new WaitForSeconds(0.1f);

        // Return to normal
        for (int i = 0; i < particleCount; i++)
        {
            if (sparkRenderers[i] != null)
            {
                sparkRenderers[i].color = sparkColor;
                sparks[i].transform.localScale = Vector3.one * sparkSize;
            }
        }
    }

    /// <summary>
    /// Creates a procedural white filled circle texture at runtime.
    /// This eliminates the need for any external sprite assets.
    /// </summary>
    private Texture2D CreateCircleTexture(int resolution)
    {
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center = resolution / 2f;
        float radius = center - 1f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= radius)
                {
                    // Smooth edge falloff for soft glow appearance
                    float edgeFade = 1f - Mathf.Clamp01((dist - radius + 3f) / 3f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, edgeFade));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.Apply();
        return tex;
    }

    /// <summary>
    /// Draws the damage radius gizmo in the Unity Editor for debugging.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.3f);
        Vector3 center = transform.parent != null ? transform.parent.position : transform.position;
        Gizmos.DrawWireSphere(center, damageRadius);

        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(center, orbitRadius);
    }
}
