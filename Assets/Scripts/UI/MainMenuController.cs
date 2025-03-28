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
    [SerializeField] private Image backgroundImage; // For fading to black
    [SerializeField] private float spinDuration = 1f; // Duration of spin
    [SerializeField] private float fadeDuration = 1f; // Duration of fade to black

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
        SceneManager.LoadScene("SCENE1_Cave");
    }


    private void LoadSceneLevel2()
    {
        SceneManager.LoadScene("SCENE2_Village");
    }

    private void LoadSceneLevel3()
    {
        SceneManager.LoadScene("SCENE3_Forest");
    }

    private void LoadSceneLevel4()
    {
        SceneManager.LoadScene("SCENE4_Castle");
    }

    private void LoadSceneLevel5()
    {
        SceneManager.LoadScene("SCENE5_BOSS");
    }

    private void OnPlayButtonClicked()
    {
        // Disable the play button to prevent multiple clicks
        playButton.interactable = false;

        // Start coroutine for transition
        StartCoroutine(PlayButtonTransition());
    }
    [Header("Music Control")]
    [SerializeField] private MenuMusicController musicController;
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
        yield return StartCoroutine(FadeToBlack());

        // Wait for spin to complete
        yield return spinCoroutine;
        // Stop the music before loading the scene (optional)
        if (musicController != null)
        {
            musicController.StopMusic();
        }


        // Load the first scene
        SceneManager.LoadScene("SCENE3_Forest");
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

    private IEnumerator FadeToBlack()
    {
        // Ensure background image is assigned
        if (backgroundImage == null)
        {
            Debug.LogWarning("Background image not assigned for fade effect!");
            yield break;
        }

        // Start with transparent black
        Color startColor = new Color(0, 0, 0, 0);
        Color endColor = new Color(0, 0, 0, 1);

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            // Interpolate the color alpha
            backgroundImage.color = Color.Lerp(startColor, endColor, elapsedTime / fadeDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it's fully black at the end
        backgroundImage.color = endColor;
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