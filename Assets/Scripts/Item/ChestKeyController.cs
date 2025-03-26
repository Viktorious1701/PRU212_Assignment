using UnityEngine;

public class ChestKeyController : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private string keyId = "1";
    [SerializeField] private Sprite keyIcon;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private bool destroyOnPickup = true;
    
    [Header("Visual & Audio")]
    [SerializeField] private GameObject collectEffectPrefab;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private string collectAnimTrigger = "Open";
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool isCollected = false;
    private bool playerInRange = false;
    private GameObject currentPlayer;
    private SimpleInventoryManager playerInventory;
    
    void Start()
    {
        // If no animator assigned, try to get it
        if (chestAnimator == null)
        {
            chestAnimator = GetComponent<Animator>();
        }
        
        // Make sure we have a collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            if (showDebugLogs)
            {
                Debug.Log("Added BoxCollider2D to chest key");
            }
        }
    }
    
    void Update()
    {
        // Check for player interaction
        if (!isCollected && playerInRange && Input.GetKeyDown(interactKey))
        {
            CollectKey();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Chest trigger with: {other.gameObject.name}, tag: {other.gameObject.tag}");
        }
        
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            currentPlayer = other.gameObject;
            
            // Get player inventory
            playerInventory = currentPlayer.GetComponent<SimpleInventoryManager>();
            if (playerInventory == null)
            {
                if (showDebugLogs)
                {
                    Debug.LogWarning("Player has no SimpleInventoryManager - adding one");
                }
                playerInventory = currentPlayer.AddComponent<SimpleInventoryManager>();
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            currentPlayer = null;
        }
    }
    
    public void CollectKey()
    {
        if (isCollected) return;
        
        if (currentPlayer != null && playerInventory != null)
        {
            // Add key to player inventory
            playerInventory.AddKey(keyId, keyIcon);
            
            // Mark as collected
            isCollected = true;
            
            // Play animation if available
            if (chestAnimator != null)
            {
                chestAnimator.SetTrigger(collectAnimTrigger);
            }
            
            // Play collection effect
            PlayCollectEffect();
            
            if (showDebugLogs)
            {
                Debug.Log($"Chest key '{keyId}' collected and added to inventory");
            }
            
            // Destroy object if configured
            if (destroyOnPickup)
            {
                // Delay destruction to allow animation to play
                Destroy(gameObject, 0.5f);
            }
        }
        else if (showDebugLogs)
        {
            Debug.LogError("Cannot collect key - player or inventory is null");
        }
    }
    
    private void PlayCollectEffect()
    {
        // Spawn effect if provided
        if (collectEffectPrefab != null)
        {
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Play sound if provided
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
    }
    
    // For external scripts to check
    public bool IsCollected()
    {
        return isCollected;
    }
} 