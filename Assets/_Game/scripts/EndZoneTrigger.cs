using UnityEngine;
using System.Collections;
using Cinemachine;

public class EndZoneTrigger : MonoBehaviour
{
    [Header("Karakter Geçiş Ayarları")]
    [Tooltip("Parkurda kullandığın karakter (Örn: Shadow1)")]
    public GameObject parkourCharacter; 
    
    [Tooltip("Savaşta kullanacağın ana karakter (Örn: Player)")]
    public GameObject combatCharacter; 

    [Tooltip("Savaşılacak düşman (Örn: Tesla) - Savaş başlayana kadar kapalı tutabilirsin")]
    public GameObject enemyCharacter;

    [Tooltip("Parkur bittiğinde arkadan kapatılacak bariyer (İsteğe bağlı)")]
    public GameObject barrierForParkour;

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Eğer giren obje Parkur Karakteriyse ve daha önce tetiklenmediyse
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(SwitchToCombatRoutine());
        }
    }

    private IEnumerator SwitchToCombatRoutine()
    {
        SceneChanger changer = FindObjectOfType<SceneChanger>();

        // 1. Loading ekranını aç
        if (changer != null && changer.loadingCanvas != null)
        {
            changer.loadingCanvas.SetActive(true);
        }

        // 2. Loading ekranında 3 saniye bekle
        yield return new WaitForSeconds(3f);

        // 3. Karakter Değişim İşlemleri
        if (parkourCharacter != null && combatCharacter != null)
        {
        // Savaş karakterini, tam olarak parkur karakterinin bittiği noktaya ışınla
            combatCharacter.transform.position = parkourCharacter.transform.position;

            // Parkur karakterini sahneden sil/gizle, savaş karakterini aktif et
            parkourCharacter.SetActive(false);
            combatCharacter.SetActive(true);

            // Kamerayı yeni savaş karakterine odakla
            CameraManager camManager = FindObjectOfType<CameraManager>();
            if (camManager != null && camManager.openWorldCam != null)
            {
                // Eğer Player'ın içinde "CameraTarget" adında özel bir boş obje varsa ona odaklan
                Transform focusTarget = combatCharacter.transform;
                Transform customTarget = combatCharacter.transform.Find("CameraTarget");
                
                if (customTarget != null)
                {
                    focusTarget = customTarget;
                }

                camManager.openWorldCam.Follow = focusTarget;
                camManager.openWorldCam.LookAt = focusTarget;
            }
        }

        // Varsa düşmanı da artık aktif et
        if (enemyCharacter != null)
        {
            enemyCharacter.SetActive(true);
        }

        // Parkur bittiğinde arkadan kapatılacak bariyer
        if (barrierForParkour != null)
        {
            barrierForParkour.SetActive(true);
        }

        // 4. Loading ekranını kapat ve dövüş başlasın!
        if (changer != null && changer.loadingCanvas != null)
        {
            changer.loadingCanvas.SetActive(false);
        }
    }
}