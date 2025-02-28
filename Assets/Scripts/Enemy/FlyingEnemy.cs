using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy
{
	[SerializeField] private float flyHeight = 3f;
	[SerializeField] private float hoverAmplitude = 0.5f;
	[SerializeField] private float hoverFrequency = 2f;

	private Vector2 startPosition;
	private float hoverOffset = 0f;

	protected override void Awake()
	{
		base.Awake();
		startPosition = transform.position;
		// Flying enemies don't need gravity
		rb.gravityScale = 0;
	}

	protected override void UpdateIdleState()
	{
		// Hover in place
		Hover();
	}

	protected override void UpdatePatrolState()
	{
		// Random movement or circular pattern
		hoverOffset += Time.deltaTime * hoverFrequency;
		float xOffset = Mathf.Sin(hoverOffset) * 2;
		float yOffset = Mathf.Cos(hoverOffset) * hoverAmplitude;

		Vector2 targetPosition = startPosition + new Vector2(xOffset, flyHeight + yOffset);
		rb.velocity = (targetPosition - (Vector2)transform.position).normalized * movementSpeed;

		// Flip if needed
		if ((rb.velocity.x > 0 && !isFacingRight) || (rb.velocity.x < 0 && isFacingRight))
			Flip();
	}

	protected override void UpdateChaseState()
	{
		// Follow player but maintain some distance
		Vector2 direction = ((Vector2)player.position + Vector2.up - (Vector2)transform.position).normalized;
		rb.velocity = direction * movementSpeed * 1.5f;

		// Flip if needed
		if ((rb.velocity.x > 0 && !isFacingRight) || (rb.velocity.x < 0 && isFacingRight))
			Flip();
	}

	protected override void UpdateAttackState()
	{
		// Stop movement or slight hover during attack
		Hover();

		if (!isAttacking && canAttack)
		{
			isAttacking = true;
			PerformAttack();
			StartCoroutine(AttackCooldown());
		}
	}

	protected override void UpdateHurtState()
	{
		// Slight backward movement
		Vector2 recoilDirection = ((Vector2)transform.position - (Vector2)player.position).normalized;
		rb.velocity = recoilDirection * movementSpeed * 0.5f;

		// Return to chase after hurt animation
		if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
		{
			currentState = EnemyState.Chase;
		}
	}

	protected override void UpdateDeathState()
	{
		// Fall to ground
		rb.gravityScale = 1;

		// Wait for death animation to finish
		if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Death"))
		{
			// Destroy the enemy or disable components
			Destroy(gameObject, 1f);
		}
	}

	protected override void PerformAttack()
	{
		// Trigger attack animation
		animator.SetTrigger("Attack");

		// Projectile attack or diving attack
		// Example: Instantiate a projectile
		GameObject projectile = Instantiate(Resources.Load<GameObject>("EnemyProjectile"), transform.position, Quaternion.identity);

		// Aim at player
		Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
		projectile.GetComponent<Rigidbody2D>().velocity = direction * 10f;

		// Reset attack state after animation
		StartCoroutine(FinishAttack());
	}

	private IEnumerator FinishAttack()
	{
		yield return new WaitForSeconds(0.5f);
		isAttacking = false;
		currentState = EnemyState.Chase;
	}

	private void Hover()
	{
    		hoverOffset += Time.deltaTime * hoverFrequency;
		float yOffset = Mathf.Sin(hoverOffset) * hoverAmplitude;
		rb.velocity = new Vector2(0, yOffset);
	}
}
