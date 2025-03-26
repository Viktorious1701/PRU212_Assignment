using System.Collections;
using UnityEngine;

public class FireballExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private float damageAmount = 50f;

    [Header("Fragment Settings")]
    [SerializeField] private GameObject[] fragmentPrefabs;
    [SerializeField] private int minFragments = 5;
    [SerializeField] private int maxFragments = 10;
    [SerializeField] private float fragmentSpawnRadius = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;

    private ProjectileController projectileController;

    private void Awake()
    {
        projectileController = GetComponent<ProjectileController>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Explode()
    {
        if(audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        Vector3 explosionOrigin = transform.position;

        // Create explosion effect
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, explosionOrigin, Quaternion.identity);
        }

        // Damage and knockback nearby objects
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(explosionOrigin, explosionRadius);
        foreach (Collider2D hitCollider in hitColliders)
        {
            // Skip if the object is on the ally layer of the projectile
            if (projectileController != null &&
                ((1 << hitCollider.gameObject.layer) & projectileController.allyLayer.value) != 0)
            {
                continue;
            }

            // Apply damage
            if (hitCollider.GetComponent<Health>() != null)
            {
                // Create damage info
                DamageInfo damageInfo = new DamageInfo(
                    damageAmount,
                    projectileController.owner,
                    hitCollider.transform.position,
                    (hitCollider.transform.position - explosionOrigin).normalized
                );
                DamageSystem.ApplyDamage(hitCollider.gameObject, damageInfo);
            }

            // Apply knockback force to rigidbodies
            Rigidbody2D rb = hitCollider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 explosionDirection = (rb.transform.position - explosionOrigin).normalized;
                rb.AddForce(explosionDirection * explosionForce, ForceMode2D.Impulse);
            }
        }

        // Spawn chaos fragments
        StartCoroutine(SpawnFragments(explosionOrigin));
    }

    private IEnumerator SpawnFragments(Vector3 explosionOrigin)
    {
        if (fragmentPrefabs == null || fragmentPrefabs.Length == 0) yield return null;

        int fragmentCount = Random.Range(minFragments, maxFragments + 1);
        yield return new WaitForSeconds(0.25f);
        for (int i = 0; i < fragmentCount; i++)
        {
            // Select a random fragment prefab
            GameObject fragmentPrefab = fragmentPrefabs[Random.Range(0, fragmentPrefabs.Length)];

            // Calculate spawn position with some randomness
            Vector3 spawnPosition = explosionOrigin + Random.insideUnitSphere * fragmentSpawnRadius;
            spawnPosition.z = 0; // Ensure 2D positioning

            // Instantiate fragment
            GameObject fragment = Instantiate(fragmentPrefab, spawnPosition, Quaternion.identity);

            // Get rigidbody and apply random force
            Rigidbody2D rb = fragment.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 explosionDirection = (fragment.transform.position - explosionOrigin).normalized;

                // Add some randomness to explosion direction
                explosionDirection += Random.insideUnitCircle * 0.5f;

                // Apply explosive force
                rb.AddForce(explosionDirection * Random.Range(explosionForce * 0.8f, explosionForce * 1.2f), ForceMode2D.Impulse);

                // Add some random rotation
                rb.AddTorque(Random.Range(-10f, 10f), ForceMode2D.Impulse);
            }
            Destroy(fragment, 1f);
        }
    }

    // Optional: Visualization of explosion radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}