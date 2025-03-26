using UnityEngine;
using System.Collections;

// Damage boost potion that increases player damage output temporarily
public class DamagePotion : LootItem
{
    [Header("Damage Boost Settings")]
    [SerializeField] private float damageMultiplier = 1.5f;
    [SerializeField] private float duration = 12f;
    [SerializeField] private GameObject damageEffectPrefab; // Effect to show while active

    protected override bool ApplyEffect(GameObject player)
    {
        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();

        if (playerCombat != null)
        {
            // Apply damage boost via the method we added
            playerCombat.ApplyDamageBoost(damageMultiplier, duration);

            // Spawn visual effect if provided
            if (damageEffectPrefab != null)
            {
                GameObject effect = Instantiate(damageEffectPrefab, player.transform);
                Destroy(effect, duration);
            }

            return true;
        }

        return false;
    }
}
