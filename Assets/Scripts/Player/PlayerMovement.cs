using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    // Animation parameter names
    private const string IS_RUNNING = "isRunning";
    private const string IS_GROUND = "isGround";
    private const string IS_JUMPING = "isJumping";
    private const string IS_FALLING = "isFalling";
    private const string IS_ON_AIR = "isOnAir";
    private const string VERTICAL_VELOCITY = "verticalVelocity";
    private const string IS_CLIMBING = "isClimbing"; // New animation parameter
    [SerializeField] private PlayerDialogueManager dialogueManager;
    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float wallJumpForce = 14f;
    [SerializeField] private float wallJumpUpForce = 12f;
    [SerializeField] private float wallSlidingSpeed = 2f;
    [SerializeField] private GameObject firePoint;

    [Header("Ladder Parameters")] // New section for ladder parameters
    [SerializeField] private float climbSpeed = 5f;
    [SerializeField] private LayerMask ladderLayer;

    [Header("Jump Physics")]
    [SerializeField] private float fallMultiplier = 2.5f; // Makes falling faster
    [SerializeField] private float lowJumpMultiplier = 2f; // For short jumps
    [SerializeField] private float jumpApexThreshold = 2f; // Velocity threshold for jump apex control
    [SerializeField] private float jumpApexBonus = 0.5f; // Extra gravity near apex for snappier jumps

    [Header("Dash Parameters")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private bool canDashInAir = true;

    [Header("Timing Parameters")]
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float wallJumpTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isWallSliding;
    private bool canDoubleJump = false;
    private float coyoteTimeCounter;
    private float wallJumpTimeCounter;
    private float jumpBufferCounter;
    private int facingDirection = 1;
    private bool isWallRight;

    // Ladder state
    private bool isOnLadder = false;
    private bool isClimbing = false;

    // Dash state
    private bool isDashing;
    private bool canDash = true;
    private float dashTimeLeft;
    private float dashCooldownTimeLeft;
    private Vector2 dashDirection;

    [Header("Collision Checks")]
    [SerializeField] private Transform groundCheck; // New: assign a child transform at your player's feet
    [SerializeField] private float groundCheckRadius = 0.2f; // New: tweak this value as needed
    [SerializeField] private LayerMask groundLayer; // Already exists in your code
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private float wallCheckDistance = 0.5f;

    private bool isKnockedBack = false;
    private float knockbackEndTime = 0f;

    public void ApplyKnockback(Vector2 force, float duration)
    {
        rb.velocity = Vector2.zero; // Optional: clear current velocity
        rb.AddForce(force, ForceMode2D.Impulse);
        isKnockedBack = true;
        knockbackEndTime = Time.time + duration;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (dialogueManager == null)
            dialogueManager = GetComponent<PlayerDialogueManager>();
    }

    private void Update()
    {
        if (isKnockedBack)
        {
            if (Time.time >= knockbackEndTime)
            {
                isKnockedBack = false;
            }
            else
            {
                // Skip normal movement processing
                return;
            }
        }

        if (dialogueManager.CanMove())
        {
            CheckAndStepUp();
            // Input handling
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
            bool jumpInput = Input.GetButtonDown("Jump");
            bool jumpHeld = Input.GetButton("Jump");
            bool dashInput = Input.GetKeyDown(KeyCode.LeftShift);

            // Update facing direction
            if (horizontalInput != 0)
            {
                facingDirection = (int)Mathf.Sign(horizontalInput);
            }

            if (rb.velocity.y < -0.1f && !isClimbing)
            {
                animator.SetBool(IS_JUMPING, false);
                animator.SetBool(IS_FALLING, true);
            }

            // Ground and wall checks
            CheckGrounded();
            CheckWallSliding(horizontalInput);
            CheckLadder(); // New ladder check

            // Handle climbing
            if (isOnLadder)
            {
                HandleLadderMovement(verticalInput, horizontalInput);
            }
            // If not on ladder or climbing, resume normal movement
            else if (!isClimbing)
            {
                // Handle dash input
                if (dashInput && canDash && (canDashInAir || isGrounded))
                {
                    InitiateDash(horizontalInput, verticalInput);
                }

                // Update dash state
                UpdateDash();

                // If not dashing, handle normal movement
                if (!isDashing)
                {
                    // Handle coyote time
                    if (isGrounded)
                    {
                        coyoteTimeCounter = coyoteTime;
                        canDoubleJump = true;
                    }
                    else
                    {
                        coyoteTimeCounter -= Time.deltaTime;
                    }

                    // Jump buffer
                    if (jumpInput)
                    {
                        jumpBufferCounter = jumpBufferTime;
                    }
                    else
                    {
                        jumpBufferCounter -= Time.deltaTime;
                    }

                    if (jumpBufferCounter > 0f)
                    {
                        // Normal jump (includes coyote time)
                        if (coyoteTimeCounter > 0f)
                        {
                            Jump(jumpForce);
                            jumpBufferCounter = 0f;
                            coyoteTimeCounter = 0f;
                        }
                        // Wall jump
                        else if (isWallSliding)
                        {
                            WallJump();
                            jumpBufferCounter = 0f;
                        }
                        // Double jump
                        else if (canDoubleJump && !isWallSliding)
                        {
                            Jump(jumpForce * 0.8f);
                            canDoubleJump = false;
                            jumpBufferCounter = 0f;
                        }
                    }

                    // Handle wall sliding
                    if (isWallSliding)
                    {
                        rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlidingSpeed));
                    }

                    // Handle horizontal movement
                    if (wallJumpTimeCounter <= 0)
                    {
                        float previousVelocityX = rb.velocity.x;
                        if (isWallSliding)
                        {
                            rb.velocity = new Vector2(previousVelocityX, -wallSlidingSpeed);
                        }
                        else
                            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

                        UpdateMovementAnimations(horizontalInput);
                    }
                    else
                    {
                        wallJumpTimeCounter -= Time.deltaTime;
                    }
                }

                // Update dash cooldown
                if (dashCooldownTimeLeft > 0)
                {
                    dashCooldownTimeLeft -= Time.deltaTime;
                    if (dashCooldownTimeLeft <= 0)
                    {
                        canDash = true;
                    }
                }
            }
        }
        UpdateAnimationStates();
    }
    // step up for small climbing
    private void CheckAndStepUp()
    {
        // Only attempt step-up when moving horizontally
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        if (horizontalInput == 0 || isClimbing) return;

        // Define step height parameters
        float stepHeight = 0.5f; // Maximum height to step up
        float stepCheckDistance = 0.5f; // Distance to check for obstacles

        // Determine direction based on input
        Vector2 moveDirection = horizontalInput > 0 ? Vector2.right : Vector2.left;

        // First, check for a horizontal obstacle
        RaycastHit2D horizontalHit = Physics2D.Raycast(
            transform.position,
            moveDirection,
            stepCheckDistance,
            groundLayer
        );

        if (horizontalHit.collider != null)
        {
            // If there's a horizontal obstacle, check if we can step up
            Vector2 stepUpOrigin = (Vector2)transform.position + moveDirection * stepCheckDistance;

            // Check if there's space above the obstacle
            RaycastHit2D verticalClearanceCheck = Physics2D.Raycast(
                stepUpOrigin,
                Vector2.up,
                stepHeight,
                groundLayer
            );

            // If there's no obstruction above, perform the step-up
            if (verticalClearanceCheck.collider == null)
            {
                // Smoothly move the player up
                transform.position += Vector3.up * stepHeight;

                // Optional: Add a small horizontal nudge to clear the obstacle
                rb.position += moveDirection * 0.1f;
            }
        }
    }
    // New method to check if player is on a ladder
    private void CheckLadder()
    {
        // Check if player is overlapping with a ladder
        Collider2D ladder = Physics2D.OverlapCircle(transform.position, 1f, ladderLayer);
        isOnLadder = ladder != null;

        // If player is no longer on ladder, exit climbing state
        if (!isOnLadder && isClimbing)
        {
            ExitLadder();
        }
    }

    // New method to handle ladder movement
    private void HandleLadderMovement(float verticalInput, float horizontalInput)
    {
        // If vertical input is provided while on ladder, enter climbing state
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            if (!isClimbing)
            {
                // Enter climbing state
                isClimbing = true;
                rb.gravityScale = 0;
                rb.velocity = Vector2.zero;
                animator.SetBool(IS_CLIMBING, true);
                animator.SetBool(IS_FALLING, false);
                animator.SetBool(IS_JUMPING, false);
            }

            // Move up/down on ladder
            rb.velocity = new Vector2(horizontalInput * moveSpeed * 0.5f, verticalInput * climbSpeed);

            // Optional: Play climbing animation based on input
            if (Mathf.Abs(verticalInput) > 0.1f)
            {
                animator.speed = Mathf.Abs(verticalInput);
            }
            else
            {
                animator.speed = 0; // Pause animation when not moving
            }
        }
        else if (isClimbing)
        {
            // If no vertical input while climbing, just stop vertical movement
            rb.velocity = new Vector2(horizontalInput * moveSpeed * 0.5f, 0);
            animator.speed = 0; // Pause animation
        }

        // Allow jumping off ladder
        if (Input.GetButtonDown("Jump") && isClimbing)
        {
            ExitLadder();
            Jump(jumpForce);
        }
    }

    // New method to exit ladder state
    private void ExitLadder()
    {
        isClimbing = false;
        rb.AddForce(Vector2.up * 2, ForceMode2D.Impulse); // Optional: Add a small jump off the ladder
        rb.gravityScale = 1; // Reset gravity
        animator.SetBool(IS_CLIMBING, false);
        animator.speed = 1; // Reset animation speed
    }

    private void UpdateAnimationStates()
    {
        // Skip normal animation updates if climbing
        if (isClimbing)
        {
            return;
        }

        // Update vertical velocity for blending or other effects
        animator.SetFloat(VERTICAL_VELOCITY, rb.velocity.y);

        if (rb.velocity.y > 0.1f)
        {
            animator.SetBool(IS_ON_AIR, true);
        }

        // Handle jump state changes
        if (rb.velocity.y < -0.1f && !isGrounded)
        {
            // We're falling
            animator.SetBool(IS_JUMPING, false);
            animator.SetBool(IS_FALLING, true);
        }
        else if (isGrounded)
        {
            // We've landed
            animator.SetBool(IS_ON_AIR, false);
            animator.SetBool(IS_FALLING, false);
        }

        // Ground state
        animator.SetBool(IS_GROUND, isGrounded);

        // Running state
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        animator.SetBool(IS_RUNNING, Mathf.Abs(horizontalInput) > 0.1f && isGrounded);
    }

    private void FixedUpdate()
    {
        if (!isDashing && !isClimbing)
        {
            ApplyJumpPhysics();
        }
    }

    private void ApplyJumpPhysics()
    {
        // Skip jump physics if climbing
        if (isClimbing)
            return;

        // Get the current gravity scale
        float gravityScale = 1f;

        // Apply higher gravity when falling
        if (rb.velocity.y < 0)
        {
            gravityScale = fallMultiplier;
        }
        // Apply lower gravity when jumping but not holding the jump button (for short jumps)
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            gravityScale = lowJumpMultiplier;
        }

        // Apply extra gravity when near the jump apex for snappier jumps
        if (Mathf.Abs(rb.velocity.y) < jumpApexThreshold)
        {
            gravityScale += jumpApexBonus;
        }

        // Apply the calculated gravity scale
        rb.velocity += Vector2.up * Physics2D.gravity.y * (gravityScale - 1) * Time.fixedDeltaTime;
    }

    private void UpdateMovementAnimations(float horizontalInput)
    {
        // Handle sprite    
        if (horizontalInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1, 1);
        }

        // If dashing, treat it like running
        if (isDashing)
        {
            animator.SetBool(IS_RUNNING, true);
        }
        else
        {
            // If there's horizontal input, run; otherwise, idle
            //if (Mathf.Abs(horizontalInput) > 0.1f)
            //{
            //    animator.SetBool(IS_RUNNING, true);
            //}
            //else
            //{
            //    animator.SetBool(IS_RUNNING, false);
            //}
        }
    }

    private void InitiateDash(float horizontalInput, float verticalInput)
    {
        isDashing = true;
        canDash = false;
        dashTimeLeft = dashDuration;
        dashCooldownTimeLeft = dashCooldown;

        // Calculate dash direction
        dashDirection = new Vector2(horizontalInput, verticalInput).normalized;
        if (dashDirection == Vector2.zero)
        {
            dashDirection = new Vector2(facingDirection, 0f);
        }
    }

    private void UpdateDash()
    {
        if (isDashing)
        {
            if (dashTimeLeft > 0)
            {
                // Apply dash velocity
                rb.velocity = dashDirection * dashSpeed;
                dashTimeLeft -= Time.deltaTime;
            }
            else
            {
                // End dash
                isDashing = false;
                // Optional: maintain some momentum
                rb.velocity = dashDirection * moveSpeed;
            }
        }
    }

    private void CheckGrounded()
    {
        // Skip ground check if climbing
        if (isClimbing)
            return;

        // Use an overlap circle for a more robust ground check on moving platforms.
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;

        // If just landed, update animations.
        if (!wasGrounded && isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            canDoubleJump = true;
            animator.SetBool(IS_FALLING, false);
            animator.SetBool(IS_JUMPING, false);
        }
        else if (wasGrounded && !isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        animator.SetBool(IS_GROUND, isGrounded);

        if (isGrounded && animator.GetBool(IS_FALLING))
        {
            animator.SetBool(IS_FALLING, false);
            string targetAnim = animator.GetBool(IS_RUNNING) ? "player_run" : "player_idle_sword";
            animator.Play(targetAnim, 0, 0f);
        }
    }

    private void CheckWallSliding(float horizontalInput)
    {
        // Skip wall check if climbing
        if (isClimbing)
            return;

        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, groundLayer);

        isWallRight = hitRight.collider != null;
        bool isWallLeft = hitLeft.collider != null;

        // Update the condition to allow wall sliding even without input
        isWallSliding = (isWallRight || isWallLeft) && !isGrounded;

        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlidingSpeed));
        }
    }

    private void Jump(float force)
    {
        rb.velocity = new Vector2(rb.velocity.x, force);
        animator.SetBool(IS_JUMPING, true);
        animator.SetBool(IS_ON_AIR, true);
        // Optional: Force the immediate transition
        animator.Play("player_jump", 0, 0f);
    }

    private void WallJump()
    {
        wallJumpTimeCounter = wallJumpTime;
        float wallJumpDirection = isWallRight ? -1 : 1;
        rb.velocity = new Vector2(wallJumpDirection * wallJumpForce, wallJumpUpForce);
        animator.SetBool(IS_JUMPING, true);
        animator.Play("player_jump", 0, 0f);
    }

    private void OnDrawGizmos()
    {
        // Draw debug rays for ground and wall checks
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector2.down * groundCheckDistance);
        Gizmos.DrawRay(transform.position, Vector2.right * wallCheckDistance);
        Gizmos.DrawRay(transform.position, Vector2.left * wallCheckDistance);

        // Draw ladder check radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }

    public bool IsFacingRight()
    {
        return facingDirection == 1;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }
}