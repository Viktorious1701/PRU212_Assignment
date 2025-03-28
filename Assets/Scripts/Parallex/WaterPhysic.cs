using UnityEngine;

public class WaterPhysic : MonoBehaviour
{
    [Header("Water Physics Settings")]
    [Tooltip("Drag coefficient for objects in water")]
    public float waterDragCoefficient = 0.5f;

    [Tooltip("Buoyancy force multiplier")]
    public float buoyancyForce = 10f;

    [Tooltip("Depth at which full buoyancy is applied")]
    public float fullBuoyancyDepth = 1f;



    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("Slow");
        // Check if the object has a Rigidbody2D
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if(rb == null)
            return;

        // Calculate depth of object in water
        float objectDepth = CalculateObjectDepth(other);

        // Apply water drag
        ApplyWaterDrag(rb, objectDepth);

        // Apply buoyancy force
        ApplyBuoyancy(rb, objectDepth);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Reset drag when object leaves water
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.drag = 0;
        }
    }

    private float CalculateObjectDepth(Collider2D collision)
    {
        // Get the bottom of the collider
        float objectBottomY = collision.GetComponent<Collider2D>().bounds.min.y;
        float waterTopY = transform.position.y + GetComponent<CompositeCollider2D>().bounds.extents.y;

        // Calculate how deep the object is in the water
        float depth = Mathf.Clamp(waterTopY - objectBottomY, 0, fullBuoyancyDepth);
        return depth / fullBuoyancyDepth;
    }

    private void ApplyWaterDrag(Rigidbody2D rb, float depth)
    {
        // Apply increased drag based on depth in water
        float dragMultiplier = Mathf.Lerp(1f, waterDragCoefficient, depth);
        rb.drag = dragMultiplier;
    }

    private void ApplyBuoyancy(Rigidbody2D rb, float depth)
    {
        // Calculate buoyancy force based on depth
        Vector2 buoyancyForceVector = Vector2.up * buoyancyForce * depth;
        rb.AddForce(buoyancyForceVector);
    }
}