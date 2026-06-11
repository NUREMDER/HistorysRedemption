using UnityEngine;

/// <summary>
/// Self-contained apple projectile. No prefab needed — call AppleProjectile.Create() from code.
/// Creates its own black circle visual, flies toward the target, and damages the player on hit.
/// </summary>
public class AppleProjectile : MonoBehaviour
{
    public int damage = 15;
    public float speed = 10f;
    public float lifetime = 5f;

    private Rigidbody2D rb;

    /// <summary>
    /// Creates an apple projectile at spawnPos that flies toward targetPos.
    /// </summary>
    public static AppleProjectile Create(Vector2 spawnPos, Vector2 targetPos, int damage = 15)
    {
        GameObject apple = new GameObject("AppleProjectile");
        apple.transform.position = spawnPos;

        // Create black circle visual
        SpriteRenderer sr = apple.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(0.08f, 0.08f, 0.08f, 1f); // Near-black
        sr.sortingOrder = 10;
        apple.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        // Physics — slight gravity for a natural arc
        Rigidbody2D rb = apple.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0.5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Trigger collider for hit detection
        CircleCollider2D col = apple.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        // Attach script
        AppleProjectile proj = apple.AddComponent<AppleProjectile>();
        proj.damage = damage;
        proj.rb = rb;

        // Calculate direction and launch
        Vector2 direction = (targetPos - spawnPos).normalized;
        rb.velocity = direction * proj.speed;

        // Auto-destroy if it misses
        Destroy(apple, proj.lifetime);

        return proj;
    }

    // Cached circle sprite so we only generate the texture once
    private static Sprite circleSprite;

    static Sprite CreateCircleSprite()
    {
        if (circleSprite != null) return circleSprite;

        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                colors[y * size + x] = dist <= radius ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return circleSprite;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);

                // Camera shake on apple hit
                if (CameraShake.instance != null)
                    CameraShake.instance.Shake(0.3f, 0.1f);
            }
            Destroy(gameObject);
        }
    }
}
