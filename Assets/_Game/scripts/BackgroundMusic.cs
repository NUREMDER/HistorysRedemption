using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;
    private AudioSource audioSource;

    void Awake()
    {
        // prevents audios binding
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // prevents this object from being destroyed when the scene changes.

        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        //  plays audio when game starts
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
        // audio can play only in this scenes
        if (scene.name == "MainMenu" || scene.name == "Araf_Lobby")
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            // stops audio in other scenes
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
