using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    [Header("Choose Menu")]
    [SerializeField] private Button level1;
    [SerializeField] private Button level2;
    [SerializeField] private Button level3;
    [SerializeField] private Button level4;
    [SerializeField] private Button level5;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip playButtonSoundEffect;

    [Header("Transition Effects")]
    [SerializeField] private Image fadeOverlay; // Renamed: This is the fade-to-black overlay, not the background
    [SerializeField] private float spinDuration = 1f; // Duration of spin
    [SerializeField] private float fadeDuration = 1f; // Duration of fade to black

    [Header("Music Control")]
    [SerializeField] private MenuMusicController musicController;

    private void Start()
    {
        // Add click listeners to buttons
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        if (level1 != null)
        {
            level1.onClick.AddListener(LoadSceneLevel1);
        }
        if (level2 != null)
        {
            level2.onClick.AddListener(LoadSceneLevel2);
        }
        if (level3 != null)
        {
            level3.onClick.AddListener(LoadSceneLevel3);
        }
        if (level4 != null)
        {
            level4.onClick.AddListener(LoadSceneLevel4);
        }
        if (level5 != null)
        {
            level5.onClick.AddListener(LoadSceneLevel5);
        }

        // Ensure AudioSource is assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // If still no AudioSource, add one
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void LoadSceneLevel1()
    {
        if (musicController != null)
        {
            musicController.StopMusic();
        }
        SceneManager.LoadScene("SCENE1_Cave");
    }

    private void LoadSceneLevel2()
    {
        if (musicController != null)
        {
            musicController.StopMusic();
        }
        SceneManager.LoadScene("SCENE2_Village");
    }

    private void LoadSceneLevel3()
    {
        if (musicController != null)
        {
            musicController.StopMusic();
        }
        SceneManager.LoadScene("SCENE3_Forest");
    }

    private void LoadSceneLevel4()
    {
        if (musicController != null)
        {
            musicController.StopMusic();
        }
        SceneManager.LoadScene("SCENE4_Castle");
    }

    private void LoadSceneLevel5()
    {
        if (musicController != null)
        {
            musicController.StopMusic();
        }
        SceneManager.LoadScene("SCENE5_BOSS");
    }

    private void OnPlayButtonClicked()
    {
        // Disable the play button to prevent multiple clicks
        playButton.interactable = false;

        // Start coroutine for transition
        StartCoroutine(PlayButtonTransition());
    }

    private IEnumerator PlayButtonTransition()
    {
        // Spin the play button
        Coroutine spinCoroutine = StartCoroutine(SpinButton());

        // Play sound effect
        if (playButtonSoundEffect != null)
        {
            audioSource.PlayOneShot(playButtonSoundEffect);
        }

        // Fade to black
        yield return StartCoroutine(FadeImagesToBlack());

        // Wait for spin to complete
        yield return spinCoroutine;

        // Stop the music before loading the scene (optional)
        if (musicController != null)
        {
            musicController.StopMusic();
        }

        // Load the first scene
        SceneManager.LoadScene("SCENE1_Cave");
    }

    private IEnumerator SpinButton()
    {
        // Get the button's RectTransform
        RectTransform buttonRectTransform = playButton.GetComponent<RectTransform>();

        // Store the start rotation
        Quaternion startRotation = buttonRectTransform.rotation;

        // Spin around Y axis
        float elapsedTime = 0f;
        while (elapsedTime < spinDuration)
        {
            // Calculate rotation around Y axis
            float yRotation = Mathf.Lerp(0, 360, elapsedTime / spinDuration);
            buttonRectTransform.rotation = Quaternion.Euler(0, yRotation, 0) * startRotation;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset to original rotation
        buttonRectTransform.rotation = startRotation;
    }

    private IEnumerator FadeImagesToBlack()
    {
        float elapsedTime = 0f;

        // Store initial colors
        Color startColorBackground = fadeOverlay.color;
        Color startColorPlay = (playButton != null) ? playButton.image.color : Color.white;
        Color endColor = Color.black; // Target color (black, fully opaque)

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;

            // Fade the background image to black
            fadeOverlay.color = Color.Lerp(startColorBackground, endColor, t);

            // Optionally fade the play button image to black
            if (playButton != null)
            {
                playButton.image.color = Color.Lerp(startColorPlay, endColor, t);
            }

            // Optionally fade other UI elements (e.g., quitButton)
            if (quitButton != null)
            {
                quitButton.image.color = Color.Lerp(startColorPlay, endColor, t);
            }

            yield return null;
        }

        // Ensure final colors are set to black
        fadeOverlay.color = endColor;
        if (playButton != null) playButton.image.color = endColor;
        if (quitButton != null) quitButton.image.color = endColor;
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}