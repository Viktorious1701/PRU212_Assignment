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
		}
	}

	protected override void UpdateChaseState()
	{
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
		// Stop or add recoil
		rb.velocity = new Vector2(0, rb.velocity.y);

		// Return to chase after hurt animation
		if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
		{
			currentState = EnemyState.Chase;
		}
	}

	protected override void UpdateDeathState()
	{
		// Stop all movement
		rb.velocity = Vector2.zero;

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

		// Check for player in attack range and deal damage
		Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Player"));
		foreach (Collider2D player in hitPlayers)
		{
			// Assuming player has a health component
			//player.GetComponent<PlayerHealth>()?.TakeDamage(damage);
		}

		// Reset attack state after animation
		StartCoroutine(FinishAttack());
	}

	private IEnumerator FinishAttack()
	{
		// Wait for attack animation to finish
		yield return new WaitForSeconds(0.5f);
		isAttacking = false;
		currentState = EnemyState.Chase;
	}
}
