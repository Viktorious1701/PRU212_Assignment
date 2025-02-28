using UnityEngine;
using System;

// Struct to hold information about damage
public struct DamageInfo
{
    public float damageAmount;
    public GameObject damageSource;
    public Vector3 hitPoint;
    public Vector3 hitDirection;

    public DamageInfo(float amount, GameObject source, Vector3 point, Vector3 direction)
    {
        damageAmount = amount;
        damageSource = source;
        hitPoint = point;
        hitDirection = direction;
    }
}

// Static class to manage damage events
public static class DamageSystem
{
    // Delegate for damage events
    public delegate void DamageEventHandler(GameObject target, DamageInfo damageInfo);

    // Event that's triggered when damage is applied
    public static event DamageEventHandler OnDamageApplied;

    // Method to apply damage
    public static void ApplyDamage(GameObject target, DamageInfo damageInfo)
    {
        // Trigger the event
        OnDamageApplied?.Invoke(target, damageInfo);
    }
}