using UnityEngine;

public class PlayerDialogueManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Dialogue dialogueSystem;
    private Rigidbody2D rb;
    private Animator animator;

    // Animation parameter names (mirrored from PlayerMovement for consistency)
    private const string IS_RUNNING = "isRunning";
    private const string IS_JUMPING = "isJumping";
    private const string IS_FALLING = "isFalling";
    private const string IS_ON_AIR = "isOnAir";
    private const string IS_GROUND = "isGround";
    private const string IS_CLIMBING = "isClimbing";

    private bool wasDialogueActiveLastFrame = false;

    private void Awake()
    {
        // Get references
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
        if (dialogueSystem == null)
            dialogueSystem = FindObjectOfType<Dialogue>();

        // Validate references
        if (playerMovement == null) Debug.LogError("PlayerMovement not found on " + gameObject.name);
        if (dialogueSystem == null) Debug.LogError("Dialogue not found in scene");
        if (rb == null) Debug.LogError("Rigidbody2D not found on " + gameObject.name);
        if (animator == null) Debug.LogError("Animator not found on " + gameObject.name);
    }

    private void Update()
    {
        if (dialogueSystem.isDialogueActive)
        {
            // Lock movement by setting velocity to zero
            rb.velocity = Vector2.zero;

            // Set animation states to idle when dialogue starts
            if (!wasDialogueActiveLastFrame)
            {
                animator.SetBool(IS_RUNNING, false);
                animator.SetBool(IS_JUMPING, false);
                animator.SetBool(IS_FALLING, false);
                animator.SetBool(IS_ON_AIR, false);
                animator.SetBool(IS_GROUND, true); // Assume grounded during dialogue
                animator.SetBool(IS_CLIMBING, false);
            }
        }
        wasDialogueActiveLastFrame = dialogueSystem.isDialogueActive;
    }

    // Public method for PlayerMovement to check if movement is allowed
    public bool CanMove()
    {
        return !dialogueSystem.isDialogueActive;
    }
}