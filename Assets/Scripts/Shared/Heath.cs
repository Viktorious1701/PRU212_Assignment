using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float maxHealth = 100f;

    // Optional: Unity Events for UI updates or other responses
    [SerializeField] private UnityEvent<float> onHealthChanged;
    [SerializeField] private UnityEvent onDeath;

    // Optional: Event for knock-back or visual effects on hit
    [SerializeField] private UnityEvent<Vector3> onDamageImpact;

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

        TakeDamage(damageInfo.damageAmount);

        // Trigger impact event (for knockback, particles, etc)
        onDamageImpact?.Invoke(damageInfo.hitDirection);
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
}