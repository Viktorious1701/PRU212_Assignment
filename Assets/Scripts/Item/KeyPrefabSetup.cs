using UnityEngine;

// Add this to a key prefab to ensure it has the right components
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class KeyPrefabSetup : MonoBehaviour
{
    [Header("Key Configuration")]
    [SerializeField] private string keyId = "1";
    [SerializeField] private bool useBoxCollider = true;
    [SerializeField] private bool useCircleCollider = false;
    
    void Awake()
    {
        // Make sure we have Rigidbody2D with the right settings
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 1f;
            rb.mass = 0.1f;
            rb.drag = 0.5f;
            rb.angularDrag = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        
        // Make sure we have a KeyItem component with the right settings
        KeyItem keyItem = GetComponent<KeyItem>();
        if (keyItem == null)
        {
            keyItem = gameObject.AddComponent<KeyItem>();
        }
        
        // Make sure we have the right collider
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        
        if (useBoxCollider && boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(0.7f, 0.7f);
        }
        else if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
        
        if (useCircleCollider && circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.isTrigger = true;
            circleCollider.radius = 0.4f;
        }
        else if (circleCollider != null)
        {
            circleCollider.isTrigger = true;
        }
        
        // Add debug helper
        if (GetComponent<KeyDebugHelper>() == null)
        {
            gameObject.AddComponent<KeyDebugHelper>();
        }
    }
} 