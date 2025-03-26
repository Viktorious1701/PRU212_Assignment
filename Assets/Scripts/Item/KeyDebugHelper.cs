using UnityEngine;

public class KeyDebugHelper : MonoBehaviour
{
    [SerializeField] private KeyCode debugCollectKey = KeyCode.K;
    [SerializeField] private KeyCode debugUnlockAllDoorsKey = KeyCode.L;
    [SerializeField] private KeyCode debugClearInventoryKey = KeyCode.C;
    
    private KeyItem keyItem;
    private SimpleInventoryManager playerInventory;
    
    void Start()
    {
        // Get the KeyItem on this object
        keyItem = GetComponent<KeyItem>();
        
        // Find the player's inventory
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInventory = player.GetComponent<SimpleInventoryManager>();
            
            if (playerInventory == null)
            {
                Debug.LogError("Player does not have SimpleInventoryManager component - add it to your player!");
                
                // Add it automatically for convenience
                playerInventory = player.AddComponent<SimpleInventoryManager>();
                Debug.Log("Added SimpleInventoryManager to player automatically");
            }
            else
            {
                Debug.Log($"Found player inventory on {player.name}");
            }
        }
        else
        {
            Debug.LogError("Could not find player with 'Player' tag!");
        }
        
        // Check colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        if (colliders.Length == 0)
        {
            Debug.LogError("Key has no collider! Add a Collider2D component.");
        }
        else
        {
            Debug.Log($"Key has {colliders.Length} colliders:");
            foreach (Collider2D col in colliders)
            {
                Debug.Log($"  - {col.GetType().Name}, isTrigger: {col.isTrigger}");
            }
        }
    }
    
    void Update()
    {
        // Debug key press to manually collect
        if (Input.GetKeyDown(debugCollectKey) && playerInventory != null)
        {
            string keyId = "1"; // The key ID from your settings
            Debug.Log($"Manually adding key '{keyId}' to player inventory");
            playerInventory.AddKey(keyId, null);
        }
        
        // Debug key to directly unlock all doors
        if (Input.GetKeyDown(debugUnlockAllDoorsKey))
        {
            UnlockAllDoors();
        }
        
        // Debug key to clear inventory
        if (Input.GetKeyDown(debugClearInventoryKey) && playerInventory != null)
        {
            ClearInventory();
        }
    }
    
    public void ClearInventory()
    {
        if (playerInventory == null)
        {
            Debug.LogError("No player inventory found");
            return;
        }
        
        // Use reflection to access and clear the keys list
        System.Type inventoryType = playerInventory.GetType();
        System.Reflection.FieldInfo keysField = inventoryType.GetField("keys", 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        
        if (keysField != null)
        {
            System.Collections.IList keysList = keysField.GetValue(playerInventory) as System.Collections.IList;
            if (keysList != null)
            {
                int count = keysList.Count;
                keysList.Clear();
                Debug.Log($"Cleared {count} keys from inventory");
            }
        }
        else
        {
            // Try using direct UseKey method if field not found
            Debug.Log("Using UseKey to remove key '1'");
            playerInventory.UseKey("1");
        }
        
        // Now relock all doors
        DoorBase[] allDoors = FindObjectsOfType<DoorBase>();
        foreach (DoorBase door in allDoors)
        {
            System.Type doorType = door.GetType();
            System.Reflection.FieldInfo isLockedField = doorType.BaseType.GetField("isLocked", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
            
            if (isLockedField != null)
            {
                isLockedField.SetValue(door, true);
                Debug.Log($"Re-locked door: {door.name}");
            }
        }
    }
    
    public void UnlockAllDoors()
    {
        // Find all DoorBase objects in the scene
        DoorBase[] allDoors = FindObjectsOfType<DoorBase>();
        
        Debug.Log($"Found {allDoors.Length} doors to unlock");
        
        foreach (DoorBase door in allDoors)
        {
            // Use reflection to access and modify the protected field
            System.Type doorType = door.GetType();
            System.Reflection.FieldInfo isLockedField = doorType.BaseType.GetField("isLocked", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
            
            if (isLockedField != null)
            {
                // Unlock the door
                isLockedField.SetValue(door, false);
                Debug.Log($"Unlocked door: {door.name}");
                
                // Try to call OpenDoor method
                door.SendMessage("OpenDoor", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Key COLLISION with: {collision.gameObject.name}, tag: {collision.gameObject.tag}");
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Key TRIGGER with: {other.gameObject.name}, tag: {other.gameObject.tag}");
    }
} 