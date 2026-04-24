using UnityEngine;
using UnityEngine.SceneManagement; // Sahne değişimi için şart
using System.Collections;

public class SceneChanger : MonoBehaviour
{
    [Header("Görsel Ayarlar")]
    public GameObject loadingCanvas; // Dönen simgenin olduğu Canvas
    public float waitTime = 40f;      // Kaç saniye dönecek?

    // Bu fonksiyonu parkur bittiğinde veya bir butona basıldığında çağıracağız
    public void ChangeScene(string targetSceneName)
    {
        StartCoroutine(LoadingProcess(targetSceneName));
    }

    IEnumerator LoadingProcess(string sceneName)
    {
        // 1. Loading ekranını aç (Dönen simge çalışmaya başlar)
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(true);
        }

        // 2. Belirlediğimiz süre kadar bekle (2 saniye)
        yield return new WaitForSeconds(waitTime);

        // 3. Yeni sahneyi yükle
        SceneManager.LoadScene(sceneName);
    }
}