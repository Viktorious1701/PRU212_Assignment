using UnityEngine;
using System.Collections.Generic;

public class SimpleInventoryManager : MonoBehaviour, IInventory
{
    [System.Serializable]
    public class Key
    {
        public string keyId;
        public Sprite keyIcon;
    }
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Inventory")]
    [SerializeField] private List<Key> keys = new List<Key>();
    
    // Session ID used to track game restarts
    private static string SESSION_ID_KEY = "InventorySessionID";
    private string currentSessionId;
    
    private void Awake()
    {
        // Generate a new session ID for this playthrough
        string savedSessionId = PlayerPrefs.GetString(SESSION_ID_KEY, "");
        currentSessionId = System.Guid.NewGuid().ToString();
        
        if (showDebugLogs)
        {
            Debug.Log($"Previous session ID: {savedSessionId}");
            Debug.Log($"New session ID: {currentSessionId}");
        }
        
        // If session IDs don't match, we have a new game session
        if (string.IsNullOrEmpty(savedSessionId) || savedSessionId != currentSessionId)
        {
            // Clear inventory since we're in a new session
            if (keys.Count > 0)
            {
                Debug.LogWarning("Detected new game session. Clearing inventory.");
                keys.Clear();
            }
        }
        
        // Save the new session ID
        PlayerPrefs.SetString(SESSION_ID_KEY, currentSessionId);
        PlayerPrefs.Save();
    }
    
    private void Start()
    {
        if (showDebugLogs)
        {
            Debug.Log($"SimpleInventoryManager initialized on {gameObject.name}");
            Debug.Log($"Current inventory has {keys.Count} keys at game start");
        }
    }
    
    public void AddKey(string keyId, Sprite keyIcon)
    {
        // Check if we already have this key
        if (!HasKey(keyId))
        {
            Key newKey = new Key { keyId = keyId, keyIcon = keyIcon };
            keys.Add(newKey);
            
            if (showDebugLogs)
            {
                Debug.Log($"Added key {keyId} to inventory. Total keys: {keys.Count}");
            }
        }
        else if (showDebugLogs)
        {
            Debug.Log($"Already have key {keyId} in inventory");
        }
    }
    
    public bool UseKey(string keyId)
    {
        // Find and remove the key
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].keyId == keyId)
            {
                keys.RemoveAt(i);
                
                if (showDebugLogs)
                {
                    Debug.Log($"Used key {keyId}. Remaining keys: {keys.Count}");
                }
                
                return true;
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"Tried to use key {keyId} but don't have it");
        }
        
        return false;
    }
    
    public bool HasKey(string keyId)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Checking for key '{keyId}' in inventory with {keys.Count} keys:");
            foreach (Key key in keys)
            {
                Debug.Log($"  - Key in inventory: '{key.keyId}'");
                // Check for exact string match issues
                if (key.keyId == keyId)
                {
                    Debug.Log($"    EXACT MATCH with '{keyId}'");
                }
                else
                {
                    Debug.Log($"    NO MATCH with '{keyId}'. Are they exactly the same?");
                    // Check for whitespace or case issues
                    if (key.keyId.Trim() == keyId.Trim())
                    {
                        Debug.Log($"    MATCH AFTER TRIMMING whitespace");
                    }
                    if (key.keyId.ToLower() == keyId.ToLower())
                    {
                        Debug.Log($"    MATCH AFTER IGNORING case");
                    }
                }
            }
        }

        foreach (Key key in keys)
        {
            if (key.keyId == keyId)
            {
                return true;
            }
        }
        
        return false;
    }
    
    // Debug method to manually add keys via the Inspector
    public void AddKeyFromInspector(string keyId)
    {
        AddKey(keyId, null);
    }
    
    // Clear the entire inventory
    public void ClearInventory()
    {
        int keyCount = keys.Count;
        keys.Clear();
        
        if (showDebugLogs)
        {
            Debug.Log($"Cleared inventory. Removed {keyCount} keys.");
        }
    }
    
    // Clear inventory when application is quit
    private void OnApplicationQuit()
    {
        if (showDebugLogs)
        {
            Debug.Log("Application is quitting. Clearing inventory.");
        }
        
        ClearInventory();
        
        // Clear the session ID to force a reset on next game launch
        PlayerPrefs.DeleteKey(SESSION_ID_KEY);
        PlayerPrefs.Save();
    }
    
    // Also clear when scene is unloaded
    private void OnDestroy()
    {
        if (showDebugLogs)
        {
            Debug.Log("SimpleInventoryManager is being destroyed. Clearing inventory.");
        }
        
        ClearInventory();
    }
} 