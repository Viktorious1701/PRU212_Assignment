using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button retryButton;
    [SerializeField] private string gameOverMessage = "GAME OVER\nThe barrel fell off!";
    
    [Header("References")]
    [SerializeField] private BarrelFallDetector barrelFallDetector;
    
    private void Awake()
    {
        // Hide the UI initially
        gameObject.SetActive(false);
        
        // Add button listener
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }
        
        // Set game over text
        if (gameOverText != null)
        {
            gameOverText.text = gameOverMessage;
        }
        
        // Find the barrel fall detector if not assigned
        if (barrelFallDetector == null)
        {
            GameObject barrel = GameObject.FindGameObjectWithTag("Barrel");
            if (barrel != null)
            {
                barrelFallDetector = barrel.GetComponent<BarrelFallDetector>();
                Debug.Log("Found barrel detector through tag");
            }
            else
            {
                // Try to find by component type
                barrelFallDetector = FindObjectOfType<BarrelFallDetector>();
                if (barrelFallDetector != null)
                {
                    Debug.Log("Found barrel detector by type");
                }
            }
        }
    }
    
    // Public method that will be visible in the Unity inspector
    public void OnRetryButtonClick()
    {
        Debug.Log("Retry button clicked!");
        
        // Call the reset method on the barrel detector
        if (barrelFallDetector != null)
        {
            barrelFallDetector.ResetGame();
        }
        else
        {
            Debug.LogError("BarrelFallDetector reference not set in GameOverDisplay!");
            // Fallback to reloading the scene if barrel reference is missing
            ReloadScene();
        }
    }
    
    private void OnRetryButtonClicked()
    {
        // Call the public method
        OnRetryButtonClick();
    }
    
    private void ReloadScene()
    {
        // Restore normal time scale first
        Time.timeScale = 1f;
        
        // Get current scene index and reload it
        int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex);
    }
} 