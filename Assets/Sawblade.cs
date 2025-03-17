using UnityEngine;

public class Sawblade : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private bool isMoving = false;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float waitTime = 5f;
    [SerializeField] private bool startMovingImmediately = true;
    [SerializeField] private bool requireTrigger = false;

    [Header("Damage Settings")]
    [SerializeField] private bool isDeadly = true;
    [SerializeField] private Transform respawnPoint;

    private bool isActivelyMoving = false;
    private Vector3 targetPosition;

    private void Start()
    {
        if (isMoving && startMovingImmediately && !requireTrigger)
        {
            BeginMovement();
        }

        // If it's not moving, make sure it stays at its current position
        if (!isMoving)
        {
            pointA = transform;
            pointB = transform;
        }
    }

    private void Update()
    {
        if (isActivelyMoving && isMoving)
        {
            // Move toward the target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Check if we've reached the target position
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                transform.position = targetPosition;
                isActivelyMoving = false;

                // Wait and then move to the other point
                Invoke("SwitchDestination", waitTime);
            }
        }
    }

    private void SwitchDestination()
    {
        isActivelyMoving = true;

        // If we're at or near pointA, move to pointB. Otherwise, move to pointA.
        if (Vector3.Distance(transform.position, pointA.position) < 0.1f)
        {
            targetPosition = pointB.position;
        }
        else
        {
            targetPosition = pointA.position;
        }
    }

    public void BeginMovement()
    {
        if (isMoving && !isActivelyMoving)
        {
            isActivelyMoving = true;
            targetPosition = pointB.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isDeadly)
            {
                // Handle player death
                Debug.Log("Player hit a sawblade!");

                // Get the player's health component
                Health playerHealth = other.GetComponent<Health>();

                // Reset player health to maximum
                if (playerHealth != null)
                {
                    // Reset to max health (assuming 100 is max)
                    float healAmount = playerHealth.GetMaxHealth() - playerHealth.GetCurrentHealth();
                    playerHealth.TakeDamage(-healAmount); // Negative damage to heal
                }

                // Respawn the player
                if (respawnPoint != null)
                {
                    other.transform.position = respawnPoint.position;
                }
                else
                {
                    other.transform.position = new Vector3(0, 0, 0); // Default respawn position
                }

                // Optional: You could add player health reduction, animation, sound effects, etc. here
            }
        }
    }
}
