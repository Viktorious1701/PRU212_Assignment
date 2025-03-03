using UnityEngine;

public class PlatformRotator : MonoBehaviour
{
    public float rotationSpeed = 50f;      // Degrees per second
    public Transform centerPoint;           // The point to rotate around
    public float radius = 2f;              // Distance from center point
    public bool clockwise = true;          // Direction of rotation

    private float currentAngle = 0f;
    private Vector3 startPosition;
    private BoxCollider2D platformCollider;
    
    private GameObject player;
    private Rigidbody2D playerRb;

    private void Start()
    {
        // Get components
        platformCollider = GetComponent<BoxCollider2D>();
        
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
        }

       

        // Store the initial distance from center if no radius is specified
        if (radius <= 0)
        {
            radius = Vector2.Distance(centerPoint.position, transform.position);
        }

        // Calculate initial angle
        startPosition = transform.position;
        currentAngle = Mathf.Atan2(
            startPosition.y - centerPoint.position.y,
            startPosition.x - centerPoint.position.x
        ) * Mathf.Rad2Deg;
    }

    private void Update()
    {
        // Update angle based on direction
        float direction = clockwise ? -1 : 1;
        currentAngle += rotationSpeed * Time.deltaTime * direction;

        // Calculate new position
        float x = centerPoint.position.x + radius * Mathf.Cos(currentAngle * Mathf.Deg2Rad);
        float y = centerPoint.position.y + radius * Mathf.Sin(currentAngle * Mathf.Deg2Rad);

        // Update position while maintaining original rotation
        transform.position = new Vector3(x, y, transform.position.z);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get the collision contact point
            ContactPoint2D contact = collision.GetContact(0);

            // Check if collision is from above (player landing on platform)
            if (contact.normal.y > 0.7f)
            {
                // Parent the player to the platform
                collision.transform.SetParent(transform);
            }
            else
            {
                // For side collisions, push the player out
                Vector2 pushDirection = contact.normal;
                float pushForce = 5f; // Adjust this value as needed
                playerRb.velocity = pushDirection * pushForce;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Unparent the player when they leave the platform
            collision.transform.SetParent(null);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get the collision contact point
            ContactPoint2D contact = collision.GetContact(0);

            // If player is trying to move through the platform from the sides
            if (Mathf.Abs(contact.normal.y) < 0.7f)
            {
                // Calculate the correction vector
                Vector2 correction = contact.normal * 0.1f;

                // Apply the correction to the player's position
                collision.transform.position += (Vector3)correction;
            }
        }
    }

    // Optional: Visualize the rotation path in the editor
    private void OnDrawGizmos()
    {
        if (centerPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerPoint.position, radius);
        }
    }
}