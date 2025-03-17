using UnityEngine.Tilemaps;
using UnityEngine;
using System.Collections;

public class DoorActivation : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float playerDetectionRadius = 3f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private KeyCode doorInteractionKey = KeyCode.F;
    [SerializeField] private string playerTag = "Player";

    [Header("Key Object References")]
    [SerializeField] private GameObject keyObject;
    [SerializeField] private string keyTag = "DoorKey";

    [Header("Dialogue System")]
    [SerializeField] private Dialogue dialogueSystem; // Reference to your dialogue system
    [SerializeField] private string noKeyMessage = "You need a key to open this door."; // Message to display
    [SerializeField] private string doorOpenMessage = "The door opens..."; // Optional message when door opens
    [SerializeField] private string characterName = ""; // Optional character name for the dialogue
    [SerializeField] private float dialogueDuration = 2f; // How long to display the message

    [Header("Debug Options")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showGizmos = true;

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private bool isDoorOpen = false;
    private bool isMoving = false;
    private bool hasShownNoKeyMessage = false; // Prevents message spam

    private Tilemap tilemap;
    private CompositeCollider2D compositeCollider;
    private Rigidbody2D rb;
    private GameObject playerObject;
    private void Awake()
    {
        // Get components
        tilemap = GetComponent<Tilemap>();
        compositeCollider = GetComponent<CompositeCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        // Store initial position
        initialPosition = transform.position;

        // Calculate target position (up direction)
        targetPosition = initialPosition + new Vector3(0, moveDistance, 0);

        // Try to find player if using tag
        playerObject = GameObject.FindGameObjectWithTag(playerTag);

        // Check for dialogue system reference
        if (dialogueSystem == null)
        {
            dialogueSystem = FindObjectOfType<Dialogue>();
            if (dialogueSystem == null && showDebugLogs)
            {
                Debug.LogWarning("No Dialogue system found. No key messages will be displayed.");
            }
        }
        if (showDebugLogs)
        {
            Debug.Log("Door initialized. Initial position: " + initialPosition);
            Debug.Log("Player layer mask value: " + playerLayer.value);

            // Check if we found the player by tag
            if (playerObject != null)
            {
                Debug.Log("Found player by tag: " + playerObject.name);
                Debug.Log("Player layer: " + LayerMask.LayerToName(playerObject.layer));
            }
            else
            {
                Debug.LogWarning("No player found with tag: " + playerTag);
            }
        }
    }

    private void Update()
    {
        // Add debug logs to track conditions
        bool hasKey = PlayerHasKey();
        bool inRange = PlayerInRange();

        // Reset the message flag when player moves away
        if (!inRange)
        {
            hasShownNoKeyMessage = false;
        }
        if (showDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log("Has key: " + hasKey + ", In range: " + inRange);
        }

        // Only check for F key press if not already open and the player has the key
        if (!isDoorOpen && !isMoving && inRange)
        {
            if (Input.GetKeyDown(doorInteractionKey))
            {
                if (hasKey)
                {
                    // Open the door
                    if (showDebugLogs) Debug.Log("F key pressed with key! Starting door movement...");
                    StartCoroutine(MoveDoor(targetPosition));
                    isDoorOpen = true;

                    // Optional: Show door opening message
                    if (dialogueSystem != null && !string.IsNullOrEmpty(doorOpenMessage))
                    {
                        dialogueSystem.Say(doorOpenMessage, characterName, dialogueDuration);
                    }
                }
                else if (!hasShownNoKeyMessage)
                {
                    // Show "need key" message
                    if (dialogueSystem != null)
                    {
                        dialogueSystem.Say(noKeyMessage, characterName, dialogueDuration);
                        hasShownNoKeyMessage = true;
                        if (showDebugLogs) Debug.Log("Showing 'no key' message");
                    }
                    else if (showDebugLogs)
                    {
                        Debug.Log("Player needs key but dialogueSystem is null");
                    }
                }
            }
        }
 }

    private bool PlayerHasKey()
    {
        // Check if the key has been collected or destroyed
        if (keyObject == null) // If key reference is null
        {
            return true;
        }
        else if (!keyObject.activeInHierarchy) // If key is inactive
        {
            return true;
        }
        else if (keyObject.TryGetComponent<KeyItem>(out var keyItem))
        {
            // Check if the key has been collected through the KeyItem component
            return keyItem.IsCollected();
        }

        return false;
    }

    private bool PlayerInRange()
    {
        // Simplest check: Use OverlapCircle as primary method
        bool inRangeByLayer = Physics2D.OverlapCircle(transform.position, playerDetectionRadius, playerLayer);

        // Backup method: Direct distance calculation between door and player
        bool inRangeByDistance = false;
        if (playerObject != null)
        {
            // Calculate relative distance (not absolute position)
            Vector2 doorPosition = transform.position;
            Vector2 playerPosition = playerObject.transform.position;
            float distance = Vector2.Distance(doorPosition, playerPosition);

            inRangeByDistance = distance <= playerDetectionRadius;

            if (showDebugLogs && Time.frameCount % 60 == 0)
            {
                Debug.Log("Door position: " + doorPosition);
                Debug.Log("Player position: " + playerPosition);
                Debug.Log("Relative distance: " + distance + ", Detection radius: " + playerDetectionRadius);
                Debug.Log("In range by distance: " + inRangeByDistance + ", In range by layer: " + inRangeByLayer);
            }
        }

        // Return true if either method works
        return inRangeByLayer || inRangeByDistance;
    }

    // Public test method to trigger door movement from Inspector or other scripts
    public void TestDoorMovement()
    {
        if (showDebugLogs) Debug.Log("Door movement test triggered");
        StartCoroutine(MoveDoor(targetPosition));
    }

    private IEnumerator MoveDoor(Vector3 targetPos)
    {
        if (showDebugLogs) Debug.Log("Moving door from " + transform.position + " to " + targetPos);
        isMoving = true;
        float timeElapsed = 0;
        Vector3 startPos = transform.position;

        while (timeElapsed < 1)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, timeElapsed);
            timeElapsed += Time.deltaTime * moveSpeed;

            // Update the collider to match the visual position
            if (compositeCollider != null)
            {
                compositeCollider.GenerateGeometry();
            }
            yield return null;
        }

        // Ensure we reach exactly the target position
        transform.position = targetPos;
        isMoving = false;
        if (showDebugLogs) Debug.Log("Door movement complete. Final position: " + transform.position);
    }

    // Visualize the detection radius in the editor
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);

        // Show if player is in range
        if (playerObject != null)
        {
            float distance = Vector2.Distance(transform.position, playerObject.transform.position);
            bool inRange = distance <= playerDetectionRadius;

            Gizmos.color = inRange ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, playerObject.transform.position);

            // Draw text in scene view
#if UNITY_EDITOR
            if (UnityEditor.SceneView.currentDrawingSceneView != null)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up, 
                    "Distance: " + distance.ToString("F2") + "\nIn Range: " + inRange);
            }
#endif
        }
    }
}