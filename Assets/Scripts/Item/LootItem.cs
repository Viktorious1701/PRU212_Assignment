using UnityEngine;

public abstract class LootItem : MonoBehaviour
{
    [Header("Base Loot Settings")]
    [SerializeField] protected float pickupDelay = 0.5f;
    [SerializeField] protected float popForce = 5f;
    [SerializeField] protected float popTorque = 2f;
    [SerializeField] protected bool applyRandomDirection = true;

    [Header("Visual Effects")]
    [SerializeField] protected GameObject collectEffectPrefab;
    [SerializeField] protected AudioClip collectSound;
    [SerializeField] protected float floatAmplitude = 0.2f;
    [SerializeField] protected float floatSpeed = 2f;
    [SerializeField] protected float rotationSpeed = 30f;

    protected Rigidbody2D rb;
    protected bool canBeCollected = false;
    protected Vector3 startPosition;
    protected bool hasSettled = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
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
    }

    protected virtual void EnableCollection()
    {
        canBeCollected = true;
    }

    protected virtual void Update()
    {
        // Check if the item has settled
        if (!hasSettled && rb != null && rb.velocity.magnitude < 0.1f)
        {
            hasSettled = true;
            startPosition = transform.position;

            // Once settled, we can switch to kinematic to prevent further physics interactions
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Apply floating effect once the item has settled
        if (hasSettled)
        {
            // Smoothly float up and down
            Vector3 newPosition = startPosition;
            newPosition.y = startPosition.y + Mathf.Abs(Mathf.Sin(Time.time * floatSpeed)) * floatAmplitude;
            transform.position = newPosition;

            // Add gentle rotation
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D other)
    {
        // Only allow collection after the delay
        if (!canBeCollected) return;

        // Check if this is the player
        if (other.gameObject.CompareTag("Player"))
        {
            if (ApplyEffect(other.gameObject))
            {
                // Play collection effect and destroy
                PlayCollectEffect();
                Destroy(gameObject);
            }
        }
    }

    // Abstract method that each loot type will implement
    protected abstract bool ApplyEffect(GameObject player);

    protected virtual void PlayCollectEffect()
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