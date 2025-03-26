using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PlayerCombat;

public class WeaponDisplayController : MonoBehaviour
{
    [Header("Weapon Icons")]
    [SerializeField] private Sprite bareHandIcon;
    [SerializeField] private Sprite swordIcon;
    [SerializeField] private Sprite bowIcon;
    [SerializeField] private Sprite spellIcon;

    [Header("UI References")]
    [SerializeField] private Image weaponIconImage;
    [SerializeField] private TextMeshProUGUI weaponNameText;

    private PlayerCombat playerCombat;

    private void Start()
    {
        // Find the PlayerCombat script on the player
        playerCombat = FindObjectOfType<PlayerCombat>();

        // If you prefer to drag the reference in inspector, you can make this optional
        if (playerCombat == null)
        {
            Debug.LogError("PlayerCombat script not found!");
            return;
        }

        // Initial weapon setup
        UpdateWeaponDisplay();
    }

    private void Update()
    {
        // You'll need to modify the PlayerCombat script to expose the current weapon
        UpdateWeaponDisplay();
    }

    private void UpdateWeaponDisplay()
    {
        // You'll need to add a method to PlayerCombat to get the current weapon
        WeaponType currentWeapon = playerCombat.GetCurrentWeapon();

        switch (currentWeapon)
        {
            case WeaponType.BareHand:
                weaponIconImage.sprite = bareHandIcon;
                weaponNameText.text = "Fists";
                break;
            case WeaponType.Sword:
                weaponIconImage.sprite = swordIcon;
                weaponNameText.text = "Sword";
                break;
            case WeaponType.Bow:
                weaponIconImage.sprite = bowIcon;
                weaponNameText.text = "Bow";
                break;
            case WeaponType.Spell:
                weaponIconImage.sprite = spellIcon;
                weaponNameText.text = "Spell";
                break;
        }
    }
}