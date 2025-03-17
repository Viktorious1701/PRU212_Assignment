using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [SerializeField] private KeyType keyType = KeyType.Destroy;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;
    [SerializeField] private string playerTag = "Player"; // Add this to specify the player tag

    private bool isCollected = false;

    public enum KeyType
    {
        Destroy,
        Collect
    }

    private void Update()
    {
        // For key items that require button press to collect
        if (keyType == KeyType.Collect && !isCollected)
        {
            // Check if player is nearby and presses the interaction key
            Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, 1f, LayerMask.GetMask("Player"));
            if (playerCollider != null && Input.GetKeyDown(interactionKey))
            {
                Collect();
            }
        }
    }

    // Add this method to detect collisions with the player
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If this is a Destroy type key and the player touches it
        if (keyType == KeyType.Destroy && collision.CompareTag(playerTag) && !isCollected)
        {
            Collect();
        }
    }

    // Called when player destroys or collects the key
    public void Collect()
    {
        isCollected = true;
        Debug.Log("Key collected!");

        if (keyType == KeyType.Destroy)
        {
            Destroy(gameObject);
        }
        else
        {
            // Hide the object but don't destroy it
            GetComponent<SpriteRenderer>().enabled = false;
            // Disable colliders if present
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                col.enabled = false;
            }
        }
    }

    public bool IsCollected()
    {
        return isCollected;
    }
}