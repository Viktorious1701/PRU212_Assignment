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
    
    private void Start()
    {
        if (showDebugLogs)
        {
            Debug.Log($"SimpleInventoryManager initialized on {gameObject.name}");
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
} 