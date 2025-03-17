using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    [System.Serializable]
    public class Key
    {
        public string keyId;
        public Sprite keyIcon;
    }

    [Header("Inventory Settings")]
    [SerializeField] private int coins = 0;
    [SerializeField] private List<Key> keys = new List<Key>();

    // Events for UI updates
    public delegate void InventoryChangedHandler();
    public event InventoryChangedHandler OnCoinsChanged;
    public event InventoryChangedHandler OnKeysChanged;

    // Methods for adding/using inventory items
    public void AddCoins(int amount)
    {
        coins += amount;
        OnCoinsChanged?.Invoke();
    }

    public bool UseCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            OnCoinsChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void AddKey(string keyId, Sprite keyIcon)
    {
        // Check if we already have this key
        if (!HasKey(keyId))
        {
            Key newKey = new Key { keyId = keyId, keyIcon = keyIcon };
            keys.Add(newKey);
            OnKeysChanged?.Invoke();
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
                OnKeysChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    public bool HasKey(string keyId)
    {
        foreach (Key key in keys)
        {
            if (key.keyId == keyId)
                return true;
        }
        return false;
    }

    // Getters
    public int GetCoins() => coins;
    public List<Key> GetKeys() => keys;
}