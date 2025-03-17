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
    private Vector2 lastHitDirection;
    private Vector3 lastHitPoint;

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
        DamageSystem.OnDamageApplied += HandleDamage;
    }

    private void OnDisable()
    {   
        DamageSystem.OnDamageApplied -= HandleDamage;
    }

    private void HandleDamage(GameObject target, DamageInfo damageInfo)
    {
        if (damageInfo.damageSource != null)
        {
            // Calculate direction FROM damage source TO this object (this is the direction the force should go)
            lastHitDirection = ((Vector2)(transform.position - damageInfo.damageSource.transform.position)).normalized;

            // Double check the direction makes sense (sometimes game logic might provide coordinates that yield unexpected results)
            if (lastHitDirection.magnitude < 0.1f)
            {
                // If the direction is too small, use the explicit hit direction instead
                lastHitDirection = damageInfo.hitDirection.normalized;
            }
        }
        else if (damageInfo.hitDirection != Vector3.zero)
        {
            // Use the provided hit direction directly
            lastHitDirection = damageInfo.hitDirection.normalized;
        }
        else
        {
            // Default direction if not available - purely fallback
            lastHitDirection = Vector2.right;
        }


        // If we're using the direct damage system instead of Health component
        if (destroyOnAnyHit)
        {
            BreakBox();
        }
        else if(healthComponent == null)
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

        // Make sure we have a valid hit direction
        if (lastHitDirection.magnitude < 0.1f)
        {
            lastHitDirection = Vector2.right; // Default fallback
        }

        foreach (Rigidbody2D fragment in fragments)
        {
            // Reset any existing velocity (important!)
            fragment.velocity = Vector2.zero;
            fragment.angularVelocity = 0;

            Vector2 direction;

            // Calculate the fragment's position relative to box center
            Vector2 fragmentOffset = (Vector2)(fragment.transform.position - transform.position);

            // If the fragment is too close to center, just use hit direction
            if (fragmentOffset.magnitude < 0.1f)
            {
                direction = lastHitDirection;
            }
            else
            {
                // For more natural-looking physics:
                // - Fragments near the hit point should move more directly away from hit
                // - Fragments far from hit point should move more based on their position

                // Calculate distance from hit point (if we have one)
                float distanceFromHit = Vector2.Distance(
                    (Vector2)fragment.transform.position,
                    (Vector2)lastHitPoint
                );

                // Normalize to a 0-1 range based on box size (assuming box radius of 1.0)
                float normalizedDistance = Mathf.Clamp01(distanceFromHit / 1.0f);

                // Weight more toward hit direction (0.7-0.9) but with some influence from fragment position
                float hitDirWeight = 0.9f - (normalizedDistance * 0.2f);  // 0.7 to 0.9
                float fragmentDirWeight = 1.0f - hitDirWeight;

                // For fragments on the opposite side of hit, strengthen the hit direction more
                float dotProduct = Vector2.Dot(fragmentOffset.normalized, lastHitDirection);
                if (dotProduct < 0)  // Fragment is on opposite side from hit direction
                {
                    hitDirWeight += 0.1f;
                    fragmentDirWeight = 1.0f - hitDirWeight;
                }

                // Combine with proper weighting
                direction = (lastHitDirection * hitDirWeight + fragmentOffset.normalized * fragmentDirWeight).normalized;
            }

            // Add a small upward component for more satisfying physics
            direction += new Vector2(0, 0.3f);
            direction.Normalize();

            // Apply immediate force
            fragment.AddForce(direction * destructionForce, ForceMode2D.Impulse);

            // Add some random spin
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