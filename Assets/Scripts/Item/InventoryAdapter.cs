using UnityEngine;

// This class adapts SimpleInventoryManager to the IInventory interface
// so we can use it with our door system without modifying the door code
public class InventoryAdapter : MonoBehaviour, IInventory
{
    private SimpleInventoryManager simpleInventory;
    
    public void Initialize(SimpleInventoryManager inventory)
    {
        this.simpleInventory = inventory;
    }
    
    public void AddKey(string keyId, Sprite keyIcon)
    {
        if (simpleInventory != null)
        {
            simpleInventory.AddKey(keyId, keyIcon);
        }
        else
        {
            Debug.LogError("InventoryAdapter: SimpleInventoryManager reference is null!");
        }
    }
    
    public bool UseKey(string keyId)
    {
        if (simpleInventory != null)
        {
            return simpleInventory.UseKey(keyId);
        }
        else
        {
            Debug.LogError("InventoryAdapter: SimpleInventoryManager reference is null!");
            return false;
        }
    }
    
    public bool HasKey(string keyId)
    {
        if (simpleInventory != null)
        {
            return simpleInventory.HasKey(keyId);
        }
        else
        {
            Debug.LogError("InventoryAdapter: SimpleInventoryManager reference is null!");
            return false;
        }
    }
} 