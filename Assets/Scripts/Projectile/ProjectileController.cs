using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private float damage;
    private float range;
    private float distanceTravelled = 0f;
    private Vector3 lastPosition;
    private GameObject owner;
    public float speed = 10f; // Adjust in Inspector

    public void Initialize(float damageAmount, float maxRange, GameObject source)
    {
        damage = damageAmount;
        range = maxRange;
        lastPosition = transform.position;
        owner = source;

    }

    private void Update()
    {
        //// Move projectile via transform (works with Kinematic or no Rigidbody)
        //transform.position += transform.right * speed * Time.deltaTime;

        //// Calculate distance traveled
        //distanceTravelled += Vector3.Distance(transform.position, lastPosition);
        //lastPosition = transform.position;

        // Destroy if exceeded range
        if (distanceTravelled >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Skip if hitting the owner
        if (owner != null && collider.gameObject == owner)
            return;

        // Apply damage
        DamageInfo damageInfo = new DamageInfo(
            damage,
            owner,
            transform.position,
            transform.forward
        );

        DamageSystem.ApplyDamage(collider.gameObject, damageInfo);

        // Destroy projectile on hit
        Destroy(gameObject);
    }
}