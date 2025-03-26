using UnityEngine;

public abstract class DoorBase : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] protected string requiredKeyId = "default_key";
    [SerializeField] protected bool destroyKeyOnUse = true;
    [SerializeField] protected float interactionDistance = 2f;
    [SerializeField] protected bool isLocked = true;
    
    [Header("Visual and Audio")]
    [SerializeField] protected GameObject lockedEffect;
    [SerializeField] protected GameObject unlockEffect;
    [SerializeField] protected AudioClip lockedSound;
    [SerializeField] protected AudioClip unlockSound;
    [SerializeField] protected AudioClip openSound;
    
    [Header("Interaction UI")]
    [SerializeField] protected GameObject interactionPrompt;
    
    [Header("Debug")]
    [SerializeField] protected bool showDebugLogs = true;
    
    protected bool isOpen = false;
    protected bool isAnimating = false;
    protected IInventory playerInventory;
    protected Transform player;

    protected virtual void Start()
    {
        // Find player and inventory
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            
            // Try SimpleInventoryManager first (since we know it works from debug logs)
            SimpleInventoryManager simpleInventory = playerObj.GetComponent<SimpleInventoryManager>();
            if (simpleInventory != null)
            {
                // Use SimpleInventoryManager directly
                playerInventory = simpleInventory;
                
                if (showDebugLogs)
                {
                    Debug.Log($"Using SimpleInventoryManager directly on {playerObj.name}");
                }
            }
            else
            {
                // If no SimpleInventoryManager, try regular InventoryManager
                playerInventory = playerObj.GetComponent<SimpleInventoryManager>();
                
                if (playerInventory == null && showDebugLogs)
                {
                    Debug.LogError($"Player has no inventory components on {playerObj.name}");
                }
            }
        }
        else if (showDebugLogs)
        {
            Debug.LogError("No player found with tag: Player");
        }
        
        // Hide the interaction prompt initially
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Additional initialization
        InitializeDoor();
    }
    
    protected virtual void Update()
    {
        // Check if player is nearby
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Show or hide interaction prompt based on distance
            if (distanceToPlayer <= interactionDistance && !isOpen && !isAnimating)
            {
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(true);
                }
                
                // Check for interaction key press (E by default)
                if (Input.GetKeyDown(KeyCode.E))
                {
                    TryOpenDoor();
                }
            }
            else if (interactionPrompt != null && interactionPrompt.activeSelf)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
    
    public void TryOpenDoor()
    {
        // If the door is already open or animating, do nothing
        if (isOpen || isAnimating) return;
        
        // Debug log to help diagnose issues
        if (showDebugLogs)
        {
            Debug.Log($"Trying to open door '{gameObject.name}'");
            Debug.Log($"  Door is locked: {isLocked}");
            Debug.Log($"  Required key ID: {requiredKeyId}");
            
            if (playerInventory != null)
            {
                bool hasKey = playerInventory.HasKey(requiredKeyId);
                Debug.Log($"  Player has key: {hasKey}");
                // Add more detailed debug info
                Debug.Log($"  Inventory type: {playerInventory.GetType().Name}");
                Debug.Log($"  Exact check: requiredKeyId='{requiredKeyId}', comparing as string");
            }
            else
            {
                Debug.LogError("  No player inventory found!");
            }
        }
        
        // If the door is locked, check for key
        if (isLocked)
        {
            bool hasKey = playerInventory != null && playerInventory.HasKey(requiredKeyId);
            if (showDebugLogs)
            {
                Debug.Log($"Key check details - inventory: {(playerInventory != null ? playerInventory.GetType().Name : "null")}, hasKey result: {hasKey}");
            }
            
            if (hasKey)
            {
                // Unlock the door
                isLocked = false;
                
                // Always remove key from inventory when used
                playerInventory.UseKey(requiredKeyId);
                
                // Play unlock effects
                PlayUnlockEffects();
                
                // Open the door after unlocking
                OpenDoor();
            }
            else
            {
                // Play locked effects (door is locked and player doesn't have the key)
                PlayLockedEffects();
            }
        }
        else
        {
            // Door is unlocked, open it directly
            OpenDoor();
        }
    }
    
    // Abstract methods to be implemented by specific door types
    protected abstract void InitializeDoor();
    protected abstract void OpenDoor();
    
    protected virtual void PlayLockedEffects()
    {
        // Play locked sound
        if (lockedSound != null)
        {
            AudioSource.PlayClipAtPoint(lockedSound, transform.position);
        }
        
        // Show locked visual effect
        if (lockedEffect != null)
        {
            Instantiate(lockedEffect, transform.position, Quaternion.identity);
        }
        
        if (showDebugLogs)
        {
            Debug.Log("Door is locked and requires key: " + requiredKeyId);
        }
    }
    
    protected virtual void PlayUnlockEffects()
    {
        // Play unlock sound
        if (unlockSound != null)
        {
            AudioSource.PlayClipAtPoint(unlockSound, transform.position);
        }
        
        // Show unlock visual effect
        if (unlockEffect != null)
        {
            Instantiate(unlockEffect, transform.position, Quaternion.identity);
        }
        
        if (showDebugLogs)
        {
            Debug.Log("Door unlocked with key: " + requiredKeyId);
        }
    }
    
    protected virtual void PlayOpenSound()
    {
        // Play open sound
        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
    }
} 