using UnityEngine;
using UnityEngine.SceneManagement;

public class BarrelFallDetector : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject gameOverUI; // UI to show when game over
    [SerializeField] private WheelbarrowFallDetector wheelbarrowDetector; // Reference to wheelbarrow for reset functionality
    [SerializeField] private string groundTag = "Ground"; // Tag for ground objects
    
    [Header("Reset Positions")]
    [SerializeField] private Vector3 playerStartPosition = new Vector3(-36.22f, -0.2f, 0);
    [SerializeField] private Vector3 wheelbarrowStartPosition = new Vector3(7.44f, 1.05f, 0);
    [SerializeField] private Vector3 barrelStartPosition = new Vector3(6.3f, 1.03f, 0);
    [SerializeField] private GameObject wheelbarrowObject; // The wheelbarrow object
    
    [Header("Prefab References")]
    [SerializeField] private GameObject wheelbarrowPrefab; // Prefab to spawn for the wheelbarrow
    
    private bool hasFallen = false;
    private Vector3 originalBarrelPosition;
    
    // Static instance for easy access
    private static BarrelFallDetector instance;
    
    private void Awake()
    {
        // Set up singleton instance
        instance = this;
    }
    
    private void Start()
    {
        Debug.Log("BarrelFallDetector initialized on " + gameObject.name);
        
        // Store original barrel position if not set
        if (barrelStartPosition == Vector3.zero)
        {
            originalBarrelPosition = transform.position;
        }
        else
        {
            originalBarrelPosition = barrelStartPosition;
        }
        
        // Find wheelbarrow detector if not assigned
        if (wheelbarrowDetector == null)
        {
            GameObject wheelbarrow = GameObject.FindGameObjectWithTag("Wheelbarrow");
            if (wheelbarrow != null)
            {
                wheelbarrowDetector = wheelbarrow.GetComponent<WheelbarrowFallDetector>();
                wheelbarrowObject = wheelbarrow;
                
                // Auto-assign the prefab if needed
                if (wheelbarrowPrefab == null)
                {
                    wheelbarrowPrefab = wheelbarrow;
                }
                
                Debug.Log("Found wheelbarrow detector: " + (wheelbarrowDetector != null));
            }
        }
        else if (wheelbarrowObject == null)
        {
            wheelbarrowObject = wheelbarrowDetector.gameObject;
            
            // Auto-assign the prefab if needed
            if (wheelbarrowPrefab == null)
            {
                wheelbarrowPrefab = wheelbarrowObject;
            }
        }
    }
    
    // Detect collision with ground
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasFallen && collision.gameObject.CompareTag(groundTag))
        {
            Debug.LogWarning("Barrel touched ground! Game Over!");
            GameOver();
        }
    }
    
    // Alternative using trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasFallen && other.CompareTag(groundTag))
        {
            Debug.LogWarning("Barrel entered ground trigger! Game Over!");
            GameOver();
        }
    }
    
    private void GameOver()
    {
        hasFallen = true;
        Debug.LogError("Barrel fell off! Game Over!");
        
        // Show game over UI if assigned
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            
            // Freeze the game
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogWarning("No Game Over UI assigned!");
        }
    }
    
    // Can be called from Retry button
    public void ResetGame()
    {
        Debug.Log("Resetting game from BarrelFallDetector");
        
        // Restore normal time scale
        Time.timeScale = 1f;
        
        // First disconnect any spring joints between player and wheelbarrow
        DisconnectPlayerFromWheelbarrow();
        
        // Reset player position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = playerStartPosition;
            
            // Reset player velocity if it has a Rigidbody2D
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
            }
            
            Debug.Log("Reset player to: " + playerStartPosition);
        }
        
        // Reset barrel position (this object)
        transform.position = originalBarrelPosition;
        
        // Reset barrel velocity
        Rigidbody2D barrelRb = GetComponent<Rigidbody2D>();
        if (barrelRb != null)
        {
            barrelRb.velocity = Vector2.zero;
            barrelRb.angularVelocity = 0f;
        }
        
        Debug.Log("Reset barrel to: " + originalBarrelPosition);
        
        // Store reference to old wheelbarrow
        GameObject oldWheelbarrow = wheelbarrowObject;
        
        if (wheelbarrowPrefab != null)
        {
            Debug.Log("Spawning new wheelbarrow at: " + wheelbarrowStartPosition);
            GameObject newWheelbarrow = Instantiate(wheelbarrowPrefab, wheelbarrowStartPosition, Quaternion.identity);
            
            // Update references
            wheelbarrowObject = newWheelbarrow;
            wheelbarrowDetector = newWheelbarrow.GetComponent<WheelbarrowFallDetector>();
            
            // Ensure the new wheelbarrow has the right tag
            newWheelbarrow.tag = "Wheelbarrow";
            
            Debug.Log("New wheelbarrow spawned successfully: " + (newWheelbarrow != null));
        }
        else
        {
            Debug.LogError("No wheelbarrow prefab assigned, can't spawn a new one!");
        }
        
        // Now destroy old wheelbarrow AFTER creating the new one
        if (oldWheelbarrow != null)
        {
            Debug.Log("Destroying old wheelbarrow");
            Destroy(oldWheelbarrow);
        }
        
        // Hide game over UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
        
        // Reset fallen state
        hasFallen = false;
    }
    
    // Helper method to disconnect player from wheelbarrow before resetting
    private void DisconnectPlayerFromWheelbarrow()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Find and destroy any SpringJoint2D components on the player
            SpringJoint2D[] joints = player.GetComponents<SpringJoint2D>();
            if (joints != null && joints.Length > 0)
            {
                foreach (SpringJoint2D joint in joints)
                {
                    Debug.Log("Destroying spring joint on player");
                    Destroy(joint);
                }
            }
        }
    }
    
    // Static method to find the barrel detector and reset the game
    // This can be called directly from UI buttons without needing a reference
    public static void StaticResetGame()
    {
        Debug.Log("Static Reset Game called");
        
        // First, make sure time is running
        Time.timeScale = 1f;
        
        // Use the instance if available
        if (instance != null)
        {
            instance.ResetGame();
            return;
        }
        
        // Otherwise try to find the barrel detector
        BarrelFallDetector detector = FindObjectOfType<BarrelFallDetector>();
        if (detector != null)
        {
            detector.ResetGame();
            return;
        }
        
        // If no barrel detector, try to disconnect any joints manually
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            SpringJoint2D[] joints = player.GetComponents<SpringJoint2D>();
            foreach (SpringJoint2D joint in joints)
            {
                Debug.Log("Destroying spring joint on player (fallback)");
                Destroy(joint);
            }
        }
        
        // If all else fails, just reload the scene
        Debug.LogWarning("No barrel detector found, reloading scene instead");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
} 