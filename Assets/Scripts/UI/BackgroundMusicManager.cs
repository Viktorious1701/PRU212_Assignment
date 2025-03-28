using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance { get; private set; }
    private AudioSource audioSource;

    [Header("Level Music Clips")]
    public AudioClip menuMusicClip;
    public AudioClip caveLevelMusic;
    public AudioClip villageLevelMusic;
    public AudioClip forestLevelMusic;
    public AudioClip castleLevelMusic;
    public AudioClip bossLevelMusic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayMusicForCurrentScene();
    }

    private void PlayMusicForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "Main Menu":
                PlayMenuMusic();
                break;
            case "SCENE1_Cave":
                PlayCaveLevelMusic();
                break;
            case "SCENE1.1_Dungeon":
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }

    private void PlayMusic(AudioClip clip, string sceneName)
    {
        if (clip == null)
        {
            Debug.LogWarning($"No music clip assigned for {sceneName}");
            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void PlayMenuMusic() => PlayMusic(menuMusicClip, "Menu");
    public void PlayCaveLevelMusic() => PlayMusic(caveLevelMusic, "Cave");
    public void PlayVillageLevelMusic() => PlayMusic(villageLevelMusic, "Village");
    public void PlayForestLevelMusic() => PlayMusic(forestLevelMusic, "Forest");
    public void PlayCastleLevelMusic() => PlayMusic(castleLevelMusic, "Castle");
    public void PlayBossLevelMusic() => PlayMusic(bossLevelMusic, "Boss");

    // Utility methods
    public void StopMusic() => audioSource.Stop();
    public void PauseMusic() => audioSource.Pause();
    public void ResumeMusic() => audioSource.UnPause();
    public void SetVolume(float volume) => audioSource.volume = Mathf.Clamp01(volume);

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}