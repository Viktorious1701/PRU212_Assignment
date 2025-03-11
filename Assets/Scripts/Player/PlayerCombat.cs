using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Properties")]
    [SerializeField] private float bareHandDamage = 5f;
    [SerializeField] private float swordDamage = 15f;
    [SerializeField] private float bowDamage = 10f;
    [SerializeField] private float spellDamage = 20f;

    [Header("Attack Cooldowns")]
    [SerializeField] private float bareHandCooldown = 0.3f;
    [SerializeField] private float swordCooldown = 0.2f;
    [SerializeField] private float bowCooldown = 1f;
    [SerializeField] private float spellCooldown = 1.5f;

    [Header("Attack Ranges")]
    [SerializeField] private float bareHandRange = 1f;
    [SerializeField] private float swordRange = 2f;
    [SerializeField] private float bowRange = 10f;
    [SerializeField] private float spellRange = 8f;

    [Header("Combo Settings")]
    [SerializeField] private float comboTimeWindow = 1.2f; // Time window to perform next combo attack
    [SerializeField] private float bareHandComboMultiplier = 1.2f; // Damage multiplier for each combo step
    [SerializeField] private float swordComboMultiplier = 1.3f; // Damage multiplier for each combo step
    private bool isComboWindowOpen = false;
    [SerializeField] private GameObject bareHandHitEffectPrefab;
    [SerializeField] private GameObject swordHitEffectPrefab;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private GameObject firePoint;

    private float lastAttackTime;
    private WeaponType currentWeapon = WeaponType.BareHand;
    private Animator animator;
    private bool isFacingRight = true;
    private PlayerMovement playerMovement;

    // Combo variables
    private int currentComboCount = 0;
    private float comboTimer = 0f;
    private bool isInCombo = false;
    private Coroutine resetComboCoroutine;
    private bool hasDealtDamage = false;

    private enum WeaponType
    {
        BareHand,
        Sword,
        Bow,
        Spell
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        // Handle attack input
        if (Input.GetMouseButtonDown(0)) // Left click for attack
        {
            PerformAttack();
        }

        // Example of weapon switching (you can modify this based on your needs)
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(WeaponType.BareHand);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(WeaponType.Sword);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(WeaponType.Bow);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchWeapon(WeaponType.Spell);

        isFacingRight = playerMovement.IsFacingRight();

        // Update combo timer
        if (isInCombo)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }

    private void SwitchWeapon(WeaponType newWeapon)
    {
        if (currentWeapon != newWeapon)
        {
            ResetCombo();
            currentWeapon = newWeapon;

            // You might want to trigger animation or visual feedback
            // animator?.SetTrigger("WeaponSwitch");
        }
    }

    private void PerformAttack()
    {
        float currentCooldown = GetCurrentCooldown();

        // Allow attack if not in combo and off cooldown, or if combo just finished
        if (!isInCombo && Time.time - lastAttackTime < currentCooldown && currentComboCount != 0)
        {
            Debug.Log("Attack on cooldown");
            return;
        }

        // Prevent new attack until damage is dealt, unless starting a new combo
        if (isInCombo && isComboWindowOpen && !hasDealtDamage && currentComboCount > 0)
        {
            Debug.Log("Waiting for damage to be applied");
            return;
        }

        lastAttackTime = Time.time;
        hasDealtDamage = false; // Reset for the new attack

        if (!isInCombo || comboTimer <= 0)
        {
            currentComboCount = 0;
            isInCombo = true;
        }

        if (isComboWindowOpen || currentComboCount == 0)
        {
            currentComboCount = Mathf.Min(currentComboCount + 1, 3);
            Debug.Log($"Combo count: {currentComboCount}");
            comboTimer = comboTimeWindow;

            if (resetComboCoroutine != null)
            {
                StopCoroutine(resetComboCoroutine);
            }
            resetComboCoroutine = StartCoroutine(ResetComboAfterAnimation());

            switch (currentWeapon)
            {
                case WeaponType.BareHand:
                    BareHandAttack();
                    break;
                case WeaponType.Sword:
                    SwordAttack();
                    break;
                case WeaponType.Bow:
                    BowAttack();
                    break;
                case WeaponType.Spell:
                    SpellAttack();
                    break;
            }
        }
    }

    public void OnAttackPoint(int comboStep)
    {
        Debug.Log($"Attack point hit for combo step {comboStep}");
        hasDealtDamage = true; // Mark damage as dealt

        // Rest of your OnAttackPoint logic...
        Vector3 attackDirection = isFacingRight ? transform.right : -transform.right;
        float damage = 0f;
        float attackRadius = 0f;
        float attackRange = 0f;

        switch (currentWeapon)
        {
            case WeaponType.BareHand:
                damage = bareHandDamage * Mathf.Pow(bareHandComboMultiplier, comboStep - 1);
                attackRadius = comboStep == 3 ? 0.7f : 0.5f;
                attackRange = bareHandRange;
                break;
            case WeaponType.Sword:
                damage = swordDamage * Mathf.Pow(swordComboMultiplier, comboStep - 1);
                attackRadius = comboStep == 3 ? 1.5f : 1f;
                attackRange = swordRange;
                break;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + attackDirection * attackRange, attackRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if(hit.GetComponent<Health>() && !hit.GetComponent<Health>().IsInvincible())
            {
                ApplyDamage(hit.gameObject, damage);
            }
            Rigidbody2D hitRigidbody = hit.GetComponent<Rigidbody2D>();
            if (hitRigidbody != null && comboStep == 3)
            {
                float knockbackForce = 15f;
                hitRigidbody.AddForce(attackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
        SpawnHitEffect(transform.position + attackDirection * attackRange);
    }
    private IEnumerator ResetComboAfterAnimation()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length * 0.9f); // Wait for 90% of the animation

        if (comboTimer <= 0) // Only reset if no new attack was input
        {
            ResetCombo();
        }
    }

    private void ResetCombo()
    {
        isInCombo = false;
        currentComboCount = 0;
        comboTimer = 0;

        if (resetComboCoroutine != null)
        {
            StopCoroutine(resetComboCoroutine);
            resetComboCoroutine = null;
        }

        // Make sure time scale is reset when combo ends
        if (HitFeedbackManager.Instance != null)
        {
            HitFeedbackManager.Instance.ResetTimeScale();
        }
    }

    private void ResetTriggers()
    {
        animator.ResetTrigger("SwordAttack1");
        animator.ResetTrigger("SwordAttack2");
        animator.ResetTrigger("SwordAttack3");
        animator.ResetTrigger("PunchAttack1");
        animator.ResetTrigger("PunchAttack2");
        animator.ResetTrigger("PunchAttack3");
    }

    private float GetCurrentCooldown()
    {
        switch (currentWeapon)
        {
            case WeaponType.BareHand: return bareHandCooldown;
            case WeaponType.Sword: return swordCooldown;
            case WeaponType.Bow: return bowCooldown;
            case WeaponType.Spell: return spellCooldown;
            default: return 0f;
        }
    }

    private void BareHandAttack()
    {
        ResetTriggers();
        // Trigger appropriate combo animation
        switch (currentComboCount)
        {
            case 1:
                animator?.SetTrigger("PunchAttack1");
                break;
            case 2:
                animator?.SetTrigger("PunchAttack2");
                break;
            case 3:
                animator?.SetTrigger("PunchAttack3");
                break;
        }
    }

    private void SwordAttack()
    {
        ResetTriggers();
        // Trigger appropriate combo animation
        switch (currentComboCount)
        {
            case 1:
                animator?.SetTrigger("SwordAttack1");
                break;
            case 2:
                animator?.SetTrigger("SwordAttack2");
                break;
            case 3:
                animator?.SetTrigger("SwordAttack3");
                break;
        }
    }

    private void BowAttack()
    {
        if (playerMovement.IsGrounded())
        {
            animator?.SetTrigger("BowAttack");
        }
        else
        {
            animator?.SetTrigger("BowAir");
        }

        // Reset combo when using bow (bows don't typically have melee combos)
        ResetCombo();
    }

    public void SpawnArrow()
    {
        // Instantiate arrow
        GameObject arrow = Instantiate(arrowPrefab, firePoint.transform.position, firePoint.transform.rotation);
        arrow.transform.Rotate(0, 0, isFacingRight ? -90 : 90);
        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
        Vector2 direction = isFacingRight ? transform.right : -transform.right;


        // Add a script to the arrow to handle damage
        ProjectileController arrowController = arrow.GetComponent<ProjectileController>();
        arrowController.Initialize(bowDamage, bowRange, gameObject);
    }

    private void SpellAttack()
    {
        animator?.SetTrigger("SpellAttack");
        // Reset combo when using spell (spells don't typically have melee combos)
        ResetCombo();
    }

    public void SpawnSpell()
    {
        // Get the attack direction based on facing direction
        Vector2 direction = isFacingRight ? transform.right : -transform.right;

        // Instantiate spell projectile
        Vector3 spawnPosition = firePoint.transform.position;
        if(isFacingRight)
        {
            spawnPosition.y -= 1.35f;
        }
        GameObject spell = Instantiate(spellPrefab, spawnPosition, firePoint.transform.rotation);

        Rigidbody2D spellRb = spell.GetComponent<Rigidbody2D>();
        

        // Add a script to the spell to handle damage and special effects
        ProjectileController spellController = spell.GetComponent<ProjectileController>();
        spellController.Initialize(spellDamage, spellRange, gameObject,direction);
    }

    private void ApplyDamage(GameObject target, float damage)
    {
        if (target == null) return;

        // Get attack direction
        Vector3 attackDirection = isFacingRight ? transform.right : -transform.right;

        // Create damage info
        DamageInfo damageInfo = new DamageInfo(
            damage,
            gameObject,
            target.transform.position,
            attackDirection
        );

        // Apply damage through the damage system
        DamageSystem.ApplyDamage(target, damageInfo);

        // Trigger hit feedback effects
        if (HitFeedbackManager.Instance != null)
        {
            Debug.Log("Triggering hit feedback");
            HitFeedbackManager.Instance.TriggerHitFeedback(target.transform.position, damage);
        }
    }

    // Helper method to visualize attack ranges in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Determine direction for Gizmos
        Vector3 direction = isFacingRight ? transform.right : -transform.right;

        switch (currentWeapon)
        {
            case WeaponType.BareHand:
                Gizmos.DrawWireSphere(transform.position + direction * bareHandRange, 0.5f);
                break;
            case WeaponType.Sword:
                // Show a larger radius for the third combo attack
                float radius = currentComboCount == 3 ? 1.5f : 1f;
                Gizmos.DrawWireSphere(transform.position + direction * swordRange, radius);
                break;
        }
    }

    public void OnComboWindowOpen()
    {
        if (hasDealtDamage) // Only open window if damage has been applied
        {
            Debug.Log("Combo window opened");
            isComboWindowOpen = true;
        }
        else
        {
            Debug.Log("Combo window delayed until damage is dealt");
        }
    }

    public void OnComboWindowClose()
    {
        Debug.Log("Combo window closed");
        isComboWindowOpen = false;

        if (currentComboCount > 0 && (Time.time - lastAttackTime > 0.2f || currentComboCount == 3))
        {
            ResetCombo();
        }

    }

    //Spawn visual effects on hit
    private void SpawnHitEffect(Vector3 position)
    {
        // Check if we have a hit effect prefab for the current weapon
        GameObject effectPrefab = null;

        switch (currentWeapon)
        {
            case WeaponType.BareHand:
                if (bareHandHitEffectPrefab != null)
                    effectPrefab = bareHandHitEffectPrefab;
                break;

            case WeaponType.Sword:
                if (swordHitEffectPrefab != null)
                    effectPrefab = swordHitEffectPrefab;
                break;
        }

        // Instantiate the effect if we have one
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);

            // Optionally destroy after a short time
            Destroy(effect, 1f);
        }
    }   

    public bool IsAttacking()
    {
        return isInCombo && currentComboCount > 0;
    }
}