using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private float damage;
    private float range;
    private Vector3 startPosition;

    public void Initialize(float damage, float range)
    {
        this.damage = damage;
        this.range = range;
        startPosition = transform.position;
    }

    private void Update()
    {
        // Destroy projectile if it exceeds range
        if (Vector3.Distance(startPosition, transform.position) > range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Health healthComponent = other.GetComponent<Health>();
        if (healthComponent != null)
        {
            healthComponent.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}