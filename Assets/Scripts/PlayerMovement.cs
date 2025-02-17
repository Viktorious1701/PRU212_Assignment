using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float wallJumpForce = 14f;
    [SerializeField] private float wallJumpUpForce = 12f;
    [SerializeField] private float wallSlidingSpeed = 2f;

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

    // Dash state
    private bool isDashing;
    private bool canDash = true;
    private float dashTimeLeft;
    private float dashCooldownTimeLeft;
    private Vector2 dashDirection;

    [Header("Collision Checks")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private float wallCheckDistance = 0.5f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
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

        // Ground and wall checks
        CheckGrounded();
        CheckWallSliding(horizontalInput);
        //CheckFaceDirection();

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

            // Handle jumping
            if (jumpBufferCounter > 0f)
            {
                // Normal jump
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
                else if (canDoubleJump)
                {
                    Jump(jumpForce * 0.8f);
                    canDoubleJump = false;
                    jumpBufferCounter = 0f;
                }
            }

            // Cut jump short if button released
            if (!jumpHeld && rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }

            // Handle wall sliding
            if (isWallSliding)
            {
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlidingSpeed));
            }

            // Handle horizontal movement
            if (wallJumpTimeCounter <= 0)
            {
                rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
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

    private void CheckFaceDirection()
    {
        
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

        // Optional: Add dash effects here
        // CreateDashEffect();
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

                // Optional: Create trail effect
                // CreateTrailEffect();
            }
            else
            {
                // End dash
                isDashing = false;
                // Optional: Maintain some momentum
                rb.velocity = dashDirection * moveSpeed;
            }
        }
    }

    private void CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
    }

    private void CheckWallSliding(float horizontalInput)
    {
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, groundLayer);

        isWallRight = hitRight.collider != null;
        bool isWallLeft = hitLeft.collider != null;

        isWallSliding = (isWallRight || isWallLeft) && !isGrounded &&
                        ((horizontalInput > 0 && isWallRight) || (horizontalInput < 0 && isWallLeft));
    }

    private void Jump(float force)
    {
        rb.velocity = new Vector2(rb.velocity.x, force);
    }

    private void WallJump()
    {
        wallJumpTimeCounter = wallJumpTime;
        float wallJumpDirection = isWallRight ? -1 : 1;
        rb.velocity = new Vector2(wallJumpDirection * wallJumpForce, wallJumpUpForce);
    }

    private void OnDrawGizmos()
    {
        // Draw debug rays for ground and wall checks
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector2.down * groundCheckDistance);
        Gizmos.DrawRay(transform.position, Vector2.right * wallCheckDistance);
        Gizmos.DrawRay(transform.position, Vector2.left * wallCheckDistance);
    }
}