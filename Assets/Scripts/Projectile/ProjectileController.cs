using UnityEngine;
using System.Collections.Generic;

public class ProjectileController : MonoBehaviour
{
    private float damage;
    private float range;
    private float distanceTravelled = 0f;
    private Vector3 lastPosition;
    private Vector2 direction = Vector2.zero;
    private GameObject owner;
    private Animator animator;
    [SerializeField] private bool isHoming = false; // Adjust in Inspector
    public float speed = 10f; // Adjust in Inspector
    private bool hasHit = false; // Flag to track if the projectile has hit something

    // Homing parameters
    [SerializeField] private float homingRadius = 10f; // Detection radius for targets
    [SerializeField] private float homingForce = 5f; // How strongly it homes in on targets
    [SerializeField] private string targetTag = "Enemy"; // Tag of objects to target
    [SerializeField] private LayerMask allyLayer; // Layer mask for allies to ignore
    private Transform currentTarget = null;

    // Cached results to avoid garbage collection
    private Collider2D[] colliderResults;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialize(float damageAmount, float maxRange, GameObject source)
    {
        damage = damageAmount;
        range = maxRange;
        lastPosition = transform.position;
        owner = source;

        // If owner is player, target enemies. If owner is enemy, target player
        if (owner.CompareTag("Player"))
        {
            targetTag = "Enemy";
        }
        else if (owner.CompareTag("Enemy"))
        {
            targetTag = "Player";
        }
    }

    public void Initialize(float damageAmount, float maxRange, GameObject source, Vector2 dir)
    {
        damage = damageAmount;
        range = maxRange;
        lastPosition = transform.position;
        owner = source;
        direction = dir;

        // If owner is player, target enemies. If owner is enemy, target player
        if (owner.CompareTag("Player"))
        {
            targetTag = "Enemy";
        }
        else if (owner.CompareTag("Enemy"))
        {
            targetTag = "Player";
        }
    }

    private void Update()
    {
        if (hasHit) return; // Skip if already hit something

        Vector3 moveDirection = direction;

        // Handle homing behavior if enabled
        if (isHoming)
        {
            // Find target if we don't have one or current target is destroyed
            if (currentTarget == null && targetTag != "")
            {
                FindNewTarget();
            }

            // Apply homing behavior if we have a target
            if (currentTarget != null)
            {
                // Calculate direction to target
                Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;

                // If we had an initial direction, blend between it and the homing direction
                if (direction != Vector2.zero)
                {
                    moveDirection = Vector3.Lerp(direction.normalized, directionToTarget, homingForce * Time.deltaTime);
                }
                else
                {
                    // Use the current right direction (the way the projectile is facing) and blend with homing
                    moveDirection = Vector3.Lerp(transform.right, directionToTarget, homingForce * Time.deltaTime);
                }

                // Update the projectile's rotation to face the movement direction
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }

        // Move the projectile
        if (moveDirection != Vector3.zero)
        {
            transform.position += (Vector3)moveDirection.normalized * speed * Time.deltaTime;
        }
        else
        {
            // Fallback to moving along the right direction
            transform.position += transform.right * speed * Time.deltaTime;
        }

        // Calculate distance traveled
        distanceTravelled += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        // Destroy if exceeded range
        if (distanceTravelled >= range)
        {
            Destroy(gameObject);
        }
    }

    private void FindNewTarget()
    {
        // Find all colliders in range
        int count = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            homingRadius,
            colliderResults
        );

        float closestDistance = float.MaxValue;
        Transform closestTarget = null;

        // Find the closest valid target
        for (int i = 0; i < count; i++)
        {
            GameObject potentialTarget = colliderResults[i].gameObject;

            // Skip if this is the owner or on the ally layer
            if (potentialTarget == owner ||
                ((1 << potentialTarget.layer) & allyLayer.value) != 0)
            {
                continue;
            }

            // Check if it has the target tag
            if (potentialTarget.CompareTag(targetTag))
            {
                float distance = Vector3.Distance(transform.position, potentialTarget.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = potentialTarget.transform;
                }
            }
        }

        currentTarget = closestTarget;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        GameObject hitObject = collider.gameObject;

        // Skip if hitting the owner
        if (owner != null && hitObject == owner)
            return;

        // Skip if hitting an ally (same layer as owner)
        if (((1 << hitObject.layer) & allyLayer.value) != 0)
            return;

        hasHit = true; // Stop movement

        if (animator != null)
        {
            animator.SetTrigger("Explode"); // Trigger explosion animation if present
        }

        // Apply damage
        DamageInfo damageInfo = new DamageInfo(
            damage,
            owner,
            transform.position,
            transform.forward
        );
        DamageSystem.ApplyDamage(hitObject, damageInfo);

        // Stick to the hit object at the exact hit point
        transform.SetParent(hitObject.transform, true); // 'true' keeps world position
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // Stop any residual velocity
            rb.isKinematic = true; // Make it kinematic to prevent physics interference
        }

        // Destroy after a delay (e.g., 2 seconds)
        Destroy(gameObject, 2f);
    }

    public void OnExplodeAnimationEnd()
    {
        Destroy(gameObject); // Destroy after explosion animation ends
    }

    // Optional: Draw gizmos for debugging
    private void OnDrawGizmosSelected()
    {
        // Show homing radius in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, homingRadius);

        // Show direction
        if (direction != Vector2.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, direction.normalized * 2);
        }

        // Show target connection if we have one
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}