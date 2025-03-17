using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Dialogue dialogueSystem;

    // Animation parameter names
    private const string IS_RUNNING = "isRunning";
    private const string IS_GROUND = "isGround";
    private const string IS_JUMPING = "isJumping";
    private const string IS_FALLING = "isFalling";
    private const string IS_ON_AIR = "isOnAir";
    private const string VERTICAL_VELOCITY = "verticalVelocity";
    private const string IS_CLIMBING = "isClimbing";

    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float wallJumpForce = 14f;
    [SerializeField] private float wallJumpUpForce = 12f;
    [SerializeField] private float wallSlidingSpeed = 2f;
    [SerializeField] private GameObject firePoint;

    [Header("Platform Parameters")]
    [SerializeField] private float platformJumpMultiplier = 1.5f;
    [SerializeField] private LayerMask movingPlatformLayer;

    [Header("Ladder Parameters")]
    [SerializeField] private float climbSpeed = 5f;
    [SerializeField] private LayerMask ladderLayer;

    [Header("Jump Physics")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float jumpApexThreshold = 2f;
    [SerializeField] private float jumpApexBonus = 0.5f;

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
    private bool canDoubleJump = true;
    private float coyoteTimeCounter;
    private float wallJumpTimeCounter;
    private float jumpBufferCounter;
    private int facingDirection = 1;
    private bool isWallRight;

    // Platform tracking
    private Transform currentPlatform;
    private Vector2 platformVelocity;
    private bool isOnMovingPlatform;

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
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private float wallCheckDistance = 0.5f;

    private bool isKnockedBack = false;
    private float knockbackEndTime = 0f;

    public void ApplyKnockback(Vector2 force, float duration)
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
        isKnockedBack = true;
        knockbackEndTime = Time.time + duration;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        dialogueSystem = FindObjectOfType<Dialogue>();
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
                return;
            }
        }

        if (!dialogueSystem.isDialogueActive)
        {
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

            // Perform collision checks
            CheckGrounded();
            CheckWallSliding(horizontalInput);
            CheckLadder();

            // Handle ladder climbing
            if (isOnLadder && Mathf.Abs(verticalInput) > 0.1f)
            {
                if (!isClimbing)
                {
                    isClimbing = true;
                    rb.gravityScale = 0;
                    rb.velocity = Vector2.zero;
                    animator.SetBool(IS_CLIMBING, true);
                }
                rb.velocity = new Vector2(horizontalInput * moveSpeed * 0.5f, verticalInput * climbSpeed);
                animator.speed = Mathf.Abs(verticalInput) > 0.1f ? Mathf.Abs(verticalInput) : 0;
            }
            else if (isClimbing && !isOnLadder)
            {
                ExitLadder();
            }

            // Allow jumping off ladder
            if (jumpInput && isClimbing)
            {
                ExitLadder();
                PerformJump();
            }

            // Handle normal movement if not climbing
            if (!isClimbing)
            {
                // Dash handling
                if (dashInput && canDash && (canDashInAir || isGrounded))
                {
                    InitiateDash(horizontalInput, verticalInput);
                }
                UpdateDash();

                if (!isDashing)
                {
                    // Jump buffer
                    if (jumpInput)
                    {
                        jumpBufferCounter = jumpBufferTime;
                    }
                    else
                    {
                        jumpBufferCounter -= Time.deltaTime;
                    }

                    // Handle jumps
                    if (jumpBufferCounter > 0f)
                    {
                        if (isGrounded || coyoteTimeCounter > 0f)
                        {
                            PerformJump();
                            jumpBufferCounter = 0f;
                            if (!isGrounded) coyoteTimeCounter = 0f;
                        }
                        else if (isWallSliding)
                        {
                            WallJump();
                            jumpBufferCounter = 0f;
                        }
                        else if (canDoubleJump)
                        {
                            Jump(jumpForce * 0.8f);
                            canDoubleJump = false;
                            jumpBufferCounter = 0f;
                        }
                    }

                    // Update coyote time
                    if (!isGrounded && coyoteTimeCounter > 0f)
                    {
                        coyoteTimeCounter -= Time.deltaTime;
                    }

                    // Wall sliding velocity
                    if (isWallSliding)
                    {
                        rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlidingSpeed));
                    }

                    // Horizontal movement
                    if (wallJumpTimeCounter <= 0)
                    {
                        if (!isWallSliding)
                        {
                            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
                        }
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
                    if (dashCooldownTimeLeft <= 0) canDash = true;
                }
            }
        }

        UpdateAnimationStates();
    }

    private void FixedUpdate()
    {
        if (dialogueSystem.isDialogueActive)
        {
            rb.velocity = Vector2.zero;
        }
        else if (!isDashing && !isClimbing)
        {
            ApplyJumpPhysics();
        }
    }

    private void PerformJump()
    {
        float actualJumpForce = jumpForce;
        if (isOnMovingPlatform)
        {
            if (platformVelocity.y > 0)
            {
                actualJumpForce += platformVelocity.y * platformJumpMultiplier;
            }
            else if (platformVelocity.y < 0)
            {
                actualJumpForce *= 1.2f;
            }
        }
        Jump(actualJumpForce);
    }

    private void CheckLadder()
    {
        Collider2D ladder = Physics2D.OverlapCircle(transform.position, 0.3f, ladderLayer);
        isOnLadder = ladder != null;
        if (!isOnLadder && isClimbing)
        {
            ExitLadder();
        }
    }

    private void ExitLadder()
    {
        isClimbing = false;
        rb.gravityScale = 1;
        animator.SetBool(IS_CLIMBING, false);
        animator.speed = 1;
    }

    private void ApplyJumpPhysics()
    {
        if (isClimbing) return;

        float gravityScale = 1f;
        if (rb.velocity.y < 0)
        {
            gravityScale = fallMultiplier;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            gravityScale = lowJumpMultiplier;
        }
        if (Mathf.Abs(rb.velocity.y) < jumpApexThreshold)
        {
            gravityScale += jumpApexBonus;
        }
        rb.velocity += Vector2.up * Physics2D.gravity.y * (gravityScale - 1) * Time.fixedDeltaTime;
    }

    private void UpdateAnimationStates()
    {
        if (isClimbing) return;

        animator.SetFloat(VERTICAL_VELOCITY, rb.velocity.y);
        if (rb.velocity.y > 0.1f)
        {
            animator.SetBool(IS_ON_AIR, true);
        }
        if (rb.velocity.y < -0.1f && !isGrounded)
        {
            animator.SetBool(IS_JUMPING, false);
            animator.SetBool(IS_FALLING, true);
        }
        else if (isGrounded)
        {
            animator.SetBool(IS_ON_AIR, false);
            animator.SetBool(IS_FALLING, false);
        }
        animator.SetBool(IS_GROUND, isGrounded);

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        animator.SetBool(IS_RUNNING, Mathf.Abs(horizontalInput) > 0.1f && isGrounded);
    }

    private void UpdateMovementAnimations(float horizontalInput)
    {
        if (horizontalInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1, 1);
        }
        if (isDashing)
        {
            animator.SetBool(IS_RUNNING, true);
        }
    }

    private void InitiateDash(float horizontalInput, float verticalInput)
    {
        isDashing = true;
        canDash = false;
        dashTimeLeft = dashDuration;
        dashCooldownTimeLeft = dashCooldown;
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
                rb.velocity = dashDirection * dashSpeed;
                dashTimeLeft -= Time.deltaTime;
            }
            else
            {
                isDashing = false;
                rb.velocity = dashDirection * moveSpeed;
            }
        }
    }

    private void CheckGrounded()
    {
        if (isClimbing) return;

        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;

        isOnMovingPlatform = false;
        platformVelocity = Vector2.zero;
        Collider2D platformCollider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, movingPlatformLayer);
        if (platformCollider != null)
        {
            isOnMovingPlatform = true;
            currentPlatform = platformCollider.transform;
            Rigidbody2D platformRb = platformCollider.GetComponent<Rigidbody2D>();
            if (platformRb != null)
            {
                platformVelocity = platformRb.velocity;
            }
            else
            {
                VerticalScrollingTilemap scrollingTilemap = platformCollider.GetComponent<VerticalScrollingTilemap>();
                if (scrollingTilemap != null)
                {
                    platformVelocity = new Vector2(0, scrollingTilemap.scrollSpeed);
                }
            }
        }

        if (!wasGrounded && isGrounded)
        {
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
        if (isClimbing) return;

        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, groundLayer);
        isWallRight = hitRight.collider != null;
        bool isWallLeft = hitLeft.collider != null;
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
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector2.down * groundCheckDistance);
        Gizmos.DrawRay(transform.position, Vector2.right * wallCheckDistance);
        Gizmos.DrawRay(transform.position, Vector2.left * wallCheckDistance);
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