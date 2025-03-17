using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerBuffManager : MonoBehaviour
{
    // References to components that can be affected by buffs
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;

    // Track active buffs
    private Dictionary<string, Coroutine> activeBuffs = new Dictionary<string, Coroutine>();
    private Dictionary<string, GameObject> activeEffects = new Dictionary<string, GameObject>();

    // Store original values
    private float originalMoveSpeed;
    private float originalJumpForce;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();

        // Store original values for resetting after buffs expire
        if (playerMovement != null)
        {
            // Use reflection to access private fields
            System.Reflection.FieldInfo moveSpeedField = typeof(PlayerMovement).GetField("moveSpeed",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            System.Reflection.FieldInfo jumpForceField = typeof(PlayerMovement).GetField("jumpForce",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (moveSpeedField != null)
                originalMoveSpeed = (float)moveSpeedField.GetValue(playerMovement);

            if (jumpForceField != null)
                originalJumpForce = (float)jumpForceField.GetValue(playerMovement);
        }
    }

    public void ApplySpeedBuff(float multiplier, float duration, GameObject effectPrefab = null)
    {
        // Cancel existing speed buff if there is one
        if (activeBuffs.ContainsKey("speed"))
        {
            StopCoroutine(activeBuffs["speed"]);
            if (activeEffects.ContainsKey("speed") && activeEffects["speed"] != null)
                Destroy(activeEffects["speed"]);
        }

        // Start new speed buff
        activeBuffs["speed"] = StartCoroutine(SpeedBuffCoroutine(multiplier, duration));

        // Spawn visual effect if provided
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, transform);
            activeEffects["speed"] = effect;
        }

        // Show UI feedback (you can implement this)
        ShowBuffUI("Speed Boost", duration);
    }

    public void ApplyJumpBuff(float multiplier, float duration, GameObject effectPrefab = null)
    {
        // Cancel existing jump buff if there is one
        if (activeBuffs.ContainsKey("jump"))
        {
            StopCoroutine(activeBuffs["jump"]);
            if (activeEffects.ContainsKey("jump") && activeEffects["jump"] != null)
                Destroy(activeEffects["jump"]);
        }

        // Start new jump buff
        activeBuffs["jump"] = StartCoroutine(JumpBuffCoroutine(multiplier, duration));

        // Spawn visual effect if provided
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, transform);
            activeEffects["jump"] = effect;
        }

        // Show UI feedback (you can implement this)
        ShowBuffUI("Jump Boost", duration);
    }

    private IEnumerator SpeedBuffCoroutine(float multiplier, float duration)
    {
        // Get move speed field through reflection
        System.Reflection.FieldInfo moveSpeedField = typeof(PlayerMovement).GetField("moveSpeed",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        if (moveSpeedField != null)
        {
            // Apply speed boost
            float boostedSpeed = originalMoveSpeed * multiplier;
            moveSpeedField.SetValue(playerMovement, boostedSpeed);

            // Wait for duration
            yield return new WaitForSeconds(duration);

            // Reset speed
            moveSpeedField.SetValue(playerMovement, originalMoveSpeed);

            // Clean up
            activeBuffs.Remove("speed");
            if (activeEffects.ContainsKey("speed") && activeEffects["speed"] != null)
            {
                Destroy(activeEffects["speed"]);
                activeEffects.Remove("speed");
            }
        }
    }

    private IEnumerator JumpBuffCoroutine(float multiplier, float duration)
    {
        // Get jump force field through reflection
        System.Reflection.FieldInfo jumpForceField = typeof(PlayerMovement).GetField("jumpForce",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        if (jumpForceField != null)
        {
            // Apply jump boost
            float boostedJumpForce = originalJumpForce * multiplier;
            jumpForceField.SetValue(playerMovement, boostedJumpForce);

            // Wait for duration
            yield return new WaitForSeconds(duration);

            // Reset jump force
            jumpForceField.SetValue(playerMovement, originalJumpForce);

            // Clean up
            activeBuffs.Remove("jump");
            if (activeEffects.ContainsKey("jump") && activeEffects["jump"] != null)
            {
                Destroy(activeEffects["jump"]);
                activeEffects.Remove("jump");
            }
        }
    }

    // Method to display a UI indicator for active buffs
    private void ShowBuffUI(string buffName, float duration)
    {
        // You can implement this to show a UI element when buffs are active
        Debug.Log($"{buffName} active for {duration} seconds");

        // This could interact with a UI manager to show an icon with a timer
    }
}