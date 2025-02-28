using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrollingEnemy : Enemy
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitTime = 1f;

    private int currentPatrolIndex = 0;
    private float waitCounter = 0f;
    private bool isWaiting = false;

    protected override void UpdateIdleState()
    {
        // Just stand still and occasionally look around
        rb.velocity = new Vector2(0, rb.velocity.y);

        // Transition to patrol after some time
        waitCounter += Time.deltaTime;
        if (waitCounter >= waitTime)
        {
            waitCounter = 0;
            currentState = EnemyState.Patrol;
        }
    }

    protected override void UpdatePatrolState()
    {
        if (patrolPoints.Length == 0) return;

        if (isWaiting)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            waitCounter += Time.deltaTime;
            if (waitCounter >= waitTime)
            {
                waitCounter = 0;
                isWaiting = false;
            }
            return;
        }

        // Move toward current patrol point
        Transform target = patrolPoints[currentPatrolIndex];
        Vector2 direction = (target.position - transform.position).normalized;

        // Only move horizontally
        rb.velocity = new Vector2(direction.x * movementSpeed, rb.velocity.y);

        // Flip if needed
        if ((direction.x > 0 && !isFacingRight) || (direction.x < 0 && isFacingRight))
            Flip();

        // Check if we've reached the patrol point
        if (Vector2.Distance(transform.position, target.position) < 0.2f)
        {
            // Move to next patrol point
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            isWaiting = true;
            currentState = EnemyState.Idle;
        }
    }

    protected override void UpdateChaseState()
    {
        if (isAttacking) return;

        // Check distance-based transitions explicitly
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange && canAttack)
        {
            currentState = EnemyState.Attack;
            return;
        }
        else if (distanceToPlayer > detectionRange)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        // Move toward player
        float direction = IsPlayerToRight() ? 1 : -1;
        rb.velocity = new Vector2(direction * movementSpeed * 1.5f, rb.velocity.y);

        // Flip if needed
        if ((direction > 0 && !isFacingRight) || (direction < 0 && isFacingRight))
            Flip();
    }

    protected override void UpdateAttackState()
    {
        // Stop moving during attack
        rb.velocity = new Vector2(0, rb.velocity.y);

        if (!isAttacking && canAttack)
        {
            isAttacking = true;
            PerformAttack();
            StartCoroutine(AttackCooldown());
        }
    }
    protected override void UpdateHurtState()
    {
        // Apply recoil only once when entering the hurt state
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("skeleton_take_hit") && stateInfo.normalizedTime == 0f) // Apply at start of animation
        {
            rb.velocity = new Vector2(-2, rb.velocity.y); // Recoil left
        }

        // Wait for hurt animation to complete fully
        if (stateInfo.IsName("skeleton_take_hit") && stateInfo.normalizedTime >= 1f)
        {
            currentState = EnemyState.Chase;
        }
    }

    protected override void UpdateDeathState()
    {
        // Stop all movement
        rb.velocity = Vector2.zero;
        // Wait for death animation to finish
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("skeleton_death") && stateInfo.normalizedTime >= 0.9f)
        {
            // Destroy the enemy or disable components
            Destroy(gameObject);
        }
    }

    protected override void PerformAttack()
    {
        // Trigger attack animation
        animator.SetTrigger("Attack");
    }

    public void ApplyDamage()
    {
        // Check for player in attack range and deal damage
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Player"));
        foreach (Collider2D player in hitPlayers)
        {
            // Assuming player has a health component
            //player.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }
        isAttacking = false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        foreach (Transform point in patrolPoints)
        {
            Gizmos.DrawWireSphere(point.position, 0.2f);
        }
    }
}
