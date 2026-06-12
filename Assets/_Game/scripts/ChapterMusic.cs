using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class ChapterMusic : MonoBehaviour
{
    private static ChapterMusic instance;
    private AudioSource audioSource;

    [Header("Music Settings")]
    [Range(0f, 1f)]
    public float volume = 0.5f;

    void Awake()
    {
        // Eğer sahnede halihazırda çalan bir ChapterMusic varsa:
        if (instance != null && instance != this)
        {
            // Çalan müziğin sesini bu sahnedeki yeni ayara göre güncelle ve kopyayı yok et
            instance.volume = this.volume;
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Sahne geçişlerinde yok olmasını engeller
        
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
    }

    void Update()
    {
        // Inspector'dan volume değiştirildiğinde anında müziğe yansıtır
        if (audioSource != null && audioSource.volume != volume)
        {
            audioSource.volume = volume;
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
        // Sadece isminde "Chapter" geçen sahnelerde çalsın
        if (scene.name.Contains("Chapter"))
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            // Chapter harici bir yere gidilirse (örn: Araf) müziği durdur
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
