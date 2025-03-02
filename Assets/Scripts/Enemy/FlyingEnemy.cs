using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    [SerializeField] private float flyHeight = 3f;
    [SerializeField] private float hoverAmplitude = 0.5f;
    [SerializeField] private float hoverFrequency = 2f;
    [SerializeField] private GameObject projectilePrefab;

    private Vector2 startPosition;
    private float hoverOffset = 0f;

    protected override void Awake()
    {
        base.Awake();
        startPosition = transform.position;
        rb.gravityScale = 0; // Flying enemies don't need gravity
    }

    protected override void UpdateIdleState()
    {
        Hover(); // Hover in place
    }

    protected override void UpdatePatrolState()
    {
        hoverOffset += Time.deltaTime * hoverFrequency;
        float xOffset = Mathf.Sin(hoverOffset) * 2;
        float yOffset = Mathf.Cos(hoverOffset) * hoverAmplitude;

        Vector2 targetPosition = startPosition + new Vector2(xOffset, flyHeight + yOffset);
        rb.velocity = (targetPosition - (Vector2)transform.position).normalized * movementSpeed;

        if ((rb.velocity.x > 0 && !isFacingRight) || (rb.velocity.x < 0 && isFacingRight))
            Flip();
    }

    protected override void UpdateChaseState()
    {
        // Hover around player's head
        hoverOffset += Time.deltaTime * hoverFrequency;
        float xOffset = Mathf.Sin(hoverOffset);
        float yOffset = Mathf.Sin(hoverOffset) * hoverAmplitude;

        // Target position is above player's head
        Vector2 targetPosition = (Vector2)player.position + new Vector2(0, flyHeight) + new Vector2(xOffset, yOffset);
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

        // Only change the facing direction based on the player's position, not the hover movement
        if (player.position.x > transform.position.x && !isFacingRight)
            Flip();
        else if (player.position.x < transform.position.x && isFacingRight)
            Flip();

        rb.velocity = direction * movementSpeed * 1.5f;
    }

    protected override void UpdateAttackState()
    {

        // Hover around player's head during attack
        hoverOffset += Time.deltaTime * hoverFrequency;
        float xOffset = Mathf.Sin(hoverOffset);
        float yOffset = Mathf.Sin(hoverOffset) * hoverAmplitude;

        Vector2 targetPosition = (Vector2)player.position + new Vector2(0, flyHeight) + new Vector2(xOffset, yOffset);
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        rb.velocity = direction * movementSpeed * 0.5f; // Slower movement during attack

        // Only change the facing direction based on the player's position
        if (player.position.x > transform.position.x && !isFacingRight)
            Flip();
        else if (player.position.x < transform.position.x && isFacingRight)
            Flip();

        if (!isAttacking && canAttack)
        {
            currentState = EnemyState.Attack;
            isAttacking = true;
            PerformAttack();
            StartCoroutine(AttackCooldown());
        }
    }

    protected override void UpdateHurtState()
    {
        Vector2 recoilDirection = ((Vector2)transform.position - (Vector2)player.position).normalized;
        rb.velocity = recoilDirection * movementSpeed * 0.5f;

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("flying_eye_hurt"))
        {
            currentState = EnemyState.Chase;
        }
    }

    protected override void UpdateDeathState()
    {
        rb.gravityScale = 1; // Fall to ground

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("flying_eye_death"))
        {
            Destroy(gameObject);
        }
    }

    protected override void PerformAttack()
    {
        animator.SetTrigger("Attack");
        currentState = EnemyState.Attack;
    }

    public void ShootProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        

        ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
        if (projectileController != null)
        {
            projectileController.Initialize(10f, 15f, gameObject,direction);
            projectile.GetComponent<Rigidbody2D>().velocity = direction * 10f;
        }
        isAttacking = false;
        currentState = EnemyState.Chase;
    }

   

    private void Hover()
    {
        hoverOffset += Time.deltaTime * hoverFrequency;
        float yOffset = Mathf.Sin(hoverOffset) * hoverAmplitude;
        rb.velocity = new Vector2(-0.5f, yOffset); // Slight left drift in idle
    }
}