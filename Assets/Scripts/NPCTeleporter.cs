using UnityEngine;
using System.Collections;

public class NPCTeleporter : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private Transform playerDetectionPoint;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool showGizmos = true;
    
    [Header("Dialogue Settings")]
    [SerializeField] private string thankYouMessage = "Thank you for your help!";
    [SerializeField] private string speakerName = "Woman";
    [SerializeField] private float dialogueDuration = 2f;
    
    [Header("Teleport Settings")]
    [SerializeField] private Transform teleportTarget; // The bearded NPC location
    [SerializeField] private float teleportOffset = 1.5f; // Distance from the bearded NPC
    [SerializeField] private float teleportDelay = 1f; // Delay after dialogue before teleporting
    [SerializeField] private bool teleportToRight = true; // Teleport to the right of the target?
    [SerializeField] private bool destroyAfterTeleport = true; // Destroy this NPC after teleporting?
    
    private bool playerInRange = false;
    private bool hasInteracted = false;
    private GameObject playerObject;
    private Dialogue dialogueSystem;
    
    private void Start()
    {
        // Set up detection point if not assigned
        if (playerDetectionPoint == null)
        {
            playerDetectionPoint = transform;
        }
        
        // Find dialogue system
        dialogueSystem = FindObjectOfType<Dialogue>();
        if (dialogueSystem == null)
        {
            Debug.LogWarning("No Dialogue system found in the scene. Dialogue will not work.");
        }
        
        // Validate teleport target
        if (teleportTarget == null)
        {
            // Try to find a bearded NPC in the scene
            var beardedNPC = GameObject.Find("bearded-idle-1");
            if (beardedNPC != null)
            {
                teleportTarget = beardedNPC.transform;
                Debug.Log("Auto-assigned bearded NPC as teleport target");
            }
            else
            {
                Debug.LogError("No teleport target assigned and couldn't find a bearded NPC automatically!");
            }
        }
    }
    
    private void Update()
    {
        // Check if player is in range and has pressed the interaction key
        if (playerInRange && !hasInteracted && Input.GetKeyDown(interactionKey))
        {
            StartInteraction();
        }
    }
    
    private void StartInteraction()
    {
        hasInteracted = true;
        
        // Show thank you dialogue
        if (dialogueSystem != null)
        {
            dialogueSystem.Say(thankYouMessage, speakerName, dialogueDuration);
            Debug.Log($"Showing dialogue: {speakerName}: {thankYouMessage}");
        }
        
        // Start teleport sequence after dialogue
        StartCoroutine(TeleportAfterDelay(teleportDelay));
    }
    
    private IEnumerator TeleportAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (teleportTarget != null)
        {
            // Calculate teleport position based on the bearded NPC position
            Vector3 targetPosition = teleportTarget.position;
            
            // Add offset to the side
            float xOffset = teleportToRight ? teleportOffset : -teleportOffset;
            targetPosition += new Vector3(xOffset, 0, 0);
            
            // Create teleport effect at current position (optional)
            // PlayTeleportEffectAt(transform.position);
            
            // Teleport this NPC
            transform.position = targetPosition;
            Debug.Log($"Teleported NPC to: {targetPosition}");
            
            // Trigger any event on the target NPC if needed
            NPCInteractionController targetNPC = teleportTarget.GetComponent<NPCInteractionController>();
            if (targetNPC != null)
            {
                // This will trigger the barrel/wheelbarrow hiding if it hasn't happened yet
                targetNPC.TriggerHideObjects();
            }
            
            // Destroy this NPC after teleporting if configured to do so
            if (destroyAfterTeleport)
            {
                Destroy(gameObject, 0.1f); // Small delay to ensure everything is processed
            }
        }
        else
        {
            Debug.LogError("Cannot teleport: target is null");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            playerObject = other.gameObject;
            
            // Show interaction hint or UI here if you have one
            Debug.Log("Player in range of NPC teleporter, press E to interact");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            
            // Hide interaction hint or UI here if you have one
        }
    }
    
    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            // Draw interaction radius
            Gizmos.color = Color.cyan;
            if (playerDetectionPoint != null)
            {
                Gizmos.DrawWireSphere(playerDetectionPoint.position, interactionDistance);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, interactionDistance);
            }
            
            // Draw teleport target link
            if (teleportTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, teleportTarget.position);
                
                // Draw teleport destination point
                Vector3 targetPosition = teleportTarget.position;
                float xOffset = teleportToRight ? teleportOffset : -teleportOffset;
                targetPosition += new Vector3(xOffset, 0, 0);
                
                Gizmos.DrawSphere(targetPosition, 0.3f);
            }
        }
    }
    
    // Public method to reset interaction state (if needed)
    public void ResetInteraction()
    {
        hasInteracted = false;
    }
} 