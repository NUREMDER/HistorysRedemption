using UnityEngine;
using UnityEditor;

public class ParkourLevelGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Vector Parkour Level")]
    public static void GenerateLevel()
    {
        // Temiz bir zemin oluşturmak için ebeveyn (parent) obje
        GameObject levelRoot = new GameObject("Parkour Level (Generated)");
        
        float currentX = 0f;
        float groundHeight = -1f;

        // 1. ZEMIN - Koşuya başlama
        CreatePlatform(levelRoot, "Start Platform", currentX, groundHeight, 15f, 1f);
        currentX += 15f;

        // 2. KÜÇÜK ZIPLAMA BOSLUGU
        currentX += 3f; // Gap 
        CreatePlatform(levelRoot, "Platform 2", currentX, groundHeight, 10f, 1f);
        // Zıplama tetikleyicisi
        CreateTrigger(levelRoot, "RunningJumpZone", currentX - 1.5f, groundHeight + 1f, 2f, 2f);
        currentX += 10f;

        // 3. SLIDE (KAYMA) ENGELİ
        CreatePlatform(levelRoot, "Platform 3", currentX, groundHeight, 15f, 1f);
        // Havada asılı duran duvar (altından kayılacak)
        GameObject slideObstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slideObstacle.name = "Slide Obstacle";
        slideObstacle.transform.position = new Vector3(currentX + 5f, groundHeight + 2f, 0f);
        slideObstacle.transform.localScale = new Vector3(2f, 2.5f, 2f);
        slideObstacle.transform.parent = levelRoot.transform;
        
        // Slide tetikleyicisi
        CreateTrigger(levelRoot, "slideZone", currentX + 3.5f, groundHeight + 1f, 3f, 2f);
        currentX += 15f;

        // 4. BÜYÜK DUVARE - (Over Jump veya Big Jump)
        currentX += 2f; // Küçük bir boşluk
        CreatePlatform(levelRoot, "Platform 4", currentX, groundHeight, 10f, 1f);
        GameObject wallObstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallObstacle.name = "Wall Obstacle";
        wallObstacle.transform.position = new Vector3(currentX + 4f, groundHeight + 1.5f, 0f);
        wallObstacle.transform.localScale = new Vector3(2f, 4f, 2f);
        wallObstacle.transform.parent = levelRoot.transform;

        // Ustunden atlama tetikleyicisi
        CreateTrigger(levelRoot, "jumpOverZone", currentX + 2.5f, groundHeight + 1f, 3f, 2f);
        currentX += 10f;

        // 5. YÜKSEK PLATFORM - (Ustune Ziplama)
        currentX += 4f; // Boşluk
        groundHeight += 2f; // Platformu yükselt
        CreatePlatform(levelRoot, "High Platform", currentX, groundHeight, 15f, 1f);
        CreateTrigger(levelRoot, "BigJumpZone", currentX - 2.5f, groundHeight - 1f, 3f, 3f);
        currentX += 15f;

        // 6. AŞAĞI ATLA (Jumping Down)
        currentX += 2f; // Boşluk
        groundHeight -= 4f; // Derin bir çukur
        CreatePlatform(levelRoot, "Low Platform", currentX, groundHeight, 20f, 1f);
        CreateTrigger(levelRoot, "jumpingDownZone", currentX - 1.5f, groundHeight + 3f, 3f, 3f);

        Debug.Log("Vector tarzı parkur haritası Ex1 sahnesine başarıyla eklendi! 'Parkour Level (Generated)' objesini inceleyin.");
    }

    private static void CreatePlatform(GameObject parent, string name, float x, float y, float width, float height)
    {
        GameObject plat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plat.name = name;
        // Platformu sağa doğru uzat, böylece X pozisyonu platformun sonunu değil başlangıcını temsil etsin.
        plat.transform.position = new Vector3(x + (width / 2f), y, 0f);
        plat.transform.localScale = new Vector3(width, height, 4f); // Derinliği 4 birim
        plat.transform.parent = parent.transform;

        // Siyahımsı gri bir renk atayalım (görsellik için opsiyonel, ama default küpten iyidir)
        var renderer = plat.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = new Material(Shader.Find("Standard"));
            renderer.sharedMaterial.color = new Color(0.2f, 0.2f, 0.2f);
        }
    }

    private static void CreateTrigger(GameObject parent, string tagName, float x, float y, float width, float height)
    {
        GameObject trig = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trig.name = "Trigger - " + tagName;
        trig.tag = tagName; // Unity'de bu tag yoksa konsola hata basabilir, oyundan önce tag'leri kontrol edin.
        
        trig.transform.position = new Vector3(x, y, 0f);
        trig.transform.localScale = new Vector3(width, height, 4f);
        trig.transform.parent = parent.transform;

        // Çarpışmayı pasif edip (IsTrigger) görünmez/saydam yapalım
        BoxCollider col = trig.GetComponent<BoxCollider>();
        col.isTrigger = true;

        var renderer = trig.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            // Yarı saydam yeşil bir materyal oluştur (Editörde triggerları rahat görebilirsiniz)
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0f, 1f, 0f, 0.3f); 
            // Transparent moduna almak koddan uzundur, bu yüzden RENDERER'ı kapatmak daha garantilidir.
            renderer.enabled = false; // Görünmez olsun
        }
    }
}
