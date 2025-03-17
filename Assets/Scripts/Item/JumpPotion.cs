using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPotion : LootItem
{
    [Header("Jump Boost Settings")]
    [SerializeField] private float jumpMultiplier = 1.4f;
    [SerializeField] private float duration = 10f;
    [SerializeField] private GameObject jumpEffectPrefab; // Optional particle effect to show while active

    protected override bool ApplyEffect(GameObject player)
    {
        PlayerBuffManager buffManager = player.GetComponent<PlayerBuffManager>();
        if (buffManager != null)
        {
            buffManager.ApplyJumpBuff(jumpMultiplier, duration, jumpEffectPrefab);
            return true;
        }
        return false;
    }
}
