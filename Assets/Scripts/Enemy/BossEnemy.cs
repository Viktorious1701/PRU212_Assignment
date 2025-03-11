using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;

public class BossEnemy : Enemy
{
    [Header("Boss Settings")]
    [SerializeField] private int maxPhases = 3;
    [SerializeField] private float[] phaseHealthThresholds = { 0.7f, 0.3f }; // 70% and 30% health triggers phase changes
    [SerializeField] private float[] phaseMovementSpeedMultipliers = { 1f, 1.3f, 1.6f }; // Speed increases with each phase
    [SerializeField] private float[] phaseDamageMultipliers = { 1f, 1.2f, 1.5f }; // Damage increases with each phase
    [SerializeField] private Color[] phaseColors = { Color.white, Color.red, Color.magenta }; // Visual indicator for phases

    [Header("Attack Patterns")]
    [SerializeField] private float specialAttackRange = 3f; // Range for special attacks
    [SerializeField] private float specialAttackCooldown = 5f; // Cooldown between special attacks
    [SerializeField] private float dashSpeed = 8f; // Speed when performing dash attack
    [SerializeField] private float dashDuration = 0.5f; // How long a dash lasts
    [SerializeField] private float jumpForce = 10f; // Force for jump attacks
    [SerializeField] private float aoeRadius = 3f; // Area of effect for ground pound attack
    [SerializeField] private float projectileSpeed = 7f; // Speed of projectiles
    [SerializeField] private GameObject projectilePrefab; // Projectile prefab for ranged attacks
    [SerializeField] private Transform firePoint; // Where projectiles spawn from
    [SerializeField] private LayerMask groundLayer; // For ground detection


    [Header("Shield Settings")]
    [SerializeField] private float shieldCooldown = 12f; // Time between shield activations
    [SerializeField] private float shieldDuration = 5f; // How long shield lasts
    [SerializeField] private float shieldDamageReduction = 0.5f; // Damage reduction (0-1)
    [SerializeField] private GameObject shieldVFX; // Visual effect for shield
    [SerializeField] private Color shieldColor = new Color(0.3f, 0.8f, 1f, 0.5f); // Shield color

    [Header("Phase Transition")]
    [SerializeField] private float immunityDuration = 3f; // How long immunity lasts during phase transition
    [SerializeField] private GameObject phaseTransitionVFX; // Visual effect for phase transition
    [SerializeField] private float knockbackForce = 5f; // Force to push nearby players during transition

    [Header("Effects")]
    [SerializeField] private GameObject groundSlamEffectPrefab; // Effect for ground slam attack
    private GameObject groundSlamEffect;

    // State tracking for boss
    private int currentPhase = 1;
    private bool canUseSpecialAttack = true;
    private bool isDashing = false;
    private bool isJumping = false;
    private bool isShieldActive = false;
    private bool canUseShield = true;
    private bool isImmune = false;
    private float maxHealth;
    private bool isDead = false;
    private Vector2 startPosition; // For returning to after certain attacks

    // Boss specific states
    private enum BossAttackType { MeleeSlash, DashAttack, JumpAttack, RangedAttack, ShieldCast }
    private BossAttackType currentAttackType;

    // Shield object reference
    private GameObject activeShieldObject;

    // Ground check
    private float groundCheckDistance = 2.5f;



    protected override void Awake()
    {
        base.Awake();

        startPosition = transform.position;
        maxHealth = health;
        // Ensure current phase starts at 1
        currentPhase = 1;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        //StartCoroutine(SpecialAttackCooldownRoutine());
        StartCoroutine(ShieldCooldownRoutine());
    }

    protected new void Update()
    {
        if(isDashing)
        CheckPlayerDamageCollision();

        // Check ground status
        CheckGroundStatus();

        // Check for phase transitions
        CheckPhaseTransition();

        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdleState();
                break;
            case EnemyState.Patrol:
                UpdatePatrolState();
                break;
            case EnemyState.Chase:
                UpdateChaseState();
                break;
            case EnemyState.Attack:
                UpdateAttackState();
                break;
            case EnemyState.Hurt:
                UpdateHurtState();
                break;
            case EnemyState.Death:
                UpdateDeathState();
                break;
        }

