using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [Header("Healing Settings")]
    [SerializeField] private float healAmount = 25f;
    [SerializeField] private bool percentageHealing = false;
    [SerializeField] private float pickupDelay = 0.5f; // Short delay before pickup is possible

    [Header("Movement Settings")]
    [SerializeField] private float popForce = 5f;
    [SerializeField] private float popTorque = 2f;
    [SerializeField] private bool applyRandomDirection = true;

    [Header("Visual Effects")]
    [SerializeField] private GameObject collectEffectPrefab;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float bobAmount = 0.2f;
    [SerializeField] private float bobSpeed = 2f;

    private Rigidbody2D rb;
    private bool canBeCollected = false;
    private Vector3 startPosition;
    private float bobTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Pop out of the crate with some force
        if (rb != null)
        {
            Vector2 popDirection;

            if (applyRandomDirection)
            {
                // Random angle between 30 and 150 degrees (mostly upward)
                float angle = Random.Range(30f, 150f) * Mathf.Deg2Rad;
                popDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }
            else
            {
                // Default upward direction
                popDirection = Vector2.up;
            }

            // Apply initial force and torque
            rb.AddForce(popDirection * popForce, ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-popTorque, popTorque), ForceMode2D.Impulse);

            // Enable collection after delay
            Invoke("EnableCollection", pickupDelay);
        }

        // Store starting position for bobbing effect
        startPosition = transform.position;
    }

    private void EnableCollection()
    {
        canBeCollected = true;
    }

    private void Update()
    {
        // Apply bobbing effect once the potion has settled
        if (rb != null && rb.velocity.magnitude < 0.1f)
        {
            bobTimer += Time.deltaTime * bobSpeed;

            // Smoothly bob up and down
            Vector3 newPosition = transform.position;
            newPosition.y += Mathf.Sin(bobTimer) * bobAmount;
            transform.position = newPosition;
        }
        else if (rb != null && rb.velocity.magnitude < 0.1f)
        {
            // Save position once the potion has settled
            startPosition = transform.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Only allow collection after the delay
        if (!canBeCollected) return;

        // Check if this is the player
        Health playerHealth = other.collider.GetComponent<Health>();

        if (playerHealth != null)
        {
            // Calculate healing amount
            float actualHealAmount = healAmount;

            if (percentageHealing)
            {
                // Calculate percentage of max health
                actualHealAmount = playerHealth.GetMaxHealth() * (healAmount / 100f);
            }

            // Apply healing - we're using the negative of damage amount to heal
            float currentHealth = playerHealth.GetCurrentHealth();
            float maxHealth = playerHealth.GetMaxHealth();

            // Only heal if not at max health
            if (currentHealth < maxHealth)
            {
                // Using TakeDamage with negative value to heal
                playerHealth.TakeDamage(-actualHealAmount);

                // Play collection effect
                PlayCollectEffect();

                // Destroy the potion
                Destroy(gameObject);
            }
        }
    }

    private void PlayCollectEffect()
    {
        // Spawn visual effect if provided
        if (collectEffectPrefab != null)
        {
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }

        // Play sound if provided
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
    }
}