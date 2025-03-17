using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowPickup : LootItem
{
    [Header("Arrow Settings")]
    [SerializeField] private int arrowAmount = 5;

    protected override bool ApplyEffect(GameObject player)
    {
        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();

        if (playerCombat != null)
        {
            int currentArrows = playerCombat.GetCurrentArrows();
            int maxArrows = playerCombat.GetMaxArrows();

            // Only collect if not at max arrows
            if (currentArrows < maxArrows)
            {
                playerCombat.AddArrows(arrowAmount);
                return true;
            }
        }
        return false;
    }
}