using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    // Yonetmenimizi (CameraManager) buraya baglayacagiz
    public CameraManager cameraManager;

    // Boss alanina girince ne olsun?
    public bool isBossZone = true;

    [Header("Dovus Alanı Bariyerleri")]
    public GameObject barrierLeft;
    public GameObject barrierRight;

    // Unity'nin "Biri alanima girdi" fonksiyonu
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Giren obje "Player" etiketine sahip mi?
        if (other.CompareTag("Player"))
        {
            if (isBossZone)
            {
                Debug.Log("Boss Alanina Girildi! Kamera Degisiyor...");
                cameraManager.EnterBossMode();

                // Bariyerleri aktif et (Dovus alanindan kacamamasi icin)
                if (barrierLeft != null) barrierLeft.SetActive(true);
                if (barrierRight != null) barrierRight.SetActive(true);
            }
            else
            {
                cameraManager.ExitBossMode();
            }
        }
    }

    // (Istege Bagli) Alandan cikinca eski haline donsun istersen:
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isBossZone)
        {
            // Burayi simdilik bos birakiyorum, genelde boss dovusu bitene kadar cikilmaz.
            // Ama acik dunya gezintisi icin kullanacaksan:
            // cameraManager.ExitBossMode();
        }
    }
}
