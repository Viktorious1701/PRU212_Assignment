using UnityEngine;
using UnityEngine.SceneManagement;

public class WheelbarrowFallDetector : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fallThreshold = -10f; // Y position considered "fallen off"
    [SerializeField] private float resetDelay = 1.5f; // Delay before resetting the scene
    [SerializeField] private GameObject gameOverUI; // Optional UI to show when game over
    
    [Header("Reset Positions")]
    [SerializeField] private Vector3 playerStartPosition = Vector3.zero; // Starting position for player
    [SerializeField] private Vector3 wheelbarrowStartPosition = new Vector3(2, 0, 0); // Starting position for wheelbarrow
    
    private bool hasFallen = false;
    
    private void Start()
    {
        // Add startup debug message
        Debug.Log("WheelbarrowFallDetector initialized on " + gameObject.name + " | Current Y: " + transform.position.y + " | Threshold: " + fallThreshold);
    }
    
    private void Update()
    {
        // Log position periodically to check if script is running
        if (Time.frameCount % 300 == 0)
        {
            Debug.Log("Wheelbarrow position: " + transform.position + " | Fall threshold: " + fallThreshold);
        }
        
        // Check if wheelbarrow has fallen below the threshold
        if (!hasFallen && transform.position.y < fallThreshold)
        {
            Debug.LogWarning("FALL DETECTED! Y position: " + transform.position.y);
            GameOver();
        }
    }
    
    private void GameOver()
    {
        hasFallen = true;
        Debug.LogError("Wheelbarrow fell off! Game Over!"); // Using LogError for higher visibility
        
        // Show game over UI if assigned
        if (gameOverUI != null)
        {
            Debug.Log("Showing Game Over UI");
            gameOverUI.SetActive(true);
            
            // Freeze the game instead of auto-resetting
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogWarning("No Game Over UI assigned! Auto-resetting after delay.");
            // If no UI, reset the scene after delay
            Invoke("ResetScene", resetDelay);
        }
    }
    
    private void ResetScene()
    {
        // Get current scene index and reload it
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log("Reloading scene: " + currentSceneIndex);
        SceneManager.LoadScene(currentSceneIndex);
    }
    
    // This can be called from the UI button
    public void ResetGame()
    {
        Debug.Log("Resetting game from WheelbarrowFallDetector");
        
        // Restore normal time speed
        Time.timeScale = 1f;
        
        // Reset player to starting position instead of reloading scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = playerStartPosition;
            Debug.Log("Reset player position to: " + playerStartPosition);
        }
        else
        {
            Debug.LogWarning("Player not found! Can't reset position.");
        }
        
        // Reset wheelbarrow position
        transform.position = wheelbarrowStartPosition;
        Debug.Log("Reset wheelbarrow position to: " + wheelbarrowStartPosition);
        
        // Reset any physics
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            Debug.Log("Reset wheelbarrow physics");
        }
        
        // Hide game over UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
            Debug.Log("Hidden game over UI");
        }
        
        // Reset fallen state
        hasFallen = false;
    }
} 