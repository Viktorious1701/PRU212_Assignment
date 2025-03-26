using UnityEngine;

// Interface for inventory functionality used by the door system
public interface IInventory
{
    void AddKey(string keyId, Sprite keyIcon);
    bool UseKey(string keyId);
    bool HasKey(string keyId);
} 