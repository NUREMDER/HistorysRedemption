using UnityEngine;

/// <summary>
/// Self-contained floating damage popup. No prefab needed — call DamagePopup.Create() from anywhere.
/// Uses Time.unscaledDeltaTime so it animates even during hitstop (Time.timeScale = 0).
/// </summary>
public class DamagePopup : MonoBehaviour
{
    private TextMesh textMesh;
    private Color startColor;
    private float moveSpeed = 1.5f;
    private float lifetime = 0.8f;
    private float timer = 0f;

    /// <summary>
    /// Creates a floating damage popup at the given world position.
    /// </summary>
    public static DamagePopup Create(Vector3 position, int damageAmount)
    {
        GameObject popup = new GameObject("DamagePopup");
        // Slight random horizontal offset so multiple hits don't stack perfectly
        popup.transform.position = position + new Vector3(Random.Range(-0.4f, 0.4f), 0.6f, 0);

        TextMesh tm = popup.AddComponent<TextMesh>();
        tm.text = "Hit -" + damageAmount;
        tm.fontSize = 36;
        tm.color = new Color(1f, 0.1f, 0.1f, 1f); // Bright red
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.characterSize = 0.12f;
        tm.fontStyle = FontStyle.Bold;

        // Ensure text renders on top of all sprites
        MeshRenderer renderer = popup.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 100;

        DamagePopup dp = popup.AddComponent<DamagePopup>();
        return dp;
    }

    void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        if (textMesh != null) startColor = textMesh.color;
    }

    void Update()
    {
        // Use unscaledDeltaTime so popup still animates during hitstop
        float dt = Time.unscaledDeltaTime;

        // Float upward
        transform.position += Vector3.up * moveSpeed * dt;

        timer += dt;

        // Pop-in scale effect: grow fast then settle
        if (timer < 0.08f)
        {
            float scale = Mathf.Lerp(0.4f, 1.4f, timer / 0.08f);
            transform.localScale = Vector3.one * scale;
        }
        else if (timer < 0.18f)
        {
            float scale = Mathf.Lerp(1.4f, 1f, (timer - 0.08f) / 0.1f);
            transform.localScale = Vector3.one * scale;
        }

        // Fade out in the second half of lifetime
        if (timer > lifetime * 0.5f)
        {
            float alpha = Mathf.Lerp(1f, 0f, (timer - lifetime * 0.5f) / (lifetime * 0.5f));
            if (textMesh != null)
                textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }

        // Self-destruct
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
