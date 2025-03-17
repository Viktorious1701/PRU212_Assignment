using UnityEngine;

public class HealthPotion : LootItem
{
    [Header("Healing Settings")]
    [SerializeField] private float healAmount = 25f;
    [SerializeField] private bool percentageHealing = false;
    
    protected override bool ApplyEffect(GameObject player)
    {

        // Check if this is the player
        Health playerHealth = player.GetComponent<Health>();

        if (playerHealth != null)
        {
            // Calculate healing amount
            float actualHealAmount = healAmount;

            if (percentageHealing)
            {
                // Calculate percentage of max health
                actualHealAmount = playerHealth.GetMaxHealth() * (healAmount / 100f);
            }

            // Apply healing - we're using the negative of damage amount to heal
            float currentHealth = playerHealth.GetCurrentHealth();
            float maxHealth = playerHealth.GetMaxHealth();

            // Only heal if not at max health
            if (currentHealth < maxHealth)
            {
                // Using TakeDamage with negative value to heal
                playerHealth.TakeDamage(-actualHealAmount);

                // Play collection effect
                PlayCollectEffect();

                // Destroy the potion
                Destroy(gameObject);
            }
            return true;
        }

        return false;
    }
}