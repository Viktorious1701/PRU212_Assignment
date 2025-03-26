using UnityEngine;

public class KeyItem : LootItem
{
    [Header("Key Settings")]
    [SerializeField] private string keyId = "default_key";
    [SerializeField] private Sprite keyIcon;
    [SerializeField] private KeyType keyType = KeyType.Automatic;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool playerInRange = false;
    private GameObject currentPlayer;
    private bool isCollected = false;

    public enum KeyType
    {
        Automatic,  // Collect on contact (original LootItem behavior)
        Interactive // Requires button press to collect
    }
    
    protected override void Start()
    {
        base.Start();
        
        // Override the pickup delay to be shorter for keys
        pickupDelay = 0.1f;
        canBeCollected = true;
        
        if (showDebugLogs)
        {
            Debug.Log($"KeyItem initialized with keyId: {keyId}, type: {keyType}");
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        // For key items that require button press to collect
        if (keyType == KeyType.Interactive && playerInRange && canBeCollected)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                if (ApplyEffect(currentPlayer))
                {
                    isCollected = true;
                    PlayCollectEffect();
                    Destroy(gameObject);
                }
            }
        }
    }

    protected override void OnCollisionEnter2D(Collision2D other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Key collision with: {other.gameObject.name}, tag: {other.gameObject.tag}, canBeCollected: {canBeCollected}");
        }
        
        // Only handle collision for automatic keys
        if (keyType == KeyType.Automatic)
        {
            // Make sure we can collect
            canBeCollected = true;
            
            if (other.gameObject.CompareTag("Player"))
            {
                if (ApplyEffect(other.gameObject))
                {
                    isCollected = true;
                    PlayCollectEffect();
                    Destroy(gameObject);
                }
            }
        }
        else if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = true;
            currentPlayer = other.gameObject;
        }
    }
    
    // Add trigger support for more flexibility
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Key trigger with: {other.gameObject.name}, tag: {other.gameObject.tag}, canBeCollected: {canBeCollected}");
        }
        
        // Force collection to be enabled
        canBeCollected = true;
        
        // For automatic collection
        if (keyType == KeyType.Automatic && other.CompareTag("Player"))
        {
            if (ApplyEffect(other.gameObject))
            {
                isCollected = true;
                PlayCollectEffect();
                Destroy(gameObject);
            }
        }
        // For interactive collection, just mark player in range
        else if (keyType == KeyType.Interactive && other.CompareTag("Player"))
        {
            playerInRange = true;
            currentPlayer = other.gameObject;
        }
    }
    
    protected virtual void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && keyType == KeyType.Interactive)
        {
            playerInRange = false;
            currentPlayer = null;
        }
    }
    
    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && keyType == KeyType.Interactive)
        {
            playerInRange = false;
            currentPlayer = null;
        }
    }

    protected override bool ApplyEffect(GameObject player)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Trying to apply key effect to player: {player.name}");
        }
        
        // Try SimpleInventoryManager first (most likely to work based on our tests)
        SimpleInventoryManager simpleInventory = player.GetComponent<SimpleInventoryManager>();
        if (simpleInventory != null)
        {
            simpleInventory.AddKey(keyId, keyIcon);
            if (showDebugLogs)
            {
                Debug.Log($"Key '{keyId}' collected (via SimpleInventoryManager)");
            }
            return true;
        }
        
        // Try interfaces and other implementations as fallback
        IInventory inventory = player.GetComponent<IInventory>();
        if (inventory != null)
        {
            inventory.AddKey(keyId, keyIcon);
            if (showDebugLogs)
            {
                Debug.Log($"Key '{keyId}' collected (via IInventory)");
            }
            return true;
        }
        
        // Last resort: try InventoryManager
        InventoryManager invManager = player.GetComponent<InventoryManager>();
        if (invManager != null)
        {
            invManager.AddKey(keyId, keyIcon);
            if (showDebugLogs)
            {
                Debug.Log($"Key '{keyId}' collected (via InventoryManager)");
            }
            return true;
        }
        
        // If we get here, we couldn't find any inventory
        if (showDebugLogs)
        {
            Debug.LogError($"Player has no inventory component! Add SimpleInventoryManager to {player.name}");
            
            // Auto-add SimpleInventoryManager as last resort
            simpleInventory = player.AddComponent<SimpleInventoryManager>();
            simpleInventory.AddKey(keyId, keyIcon);
            Debug.Log($"Added SimpleInventoryManager to player and collected key '{keyId}'");
            return true;
        }
        
        return false;
    }
    
    // Add this method to support the DoorActivation.cs script
    public bool IsCollected()
    {
        return isCollected;
    }
} 