        // Run the base update logic which handles state machine
        UpdateAnimation();
    }

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();
        if (animator != null)
        {
            animator.SetInteger("Phase", currentPhase);
            animator.SetBool("IsDashing", isDashing);
            animator.SetBool("IsJumping", isJumping);
            animator.SetBool("IsShielded", isShieldActive);
            animator.SetBool("IsImmune", isImmune);
        }
    }

    #region State Management

    protected override void UpdateIdleState()
    {
        // Stop movement
        rb.velocity = new Vector2(0, rb.velocity.y);

        // Transition to chase if player is in detection range
        if (IsPlayerInRange(detectionRange))
        {
            currentState = EnemyState.Chase;
        }
    }

    protected override void UpdatePatrolState()
    {
        // Boss doesn't patrol, just transitions back to idle or chase
        if (IsPlayerInRange(detectionRange))
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            currentState = EnemyState.Idle;
        }
    }

    protected override void UpdateChaseState()
    {
        if (isAttacking || isDashing || isJumping || isImmune) return;

        // Check if player is in attack range
        if (IsPlayerInRange(attackRange) && canAttack)
        {
            currentState = EnemyState.Attack;
        }

        // Check if player is in special attack range and we can use a special attack
        if (IsPlayerInRange(specialAttackRange) && canUseSpecialAttack)
        {
            // Choose a special attack based on current phase
            ChooseAttackPattern();
            currentState = EnemyState.Attack;
            return;
        }

        // Consider using shield when health is below 50% in the current phase threshold
        // and player is getting close but not in melee range yet
        //if (CanActivateShield() && IsPlayerInRange(specialAttackRange) && !IsPlayerInRange(attackRange))
        //{
        //    currentAttackType = BossAttackType.ShieldCast;
        //    currentState = EnemyState.Attack;
        //    return;
        //}

        // Chase the player
        float direction = IsPlayerToRight() ? 1 : -1;
        float currentSpeed = movementSpeed * phaseMovementSpeedMultipliers[currentPhase - 1];
        rb.velocity = new Vector2(direction * currentSpeed, rb.velocity.y);

        // Flip if needed
        if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
            Flip();
    }

    protected override void UpdateAttackState()
    {
        float direction = IsPlayerToRight() ? 1 : -1;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        // Flip if needed
        if (((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight)) && distanceToPlayer > 1f)
            Flip();

        // Regular attack handling
        if (!isAttacking && canAttack && !isDashing && !isJumping && !isImmune)
        {
            ChooseAttackPattern();
            rb.velocity = new Vector2(0, rb.velocity.y);
            isAttacking = true;
            PerformAttack();
            StartCoroutine(AttackCooldown());
        }


        // Return to chase once attack is complete
        if (!isAttacking && !isDashing && !isJumping && !isImmune)
        {
            currentState = EnemyState.Chase;
        }
    }

    protected override void UpdateHurtState()
    {
        // If immune, skip hurt state
        if (isImmune)
        {
            currentState = EnemyState.Chase;
            return;
        }

        // Stop all active attacks
        if (!isShieldActive)
        {
            animator.SetTrigger("Hurt");

            // Don't stop ALL coroutines, just attack-related ones
            // Instead of StopAllCoroutines(), stop specific ones:
            StopAttackCoroutines();

            isDashing = false;
            isJumping = false;
            isAttacking = false; // Reset this flag too

            // Apply knockback
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("glow") && stateInfo.normalizedTime < 0.1f)
            {
                Vector2 knockbackDirection = transform.position - player.position;
                knockbackDirection.Normalize();
                rb.velocity = knockbackDirection * 4f;
            }

            // Transition back to chase after hurt animation
            if (stateInfo.normalizedTime >= 0.9f)
            {
                currentState = EnemyState.Chase;

                // Always ensure these are restarted
                if (!canUseSpecialAttack)
                    StartCoroutine(SpecialAttackCooldownRoutine());
                if (!canUseShield)
                    StartCoroutine(ShieldCooldownRoutine());
            }
        }
        else
        {
            // If shield is active, just flash the shield and return to previous state
            if (activeShieldObject != null)
            {
                // Flash the shield effect
                StartCoroutine(FlashShield());
            }

            // Skip hurt state animation
            currentState = EnemyState.Chase;
        }
    }

    private void StopAttackCoroutines()
    {
        // Find and stop specific attack coroutines
        StopCoroutine(DashAttack());
        StopCoroutine(JumpAttack());
        StopCoroutine(RangedAttack());
        StopCoroutine(ActivateShieldSequence());

        // Don't stop cooldown coroutines
    }

    protected override void UpdateDeathState()
    {
        animator.SetTrigger("Death");
        // Stop all movement and attacks
        rb.velocity = Vector2.zero;
        StopAllCoroutines();

        // Deactivate shield if active
        DeactivateShield();

       

        // Disable colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
            col.enabled = false;
        // Drop rewards, trigger events, etc.
        if (!isDead)
        {
            isDead = true;
            //DropRewards();
            //TriggerDeathEvents();
            GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        }
        // Wait for death animation to finish
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("death") && stateInfo.normalizedTime >= 0.9f)
        {
           
            // Destroy after delay for dramatic effect
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {

        // Optional: Add particle effects, screen shake, etc.
        GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    #endregion

    #region Attack Management

    protected override void PerformAttack()
    {
        // Set the attack type if not already set (for basic attacks)
        if (currentAttackType == BossAttackType.MeleeSlash || !canUseSpecialAttack)
        {
            currentAttackType = BossAttackType.MeleeSlash;
        }
        

        if(currentAttackType == BossAttackType.MeleeSlash && !IsPlayerInRange(attackRange))
        {
            currentState = EnemyState.Chase;
            isAttacking = false;
            canAttack = true;
            return;
        }
       
        switch (currentAttackType)
        {
            case BossAttackType.MeleeSlash:
                // Basic melee attack handled by animation event
                animator.SetTrigger("MeleeAttack");
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;

            case BossAttackType.DashAttack:
                StartCoroutine(DashAttack());
                break;

            case BossAttackType.JumpAttack:
                StartCoroutine(JumpAttack());
                break;

            case BossAttackType.RangedAttack:
                StartCoroutine(RangedAttack());
                break;

            case BossAttackType.ShieldCast:
                StartCoroutine(ActivateShieldSequence());
                break;
        }
    }

    private void ChooseAttackPattern()
    {
        if (!canUseSpecialAttack) return;

        // Different phases have different attack possibilities
        List<BossAttackType> possibleAttacks = new List<BossAttackType>();

        // Phase 1: Only dash attack available as special
        if (currentPhase == 1)
        {
            possibleAttacks.Add(BossAttackType.DashAttack);
        }
        // Phase 2: Dash and ranged attacks
        else if (currentPhase == 2)
        {
            possibleAttacks.Add(BossAttackType.DashAttack);
            possibleAttacks.Add(BossAttackType.RangedAttack);
        }
        // Phase 3: All attacks available
        else
        {
            possibleAttacks.Add(BossAttackType.DashAttack);
            possibleAttacks.Add(BossAttackType.JumpAttack);
            possibleAttacks.Add(BossAttackType.RangedAttack);
        }

        // Randomly select an attack
        int attackIndex = Random.Range(0, possibleAttacks.Count);
        currentAttackType = possibleAttacks[attackIndex];

        // Trigger the attack state
        currentState = EnemyState.Attack;
    }

    private IEnumerator DashAttack()
    {
        Debug.Log("Dash Attack!");
        isDashing = true;

        // Play dash attack animation
        animator.SetTrigger("DashAttack");

        // Wait for animation wind-up
        yield return new WaitForSeconds(0.75f);

        // Direction toward player
        float direction = IsPlayerToRight() ? 1 : -1;

        // Apply dash velocity
        rb.velocity = new Vector2(direction * dashSpeed, rb.velocity.y);

        // Enable damage hitbox during dash
        // This would be a separate collider specifically for the dash attack

        // Wait for dash duration
        yield return new WaitForSeconds(dashDuration);

        // Reset dash state
        rb.velocity = new Vector2(0, rb.velocity.y);
        isDashing = false;
        isAttacking = false;

        // Start special attack cooldown
        StartCoroutine(SpecialAttackCooldownRoutine());
    }
    private void CheckPlayerDamageCollision()
    {
        if (isDashing || isJumping)
        {
            Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(
                transform.position,
                new Vector2(attackRange * 0.8f, attackRange * 0.8f),
                0,
                LayerMask.GetMask("Player")
            );

            foreach (Collider2D playerCollider in hitPlayers)
            {
                float phaseDamage = damage * phaseDamageMultipliers[currentPhase - 1];
                DealDamageToPlayer(phaseDamage);
            }
        }
    }
    private IEnumerator JumpAttack()
    {
        Debug.Log("Jump Attack!");
        isJumping = true;

        // Play jump attack animation
        animator.SetTrigger("JumpAttack");

        // Wait for animation wind-up
        yield return new WaitForSeconds(0.2f);

        // Calculate jump direction (toward player)
        Vector2 jumpDirection = player.position - transform.position;
        jumpDirection.Normalize();

        // Apply jump force
        rb.velocity = new Vector2(jumpDirection.x * movementSpeed, jumpForce);

        // Wait until at apex of jump
        yield return new WaitUntil(() => rb.velocity.y <= 0);

        rb.gravityScale = 2f; // Increase gravity for faster descent

        // Wait until landed
        yield return new WaitUntil(() => isGrounded);

        rb.gravityScale = 1f; // Reset gravity

        // Ground pound effect when landing
        animator.SetTrigger("GroundPound");

        // Shake camera/screen
        // CameraShake.Instance.ShakeCamera(0.5f, 0.2f);

        // Deal AOE damage
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, aoeRadius, LayerMask.GetMask("Player"));
        foreach (Collider2D obj in hitObjects)
        {
            // Deal damage with current phase multiplier
            float phaseDamage = damage * phaseDamageMultipliers[currentPhase - 1];
            DealDamageToPlayer(phaseDamage * 1.5f); // Ground pound does more damage
        }

        StartCoroutine(GroundSlamEffect());

        // Reset jump state
        isJumping = false;
        isAttacking = false;

        // Start special attack cooldown
        StartCoroutine(SpecialAttackCooldownRoutine());
    }

    private IEnumerator GroundSlamEffect()
    {
        GameObject groundSlamEffect = Instantiate(groundSlamEffectPrefab, transform.position, Quaternion.identity);
        groundSlamEffect.SetActive(false);
        if (groundSlamEffect)
        {
            groundSlamEffect.SetActive(true);
            CinemachineImpulseSource temp = GetComponent<CinemachineImpulseSource>();
            if(temp != null)
            {
                temp.GenerateImpulse();
            }
            yield return new WaitForSeconds(0.65f);
            groundSlamEffect.GetComponent<SpriteRenderer>().enabled = false;
            yield return new WaitForSeconds(1.5f);
            Destroy(groundSlamEffect);
        }
        yield return null;
    }

    private IEnumerator RangedAttack()
    {
        Debug.Log("Ranged Attack!");
        // Play ranged attack animation
        animator.SetTrigger("RangedAttack");
        rb.velocity = new Vector2(0, rb.velocity.y);
        // Wait for animation wind-up
        yield return new WaitForSeconds(0.4f);

        // Number of projectiles based on phase
        int projectileCount = currentPhase;

        for (int i = 0; i < projectileCount; i++)
        {
            if (firePoint != null && projectilePrefab != null)
            {
                // Spawn projectile
                GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

                // Calculate direction toward player with slight variation for multiple projectiles
                Vector2 direction = player.position - firePoint.position;
                if (projectileCount > 1)
                {
                    // Add spread for multiple projectiles
                    float spread = 0.2f * (i - (projectileCount - 1) / 2f);
                    direction = Quaternion.Euler(0, 0, spread * 45f) * direction;
                }
                direction.Normalize();

                // Set up the projectile using your existing ProjectileController
                ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
                if (projectileController != null)
                {
                    float phaseDamage = damage * phaseDamageMultipliers[currentPhase - 1];
                    float projectileRange = 15f; // Maximum range before auto-destroying

                    projectileController.Initialize(phaseDamage, projectileRange, gameObject, direction);
                    projectileController.speed = projectileSpeed;
                }
            }

            // Brief delay between multiple projectiles
            if (projectileCount > 1 && i < projectileCount - 1)
            {
                yield return new WaitForSeconds(0.15f);
            }
        }

        // Reset attack state
        isAttacking = false;

        // Start special attack cooldown
        StartCoroutine(SpecialAttackCooldownRoutine());
    }

    private IEnumerator ActivateShieldSequence()
    {
        if (!canUseShield || isShieldActive)
        {
            isAttacking = false;
            yield break;
        }

        // Play shield cast animation
        animator.SetTrigger("ShieldCast");

        // Wait for animation
        yield return new WaitForSeconds(0.5f);

        // Activate shield
        ActivateShield();

        // Reset attack state
        isAttacking = false;

        // Start shield cooldown
        StartCoroutine(ShieldCooldownRoutine());
    }

    private void ActivateShield()
    {
        if (isShieldActive) return;

        isShieldActive = true;
        canUseShield = false;

        // Create shield visual effect
        if (shieldVFX != null)
        {
            activeShieldObject = Instantiate(shieldVFX, transform.position, Quaternion.identity, transform);

            // Set shield color if it has a renderer
            Renderer shieldRenderer = activeShieldObject.GetComponent<Renderer>();
            if (shieldRenderer != null)
            {
                shieldRenderer.material.color = shieldColor;
            }
        }

        // Deactivate shield after duration
        StartCoroutine(DeactivateShieldAfterDuration());
    }

    private void DeactivateShield()
    {
        if (!isShieldActive) return;

        isShieldActive = false;

        // Destroy shield visual effect
        if (activeShieldObject != null)
        {
            Destroy(activeShieldObject);
            activeShieldObject = null;
        }
    }

    private IEnumerator DeactivateShieldAfterDuration()
    {
        yield return new WaitForSeconds(shieldDuration);
        DeactivateShield();
    }

    private IEnumerator FlashShield()
    {
        // Flash the shield when hit to provide visual feedback
        if (activeShieldObject != null)
        {
            Renderer shieldRenderer = activeShieldObject.GetComponent<Renderer>();
            if (shieldRenderer != null)
            {
                Color originalColor = shieldRenderer.material.color;
                shieldRenderer.material.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                shieldRenderer.material.color = originalColor;
            }
        }
    }

    // Called by animation event
    public void ApplyDamage()
    {
        // For melee attacks
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Player"));
        foreach (Collider2D playerCollider in hitPlayers)
        {
            float phaseDamage = damage * phaseDamageMultipliers[currentPhase - 1];
            DealDamageToPlayer(phaseDamage);
        }

        isAttacking = false;
    }

    private IEnumerator SpecialAttackCooldownRoutine()
    {
        canUseSpecialAttack = false;
        yield return new WaitForSeconds(specialAttackCooldown); // Shorter cooldown in later phases
        canUseSpecialAttack = true;
    }

    private IEnumerator ShieldCooldownRoutine()
    {
        canUseShield = false;
        yield return new WaitForSeconds(shieldCooldown);
        canUseShield = true;
    }

    #endregion

    #region Health and Damage

    public override void HandleDamage(GameObject target, DamageInfo damageInfo)
    {
        // Only process damage for this gameObject
        if (target != gameObject) return;

        // If immune, don't take damage
        if (isImmune)
        {
            // Maybe play a "deflect" effect or animation
            return;
        }

        // If shield is active, reduce damage
        if (isShieldActive)
        {
            // Modify the damage info to reduce damage
            damageInfo = new DamageInfo(
                damageInfo.damageAmount * (1 - shieldDamageReduction),
                damageInfo.damageSource,
                damageInfo.hitPoint,
                damageInfo.hitDirection
            );

            // Flash the shield to show it absorbed some damage
            StartCoroutine(FlashShield());
        }

        // Pass to base class to apply the damage
        base.HandleDamage(target, damageInfo);
    }

    #endregion

    #region Phase Management

    private void CheckPhaseTransition()
    {
        if (currentState == EnemyState.Death || isImmune) return;

        // Calculate current health percentage
        float healthPercentage = healthComponent.GetCurrentHealth() / maxHealth;

        // Check for phase transitions
        if (currentPhase < maxPhases)
        {
            // Check if health dropped below threshold for next phase
            if (healthPercentage <= phaseHealthThresholds[currentPhase - 1])
            {
                TransitionToNextPhase();
            }
        }
    }

    private void TransitionToNextPhase()
    {
        // Stop all actions
        StopAttackCoroutines();

        // Clear any active effects
        isAttacking = false;
        isDashing = false;
        isJumping = false;
        DeactivateShield();

        // Start phase transition
        StartCoroutine(PhaseTransitionSequence());
    }

    private IEnumerator PhaseTransitionSequence()
    {
        // Enter immunity state
        isImmune = true;
        // Reset attack states
        isAttacking = false;
        isDashing = false;
        isJumping = false;
        DeactivateShield();

        // Clear attack coroutines but don't stop ALL coroutines
        StopAttackCoroutines();

        // Stop movement
        rb.velocity = Vector2.zero;

        // Play phase transition animation
        animator.SetTrigger("PhaseTransition");

        // Spawn VFX
        if (phaseTransitionVFX != null)
        {
            GameObject vfx = Instantiate(phaseTransitionVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }
        // Wait for animation
        yield return new WaitForSeconds(immunityDuration);

        // Create a shockwave that pushes players away
        Collider2D[] nearbyPlayers = Physics2D.OverlapCircleAll(transform.position, detectionRange, LayerMask.GetMask("Player"));
        foreach (Collider2D playerCollider in nearbyPlayers)
        {
            if (playerCollider.attachedRigidbody != null)
            {
                Vector2 direction = (playerCollider.transform.position - transform.position).normalized;
                if (playerCollider.TryGetComponent<PlayerMovement>(out PlayerMovement movement))
                {
                    Vector2 force = (direction + Vector2.up) * knockbackForce;
                    movement.ApplyKnockback(force, 0.5f);
                }
                else if (playerCollider.attachedRigidbody != null)
                {
                    // Fallback for objects without PlayerMovement
                    playerCollider.attachedRigidbody.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
                }
                GetComponent<CinemachineImpulseSource>().GenerateImpulse();
                // Apply small damage to player
                DamageInfo damageInfo = new DamageInfo(
                    damage * 0.25f,
                    gameObject,
                    playerCollider.transform.position,
                    direction
                );

                DamageSystem.ApplyDamage(playerCollider.gameObject, damageInfo);
            }
        }



        // Update phase
        currentPhase++;

        // Update visual appearance
        if (spriteRenderer != null && phaseColors.Length >= currentPhase)
        {
            spriteRenderer.color = phaseColors[currentPhase - 1];
        }

        StartCoroutine(SpecialAttackCooldownRoutine());
        StartCoroutine(ShieldCooldownRoutine());

        // End immunity state
        isImmune = false;

        // Reset cooldowns for attacks
        canUseShield = true;
        canAttack = true;

        // Return to chase state
        currentState = EnemyState.Chase;
    }

    #endregion

    #region Helper Methods

    private void CheckGroundStatus()
    {
        // Cast a small ray to check if the boss is grounded
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
    }

    private bool CanActivateShield()
    {
        if (!canUseShield || isShieldActive) return false;

        // Check health threshold for current phase
        float healthPercentage = healthComponent.GetCurrentHealth() / maxHealth;

        // Phase 1: Only use shield below 50% health in this phase
        if (currentPhase == 1)
        {
            float phaseThreshold = (currentPhase < maxPhases) ? phaseHealthThresholds[currentPhase - 1] : 0;
            return healthPercentage < (1.0f + phaseThreshold) / 2.0f;
        }
        // Later phases: More aggressive with shield use
        else
        {
            return true;
        }
    }

    #endregion

    private void OnDrawGizmos()
    {
        // Draw attack ranges
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, specialAttackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, groundCheckDistance);
    }
}