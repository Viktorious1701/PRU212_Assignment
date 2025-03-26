using UnityEngine;

public class WheelbarrowController : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private Transform handlePoint;
    [SerializeField] private float breakDistance = 3f; // Distance at which connection breaks
    
    private bool isHeld = false;
    private GameObject player;
    private SpringJoint2D joint;
    private Rigidbody2D wheelbarrowRb;
    private Rigidbody2D playerRb;
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        wheelbarrowRb = GetComponent<Rigidbody2D>();
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
        }
        
        if (handlePoint == null)
        {
            handlePoint = transform;
        }
    }
    
    private void Update()
    {
        // Check if player is in range
        if (player != null && Vector2.Distance(player.transform.position, handlePoint.position) <= interactionRange)
        {
            // Toggle holding state when E is pressed
            if (Input.GetKeyDown(KeyCode.E))
            {
                isHeld = !isHeld;
                if (isHeld)
                {
                    // Create spring joint when grabbing
                    joint = player.AddComponent<SpringJoint2D>();
                    joint.connectedBody = wheelbarrowRb;
                    joint.autoConfigureConnectedAnchor = false;
                    
                    // Set anchor points
                    joint.anchor = Vector2.zero; // Center of player
                    joint.connectedAnchor = transform.InverseTransformPoint(handlePoint.position); // Handle point relative to wheelbarrow
                    
                    // Configure spring settings for stronger connection
                    joint.distance = 0.2f; // Shorter distance for tighter control
                    joint.dampingRatio = 0.5f; // Less damping for more responsive movement
                    joint.frequency = 5f; // Higher frequency for stronger spring
                    
                    // Enable collision and set break force
                    joint.enableCollision = true;
                    joint.breakForce = float.PositiveInfinity; // Prevent breaking from force
                }
                else
                {
                    // Remove joint when releasing
                    if (joint != null)
                    {
                        Destroy(joint);
                    }
                }
            }
        }
        else if (isHeld && Vector2.Distance(player.transform.position, handlePoint.position) > breakDistance)
        {
            // Auto-release if too far
            isHeld = false;
            if (joint != null)
            {
                Destroy(joint);
            }
        }
    }
    
    // Public method to force release the wheelbarrow (can be called from other scripts)
    public void ForceRelease()
    {
        if (isHeld)
        {
            Debug.Log("Force releasing wheelbarrow");
            isHeld = false;
            
            // Find and destroy any joints on the player
            if (player != null)
            {
                SpringJoint2D[] joints = player.GetComponents<SpringJoint2D>();
                foreach (SpringJoint2D j in joints)
                {
                    if (j != null)
                    {
                        Destroy(j);
                    }
                }
            }
            
            // Also clear our joint reference
            joint = null;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (handlePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(handlePoint.position, interactionRange);
            
            // Draw break distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(handlePoint.position, breakDistance);
        }
    }
} 