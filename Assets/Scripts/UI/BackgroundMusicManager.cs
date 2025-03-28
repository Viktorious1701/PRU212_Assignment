using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusicManager : MonoBehaviour
{
    // Singleton instance
    public static BackgroundMusicManager Instance { get; private set; }

    // Audio source for playing background music
    private AudioSource audioSource;

    // Array of background music clips for different levels
    [Header("Level Music Clips")]
    public AudioClip menuMusicClip;
    public AudioClip caveLevelMusic;
    public AudioClip villageLevelMusic;
    public AudioClip forestLevelMusic;
    public AudioClip castleLevelMusic;
    public AudioClip bossLevelMusic;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Add AudioSource component
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true; // Ensure music loops
    }

    private void Start()
    {
        // Subscribe to scene loaded event to change music automatically
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Automatically play music based on scene name
        switch (scene.name)
        {
            case "Main Menu":
                PlayMenuMusic();
                break;
            case "SCENE1_Cave":
                PlayCaveLevelMusic();
                break;
            case "SCENE2_Village":
                PlayVillageLevelMusic();
                break;
            case "SCENE3_Forest":
                PlayForestLevelMusic();
                break;
            case "SCENE4_Castle":
                PlayCastleLevelMusic();
                break;
            case "SCENE5_BOSS":
                PlayBossLevelMusic();
                break;
        }
    }

    // Specific music play methods for each scene
    public void PlayMenuMusic()
    {
        Debug.Log("Playing menu music");
        PlayMusic(menuMusicClip);
    }

    public void PlayCaveLevelMusic()
    {
        Debug.Log("Playing cave level music");
        PlayMusic(caveLevelMusic);
    }

    public void PlayVillageLevelMusic()
    {
        Debug.Log("Playing village level music");
        PlayMusic(villageLevelMusic);
    }

    public void PlayForestLevelMusic()
    {
        Debug.Log("Playing forest level music");
        PlayMusic(forestLevelMusic);
    }

    public void PlayCastleLevelMusic()
    {
        Debug.Log("Playing castle level music");
        PlayMusic(castleLevelMusic);
    }

    public void PlayBossLevelMusic()
    {
        Debug.Log("Playing boss level music");
        PlayMusic(bossLevelMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        // Stop current music
        audioSource.Stop();

        // Set and play new music clip
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("No music clip assigned for this scene!");
        }
    }

    // Additional utility methods
    public void StopMusic()
    {
        audioSource.Stop();
    }

    public void PauseMusic()
    {
        audioSource.Pause();
    }

    public void ResumeMusic()
    {
        audioSource.UnPause();
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}