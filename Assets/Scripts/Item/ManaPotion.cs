using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaPotion : LootItem
{
    [Header("Mana Settings")]
    [SerializeField] private float manaAmount = 30f;
    [SerializeField] private bool percentageRestore = false;

    protected override bool ApplyEffect(GameObject player)
    {
        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();

        if (playerCombat != null)
        {
            float currentMana = playerCombat.GetCurrentMana();
            float maxMana = playerCombat.GetMaxMana();

            // Only collect if not at max mana
            if (currentMana < maxMana)
            {
                // Calculate actual mana amount
                float actualManaAmount = manaAmount;
                if (percentageRestore)
                {
                    actualManaAmount = maxMana * (manaAmount / 100f);
                }

                // Need to add a method to PlayerCombat to increase mana
                AddManaToPlayer(playerCombat, actualManaAmount);
                return true;
            }
        }
        return false;
    }

    private void AddManaToPlayer(PlayerCombat playerCombat, float amount)
    {
        // This will need to be implemented in your PlayerCombat class
        // For now we'll use reflection to access the private field
        System.Reflection.FieldInfo field = typeof(PlayerCombat).GetField("currentMana",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        if (field != null)
        {
            float currentMana = (float)field.GetValue(playerCombat);
            currentMana = Mathf.Min(currentMana + amount, playerCombat.GetMaxMana());
            field.SetValue(playerCombat, currentMana);
        }
    }
}