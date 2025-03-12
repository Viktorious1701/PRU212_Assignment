using UnityEngine;

public class DestructibleBox : MonoBehaviour
{
    [Header("Destruction Settings")]
    [SerializeField] private GameObject brokenBoxPrefab;
    [SerializeField] private float destructionForce = 5f;
    [SerializeField] private float destructionTorque = 2f;
    [SerializeField] private bool destroyOnAnyHit = true;
    [SerializeField] private float hitPointsRequired = 1f;

    [Header("Reward Settings")]
    [SerializeField] private GameObject[] possibleRewards;
    [SerializeField] private float rewardSpawnChance = 0.5f;
    [SerializeField] private Vector2 rewardSpawnOffset = new Vector2(0f, 0.5f);

    private float currentHitPoints;
    private Health healthComponent;

    private void Awake()
    {
        // Use the Health component if we want to handle hit points through it
        healthComponent = GetComponent<Health>();

        if (healthComponent == null)
        {
            currentHitPoints = hitPointsRequired;
        }
        else
        {
            // Subscribe to the death event
            healthComponent.onDeath.AddListener(BreakBox);
        }
    }

    private void OnEnable()
    {
        // If we're not using the Health component, subscribe directly to damage events
        if (healthComponent == null)
        {
            DamageSystem.OnDamageApplied += HandleDamage;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled
        if (healthComponent == null)
        {
            DamageSystem.OnDamageApplied -= HandleDamage;
        }
    }

    private void HandleDamage(GameObject target, DamageInfo damageInfo)
    {
        // Only process damage for this gameObject
        if (target != gameObject) return;

        // If we're using the direct damage system instead of Health component
        if (destroyOnAnyHit)
        {
            BreakBox();
        }
        else
        {
            currentHitPoints -= damageInfo.damageAmount;
            if (currentHitPoints <= 0)
            {
                BreakBox();
            }
        }
    }

    private void BreakBox()
    {
        // Spawn broken box pieces
        if (brokenBoxPrefab != null)
        {
            GameObject brokenBox = Instantiate(brokenBoxPrefab, transform.position, transform.rotation);

            // Apply force to each piece
            ApplyForceToFragments(brokenBox);
        }

        // Potentially spawn a reward
        SpawnReward();

        // Destroy the original box
        Destroy(gameObject);
    }

    private void ApplyForceToFragments(GameObject brokenBox)
    {
        // Find all rigidbodies in the broken box prefab
        Rigidbody2D[] fragments = brokenBox.GetComponentsInChildren<Rigidbody2D>();

        foreach (Rigidbody2D fragment in fragments)
        {
            // Calculate direction from box center to fragment
            Vector2 direction = (Vector2)(fragment.transform.position - transform.position).normalized;

            // If the fragment is at the exact same position as the center, give it a random direction
            if (direction == Vector2.zero)
            {
                direction = Random.insideUnitCircle.normalized;
            }

            // Apply force and torque
            fragment.AddForce(direction * destructionForce, ForceMode2D.Impulse);
            fragment.AddTorque(Random.Range(-destructionTorque, destructionTorque), ForceMode2D.Impulse);
        }
    }

    private void SpawnReward()
    {
        // Check if we should spawn a reward
        if (possibleRewards.Length == 0 || Random.value > rewardSpawnChance)
            return;

        // Select a random reward
        GameObject rewardPrefab = possibleRewards[Random.Range(0, possibleRewards.Length)];

        if (rewardPrefab != null)
        {
            // Calculate spawn position with offset
            Vector3 spawnPosition = transform.position + new Vector3(rewardSpawnOffset.x, rewardSpawnOffset.y, 0);

            // Spawn the reward
            Instantiate(rewardPrefab, spawnPosition, Quaternion.identity);
        }
    }
}