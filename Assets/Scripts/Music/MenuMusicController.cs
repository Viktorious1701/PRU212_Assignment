using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMusicController : MonoBehaviour
{
    [Header("Music Settings")]
    [SerializeField] private AudioClip menuBackgroundMusic;
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private string[] scenesToStopMusic; // Scenes where music should stop

    private AudioSource musicSource;

    private void Awake()
    {
        // Ensure only one music controller exists
        if (FindObjectsOfType<MenuMusicController>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // Make this object persist between scenes
        DontDestroyOnLoad(gameObject);

        // Set up audio source
        SetupAudioSource();

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void SetupAudioSource()
    {
        // Add an AudioSource component if not already present
        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure the audio source
        musicSource.clip = menuBackgroundMusic;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;
    }

    private void Start()
    {
        // Start playing music
        if (menuBackgroundMusic != null)
        {
            PlayMusic();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the loaded scene is in the list of scenes to stop music
        if (System.Array.Exists(scenesToStopMusic, s => s == scene.name))
        {
            StopMusic();
        }
    }

    public void PlayMusic()
    {
        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    // Cleanup to prevent memory leaks
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}