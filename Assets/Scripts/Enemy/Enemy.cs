using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
	// Common properties
	[SerializeField] protected float movementSpeed = 3f;
	[SerializeField] protected float attackRange = 1.5f;
	[SerializeField] protected float detectionRange = 5f;
	[SerializeField] protected int health = 100;
	[SerializeField] protected int damage = 10;
	[SerializeField] protected float attackCooldown = 1.5f;

	// State tracking
	protected bool isAttacking = false;
	protected bool canAttack = true;
	protected bool isFacingRight = true;
	protected bool isGrounded = true;
	protected Transform player;
	protected Animator animator;
	protected Rigidbody2D rb;
	protected SpriteRenderer spriteRenderer;

	// States
	protected enum EnemyState { Idle, Patrol, Chase, Attack, Hurt, Death }
	protected EnemyState currentState = EnemyState.Idle;

	protected virtual void Awake()
	{
		// Get common components
		animator = GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		player = GameObject.FindGameObjectWithTag("Player").transform;
	}

	protected virtual void Update()
	{
		// State machine pattern
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

		// Check for state transitions
		EvaluateStateTransitions();

		// Update animations based on state
		UpdateAnimation();
	}

	// Abstract methods that subclasses must implement
	protected abstract void UpdateIdleState();
	protected abstract void UpdatePatrolState();
	protected abstract void UpdateChaseState();
	protected abstract void UpdateAttackState();
	protected abstract void UpdateHurtState();
	protected abstract void UpdateDeathState();
	protected abstract void PerformAttack();

	// Virtual methods that can be overridden
	protected virtual void EvaluateStateTransitions()
	{
		// Default implementation for state transitions
		if (health <= 0 && currentState != EnemyState.Death)
		{
			currentState = EnemyState.Death;
			return;
		}

		// Basic transitions based on player distance
		float distanceToPlayer = Vector2.Distance(transform.position, player.position);

		if (currentState != EnemyState.Hurt && currentState != EnemyState.Death)
		{
			if (distanceToPlayer <= attackRange && canAttack)
			{
				currentState = EnemyState.Attack;
			}
			else if (distanceToPlayer <= detectionRange)
			{
				currentState = EnemyState.Chase;
			}
			else if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
			{
				currentState = EnemyState.Patrol;
			}
		}
	}

	protected virtual void UpdateAnimation()
	{
		// Update animation parameters
		if (animator != null)
		{
			animator.SetInteger("State", (int)currentState);
			animator.SetBool("IsGrounded", isGrounded);
			animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
		}
	}

	protected virtual void Flip()
	{
		// Flip the enemy's direction
		isFacingRight = !isFacingRight;
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
	}

	public virtual void TakeDamage(int damage)
	{
		health -= damage;
		if (health > 0)
		{
			currentState = EnemyState.Hurt;
			// Trigger hurt animation
			if (animator != null)
				animator.SetTrigger("Hurt");
		}
	}

	protected IEnumerator AttackCooldown()
	{
		canAttack = false;
		yield return new WaitForSeconds(attackCooldown);
		canAttack = true;
	}

	// Check if player is within detection range
	protected bool IsPlayerInRange(float range)
	{
		return Vector2.Distance(transform.position, player.position) <= range;
	}

	// Check which direction the player is
	protected bool IsPlayerToRight()
	{
		return player.position.x > transform.position.x;
	}
}