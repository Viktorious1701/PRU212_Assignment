using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float maxHealth = 100f;

    // Invincibility settings
    [SerializeField] private float invincibilityTime = 1f; // Duration of invincibility after being hit
    [SerializeField] private bool isInvincible = false;

    // Optional: Unity Events for UI updates or other responses
    [SerializeField] public UnityEvent<float> onHealthChanged;
    [SerializeField] private UnityEvent onDeath;

    // Optional: Event for knock-back or visual effects on hit
    [SerializeField] private UnityEvent<Vector3> onDamageImpact;

    // Event to handle invincibility state changes (optional, for visual feedback)
    [SerializeField] private UnityEvent<bool> onInvincibilityChanged;

    private void OnEnable()
    {
        // Subscribe to the damage event
        DamageSystem.OnDamageApplied += HandleDamage;
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled
        DamageSystem.OnDamageApplied -= HandleDamage;
    }

    private void HandleDamage(GameObject target, DamageInfo damageInfo)
    {
        // Only process damage for this gameObject
        if (target != gameObject) return;

        // Check if currently invincible
        if (isInvincible) return;

        // Trigger impact event (for knockback, particles, etc)
        onDamageImpact?.Invoke(damageInfo.hitDirection);
        TakeDamage(damageInfo.damageAmount);


        // Start invincibility
        StartInvincibility();
    }

    public void TakeDamage(float damage)
    {
        float previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Invoke health changed event
        onHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0 && previousHealth > 0)
        {
            Die();
        }
    }

    private void StartInvincibility()
    {
        // Activate invincibility
        isInvincible = true;
        onInvincibilityChanged?.Invoke(true);

        // Start coroutine to end invincibility after set time
        StartCoroutine(EndInvincibility());
    }

    private System.Collections.IEnumerator EndInvincibility()
    {
        // Wait for invincibility duration
        yield return new WaitForSeconds(invincibilityTime);

        // Deactivate invincibility
        isInvincible = false;
        onInvincibilityChanged?.Invoke(false);
    }

    private void Die()
    {
        // Invoke death event
        onDeath?.Invoke();

        // You might want to control destruction through an animation
    }

    // Helper methods
    public float GetCurrentHealth() { return currentHealth; }
    public float GetMaxHealth() { return maxHealth; }
    public float GetHealthPercentage() { return currentHealth / maxHealth; }
    public bool IsInvincible() { return isInvincible; }
}