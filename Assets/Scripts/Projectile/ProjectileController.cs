using UnityEngine;

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
    }

    public void Initialize(float damageAmount, float maxRange, GameObject source, Vector2 dir)
    {
        damage = damageAmount;
        range = maxRange;
        lastPosition = transform.position;
        owner = source;
        direction = dir;
    }



    private void Update()
    {
        if (!hasHit) // Only move if it hasn't hit anything yet
        {
            if (direction != Vector2.zero)
            {
                transform.position += (Vector3)direction * speed * Time.deltaTime;
            }
            else
            {
                // Move projectile via transform (works with Kinematic or no Rigidbody)
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
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Skip if hitting the owner
        if (owner != null && collider.gameObject == owner)
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
        DamageSystem.ApplyDamage(collider.gameObject, damageInfo);

        // Stick to the hit object at the exact hit point
        transform.SetParent(collider.gameObject.transform, true); // 'true' keeps world position
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
}