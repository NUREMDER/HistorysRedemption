using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject invisibleWall; // ParkourBlocker'ı buraya sürükle
    public float delay = 1.0f;       // 1 saniye sonra kilitlensin

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Sen geçtikten 'delay' saniye sonra 'ActivateWall' fonksiyonunu çalıştır
            Invoke("ActivateWall", delay);

            // Parkur modunu hemen kapat (Dövüş başlasın)
            var parkur = other.GetComponent<ParkourController2d>();
            if (parkur != null)
            {
                parkur.isParkourActive = false;
                // Karakterin durması için hızı sıfırlayalım
                other.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }

            Debug.Log("Çizgi geçildi, 1 saniye içinde kapı kapanacak!");
        }
    }

    void ActivateWall()
    {
        if (invisibleWall != null)
        {
            invisibleWall.SetActive(true);
            Debug.Log("DUVAR ÖRÜLDÜ! Artık geri dönüş yok.");
        }
        
        // Bu trigger'ı sahneden temizle
        Destroy(gameObject);
    }
}