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
    [SerializeField] private bool isHoming = true; // Adjust in Inspector
    public float speed = 10f; // Adjust in Inspector
    private bool hasHit = false; // Flag to track if the projectile has hit something
    [SerializeField] private float startChasingTime = 0.1f; // Time to start chasing target
    private float chasingTimer;

    // Rotation smoothing
    [SerializeField] private float rotationSpeed = 10f; // Adjust this to control rotation smoothness
    [SerializeField] private float homingSmoothness = 0.5f; // Higher values make direction change more gradual (0-1)

    // Homing parameters
    [SerializeField] private float homingRadius = 10f; // Detection radius for targets
    [SerializeField] private float homingForce = 5f; // How strongly it homes in on targets
    [SerializeField] private string targetTag = "Enemy"; // Tag of objects to target
    [SerializeField] private LayerMask allyLayer; // Layer mask for allies to ignore
    private Transform currentTarget = null;
    private Vector2 currentMoveDirection;
    [SerializeField] private bool needRoate = false;

    // Cached results to avoid garbage collection
    private Collider2D[] colliderResults = new Collider2D[10];

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialize(float damageAmount, float maxRange, GameObject source)
    {
        damage = damageAmount;
        range = maxRange * 5;
        lastPosition = transform.position;
        owner = source;
        chasingTimer = startChasingTime;
        currentMoveDirection = transform.up;

        // If owner is player, target enemies. If owner is enemy, target player
        if (owner.CompareTag("Player"))
        {
            targetTag = "Enemy";
            allyLayer = LayerMask.GetMask("Player");
        }
        else if (owner.CompareTag("Enemy"))
        {
            targetTag = "Player";
            allyLayer = LayerMask.GetMask("Enemy");
        }
    }

    public void Initialize(float damageAmount, float maxRange, GameObject source, Vector2 dir)
    {
        damage = damageAmount;
        range = maxRange;
        lastPosition = transform.position;
        owner = source;
        direction = dir.normalized;
        currentMoveDirection = direction;
        chasingTimer = startChasingTime;

        // If owner is player, target enemies. If owner is enemy, target player
        if (owner.CompareTag("Player"))
        {
            targetTag = "Enemy";
            allyLayer = LayerMask.GetMask("Player");
        }
        else if (owner.CompareTag("Enemy"))
        {
            targetTag = "Player";
            allyLayer = LayerMask.GetMask("Enemy");
        }

        // Set initial rotation based on direction
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // Subtract 90 degrees because sprite faces up
            if(!needRoate)
            {
                angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            }
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void Update()
    {
        if (hasHit) return; // Skip if already hit something

        // Update chase timer
        if (chasingTimer > 0)
        {
            chasingTimer -= Time.deltaTime;
        }

        // Handle homing behavior if enabled
        if (isHoming)
        {
            // Find target if we don't have one or current target is destroyed
            if (currentTarget == null && targetTag != "")
            {
                FindNewTarget();
            }

            // Apply homing behavior if we have a target
            if (currentTarget != null && chasingTimer <= 0)
            {
                // Calculate direction to target
                Vector2 directionToTarget = (currentTarget.position - transform.position).normalized;

                // Smoothly change direction
                if (direction != Vector2.zero)
                {
                    // Gradually adjust direction based on homingSmoothness
                    currentMoveDirection = Vector2.Lerp(
                        currentMoveDirection,
                        directionToTarget,
                        homingForce * Time.deltaTime * (1f - homingSmoothness)
                    );
                }
                else
                {
                    currentMoveDirection = Vector2.Lerp(
                        currentMoveDirection,
                        directionToTarget,
                        homingForce * Time.deltaTime * (1f - homingSmoothness)
                    );
                }
            }
        }
        else
        {
            // If not homing, just use the initial direction
            if (direction != Vector2.zero)
            {
                currentMoveDirection = direction;
            }
            else
            {
                currentMoveDirection = transform.up;
            }
        }

        // Update rotation smoothly
        if (currentMoveDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(currentMoveDirection.y, currentMoveDirection.x) * Mathf.Rad2Deg;
            if(needRoate)
            {
                angle = Mathf.Atan2(currentMoveDirection.y, currentMoveDirection.x) * Mathf.Rad2Deg - 90f;
            }

            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Move the projectile
        transform.position += (Vector3)currentMoveDirection * speed * Time.deltaTime;

        // Draw debug ray
        Debug.DrawRay(transform.position, currentMoveDirection * 2, Color.green);

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

        if(collider.gameObject.CompareTag("Player") || collider.gameObject.CompareTag("Enemy"))
        {
            hasHit = true; // Stop movement
        }
        else
        {
            return;
        }

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
        Destroy(gameObject, 1f);
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