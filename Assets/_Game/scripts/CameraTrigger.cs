using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    // Yönetmenimizi (CameraManager) buraya baðlayacaðýz
    public CameraManager cameraManager;

    // Boss alanýna girince ne olsun?
    public bool isBossZone = true;

    // Unity'nin "Biri alanýma girdi" fonksiyonu
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Giren obje "Player" etiketine sahip mi?
        if (other.CompareTag("Player"))
        {
            if (isBossZone)
            {
                Debug.Log("Boss Alanýna Girildi! Kamera Deðiþiyor...");
                cameraManager.EnterBossMode();
            }
            else
            {
                cameraManager.ExitBossMode();
            }
        }
    }

    // (Ýsteðe Baðlý) Alandan çýkýnca eski haline dönsün istersen:
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isBossZone)
        {
            // Burayý þimdilik boþ býrakýyorum, genelde boss dövüþü bitene kadar çýkýlmaz.
            // Ama açýk dünya gezintisi için kullanacaksan:
            // cameraManager.ExitBossMode();
        }
    }
}