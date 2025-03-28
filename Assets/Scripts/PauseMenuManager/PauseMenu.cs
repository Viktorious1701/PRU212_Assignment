using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused = false;

    [Header("Pause Menu References")]
    public GameObject pauseMenuUI;
    public Button resumeButton;
    public Button mainMenuButton;
    public Button quitButton;

    void Start()
    {
        // Ensure pause menu is hidden when scene starts
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        // Add listeners to buttons
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(LoadMainMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape key pressed!");
            if (IsPaused)
            {
                Debug.Log("Attempting to resume");
                ResumeGame();
            }
            else
            {
                Debug.Log("Attempting to pause");
                PauseGame();
            }
        }
    }

    void PauseGame()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            IsPaused = true;
        }
        else
        {
            Debug.LogError("Pause Menu UI reference is null!");
        }
    }

    public void ResumeGame()
    {
        // Resume the game
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Unfreeze game time
        IsPaused = false;
    }

    public void LoadMainMenu()
    {
        // Ensure game time is unfrozen when loading new scene
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu"); // Replace with your main menu scene name
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
