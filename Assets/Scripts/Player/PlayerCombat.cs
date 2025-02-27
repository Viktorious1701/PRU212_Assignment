using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Properties")]
    [SerializeField] private float bareHandDamage = 5f;
    [SerializeField] private float swordDamage = 15f;
    [SerializeField] private float bowDamage = 10f;
    [SerializeField] private float spellDamage = 20f;

    [Header("Attack Cooldowns")]
    [SerializeField] private float bareHandCooldown = 0.3f;
    [SerializeField] private float swordCooldown = 0.5f;
    [SerializeField] private float bowCooldown = 1f;
    [SerializeField] private float spellCooldown = 1.5f;

    [Header("Attack Ranges")]
    [SerializeField] private float bareHandRange = 1f;
    [SerializeField] private float swordRange = 2f;
    [SerializeField] private float bowRange = 10f;
    [SerializeField] private float spellRange = 8f;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private GameObject firePoint;

    private float lastAttackTime;
    private WeaponType currentWeapon = WeaponType.BareHand;
    private Animator animator;
    private bool isFacingRight = true;

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
    }

    private void Update()
    {
        // Handle attack input
        if (Input.GetMouseButtonDown(0)) // Left click for attack
        {
            PerformAttack();
        }

        // Example of weapon switching (you can modify this based on your needs)
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentWeapon = WeaponType.BareHand;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentWeapon = WeaponType.Sword;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentWeapon = WeaponType.Bow;
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentWeapon = WeaponType.Spell;

        // This is just an example - you should integrate with your actual player movement code
        float moveX = Input.GetAxisRaw("Horizontal");
        if (moveX > 0 && !isFacingRight)
        {
            isFacingRight = !isFacingRight;
        }
        else if (moveX < 0 && isFacingRight)
        {
            isFacingRight = !isFacingRight;
        }    
    }

    private void PerformAttack()
    {
        float currentCooldown = GetCurrentCooldown();

        if (Time.time - lastAttackTime < currentCooldown)
            return;

        lastAttackTime = Time.time;

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

        // Trigger animation
        animator?.SetTrigger("PunchAttack");

        // Get the attack direction based on facing direction
        Vector3 attackDirection = isFacingRight ? transform.right : -transform.right;

        // Check for hits using raycast or overlap
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + attackDirection * bareHandRange, 0.5f);
        foreach (Collider2D hit in hits)
        {
            ApplyDamage(hit.gameObject, bareHandDamage);
        }
    }

    private void SwordAttack()
    {
        animator?.SetTrigger("SwordAttack");

        // Get the attack direction based on facing direction
        Vector3 attackDirection = isFacingRight ? transform.right : -transform.right;

        // Create a larger arc for sword attack
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + attackDirection * swordRange, 1f);
        foreach (Collider2D hit in hits)
        {
            ApplyDamage(hit.gameObject, swordDamage);
        }
    }
    private void BowAttack()
    {
        animator?.SetTrigger("BowAttack");

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
        spellRb.velocity = direction * projectileSpeed;

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
                Gizmos.DrawWireSphere(transform.position + direction * swordRange, 1f);
                break;
        }
    }
}