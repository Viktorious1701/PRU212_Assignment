using UnityEngine;
using System.Collections;

// Speed boost potion that increases player movement speed temporarily
public class SpeedPotion : LootItem
{
    [Header("Speed Boost Settings")]
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float duration = 10f;
    [SerializeField] private GameObject speedEffectPrefab; // Optional particle effect to show while active

    protected override bool ApplyEffect(GameObject player)
    {
        PlayerBuffManager buffManager = player.GetComponent<PlayerBuffManager>();
        if (buffManager != null)
        {
            buffManager.ApplySpeedBuff(speedMultiplier, duration, speedEffectPrefab);
            return true;
        }
        return false;
    }
}
