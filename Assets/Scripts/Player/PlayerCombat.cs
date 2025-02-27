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

        // Only check cooldown if not in a combo
        if (!isInCombo && Time.time - lastAttackTime < currentCooldown)
        {
            Debug.Log("Attack on cooldown");
            return;
        }
            

       // If not in a combo or combo has timed out, start fresh
    if (!isInCombo || comboTimer <= 0)
    {
        ResetCombo(); // Ensure clean slate
        currentComboCount = 1; // Start at 1
        isInCombo = true;
        comboTimer = comboTimeWindow;
    }
    // If in combo and the combo window is open, proceed to next attack
    else if (isInCombo && isComboWindowOpen && currentComboCount < 3)
    {
        currentComboCount++;
        comboTimer = comboTimeWindow; // Reset timer for next attack
    }
    // If combo window isn’t open or we’re at max combo, don’t increment
    else
    {
        return; // Ignore input until the combo window opens or combo resets
    }

    lastAttackTime = Time.time;

    // Stop any existing reset coroutine
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

    private IEnumerator ResetComboAfterAnimation()
    {
        // Wait for animation to complete (approx)
        float animationTime = 0.6f; // You may want to adjust this based on your actual animation length
        yield return new WaitForSeconds(animationTime);

        // Don't reset combo if player attacked again during animation
        if (comboTimer <= 0)
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

        // Get the attack direction based on facing direction
        Vector3 attackDirection = isFacingRight ? transform.right : -transform.right;

        // Calculate damage based on combo
        float damage = bareHandDamage;
        if (currentComboCount > 1)
        {
            // Increase damage for each combo step
            damage *= Mathf.Pow(bareHandComboMultiplier, currentComboCount - 1);
        }

        // Check for hits using raycast or overlap
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + attackDirection * bareHandRange, 0.5f);
        foreach (Collider2D hit in hits)
        {
            ApplyDamage(hit.gameObject, damage);
        }
    }

    private void SwordAttack()
    {
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

        // Get the attack direction based on facing direction
        Vector3 attackDirection = isFacingRight ? transform.right : -transform.right;

        // Calculate damage based on combo
        float damage = swordDamage;
        if (currentComboCount > 1)
        {
            // Increase damage for each combo step
            damage *= Mathf.Pow(swordComboMultiplier, currentComboCount - 1);
        }

        // Create a larger arc for sword attack
        float attackRadius = currentComboCount == 3 ? 1.5f : 1f; // Make the last attack in combo have a wider arc
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + attackDirection * swordRange, attackRadius);
        foreach (Collider2D hit in hits)
        {
            ApplyDamage(hit.gameObject, damage);
        }
    }

    private void BowAttack()
    {
        animator?.SetTrigger("BowAttack");
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
        arrowRb.velocity = direction * projectileSpeed;

        // Add a script to the arrow to handle damage
        ProjectileController arrowController = arrow.AddComponent<ProjectileController>();
        arrowController.Initialize(bowDamage, bowRange);
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
        Vector3 direction = isFacingRight ? transform.right : -transform.right;

        // Instantiate spell projectile
        Vector3 spawnPosition = firePoint.transform.position;
        spawnPosition.y -= 0.75f;
        GameObject spell = Instantiate(spellPrefab, spawnPosition, firePoint.transform.rotation);

        Rigidbody2D spellRb = spell.GetComponent<Rigidbody2D>();
        spellRb.velocity = direction * projectileSpeed * 0.5f;

        // Add a script to the spell to handle damage and special effects
        ProjectileController spellController = spell.AddComponent<ProjectileController>();
        spellController.Initialize(spellDamage, spellRange);
    }

    private void ApplyDamage(GameObject target, float damage)
    {
        // Get the health component of the target
        Health healthComponent = target.GetComponent<Health>();
        if (healthComponent != null)
        {
            healthComponent.TakeDamage(damage);
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

    // Public method to check if player is currently in an attack animation
    // Useful for preventing movement during attacks if desired

    // Called by animation event when the attack reaches its impact point (the moment damage should be applied)
    public void OnAttackPoint(int comboStep)
    {

        Debug.Log($"Attack point hit for combo step {comboStep}");

        // Get the attack direction based on facing direction
        Vector3 attackDirection = isFacingRight ? transform.right : -transform.right;

        float damage = 0f;
        float attackRadius = 0f;
        float attackRange = 0f;

        // Apply appropriate damage and range based on current weapon and combo step
        switch (currentWeapon)
        {
            case WeaponType.BareHand:
                // Base damage with combo multiplier
                damage = bareHandDamage * Mathf.Pow(bareHandComboMultiplier, comboStep - 1);
                attackRadius = 0.5f;
                attackRange = bareHandRange;

                // Make the third combo hit have a slightly larger radius
                if (comboStep == 3) attackRadius = 0.7f;
                break;

            case WeaponType.Sword:
                // Base damage with combo multiplier
                damage = swordDamage * Mathf.Pow(swordComboMultiplier, comboStep - 1);
                attackRadius = 1f;
                attackRange = swordRange;

                // Make the third combo hit have a larger radius
                if (comboStep == 3) attackRadius = 1.5f;
                break;
        }

        // Apply the damage using a circle overlap
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + attackDirection * attackRange, attackRadius);
        foreach (Collider2D hit in hits)
        {
            // Skip self-collision
            if (hit.gameObject == gameObject) continue;

            ApplyDamage(hit.gameObject, damage);

            // Optional: Apply knockback effect to enemies
            Rigidbody2D hitRigidbody = hit.GetComponent<Rigidbody2D>();
            if (hitRigidbody != null && comboStep == 3)
            {
                // Apply stronger knockback on the final hit
                float knockbackForce = 5f;
                hitRigidbody.AddForce(attackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }

        // Optional: Spawn hit effect
        SpawnHitEffect(transform.position + attackDirection * attackRange);

    }

    // Call this method from your animation event when the player can input the next combo attack
    public void OnComboWindowOpen()
    {
        // Set a flag to indicate that player can input the next combo attack
        isComboWindowOpen = true;
    }

    // Call this method from your animation event when the combo window closes
    public void OnComboWindowClose()
    {
        // If the player didn't continue the combo, reset it
        if (isComboWindowOpen && currentComboCount < 3)
        {
            // Reset only if the player didn't input a new attack
            if (Time.time - lastAttackTime > 0.4f)
            {
                ResetCombo();
            }

        }

        if(isComboWindowOpen && currentComboCount == 3)
        {
            ResetCombo();
        }

       

        isComboWindowOpen = false;
    }

    //Spawn visual effects on hit
    private void SpawnHitEffect(Vector3 position)
    {
        // Check if we have a hit effect prefab for the current weapon
        GameObject effectPrefab = null;

        switch (currentWeapon)
        {
            case WeaponType.BareHand:
                // You would assign these in the inspector
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

