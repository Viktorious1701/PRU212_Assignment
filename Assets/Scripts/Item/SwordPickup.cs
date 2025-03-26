using static PlayerCombat;
using UnityEngine;

public class WeaponPickup : LootItem
{
    public WeaponType weaponType;
    private PlayerCombat playerCombat;
    [SerializeField] private Sprite weaponSprite;

    protected override bool ApplyEffect(GameObject player)
    {
        playerCombat = player.GetComponent<PlayerCombat>();
        if (playerCombat != null)
        {
            playerCombat.CollectWeapon(weaponType, weaponSprite);
            Destroy(gameObject); // Remove pickup after collecting
            return true;
        }
        return false;
    }

   
}