using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;
    private AudioSource audioSource;

    void Awake()
    {
        // Eğer sahnede halihazırda çalan bir müzik objesi varsa yenisini yok et (Müziklerin üst üste binmesini engeller)
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Sahne değişse bile bu objenin silinmesini engeller

        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Oyun ilk açıldığında müziği başlat
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sadece Menü ve Lobi'de çalmaya devam etsin
        if (scene.name == "MainMenu" || scene.name == "Araf_Lobby")
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            // Parkur (ex1) veya Boss (Tutorial_Scene) sahnelerine geçilince müziği sustur
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
