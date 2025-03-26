using UnityEngine;

public class KeyDebugger : MonoBehaviour
{
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private float gizmoRadius = 0.5f;
    [SerializeField] private KeyCode debugInteractionKey = KeyCode.E;
    
    private KeyItem keyItem;
    private LadderDoor targetDoor;
    
    void Start()
    {
        // Get the KeyItem component
        keyItem = GetComponent<KeyItem>();
        if (keyItem == null)
        {
            Debug.LogError("KeyDebugger requires a KeyItem component on the same GameObject!");
        }
        
        // Find all LadderDoor objects in the scene
        LadderDoor[] doors = FindObjectsOfType<LadderDoor>();
        
        // Log what doors were found
        Debug.Log($"Found {doors.Length} LadderDoor objects in the scene");
        foreach (var door in doors)
        {
            Debug.Log($"Door found: {door.gameObject.name}");
        }
        
        // Find the InventoryManager
        InventoryManager inventory = FindObjectOfType<InventoryManager>();
        if (inventory == null)
        {
            Debug.LogError("No InventoryManager found in the scene! Add it to your player!");
        }
        else
        {
            Debug.Log($"InventoryManager found on: {inventory.gameObject.name}");
        }
    }
    
    void Update()
    {
        // Debug: Press the key to manually attempt to collect
        if (Input.GetKeyDown(debugInteractionKey))
        {
            Debug.Log($"Debug: Manually attempting to collect key {gameObject.name}");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Check if the player has an InventoryManager
                InventoryManager inventory = player.GetComponent<InventoryManager>();
                if (inventory != null)
                {
                    Debug.Log($"Player has InventoryManager. Attempting to add key...");
                }
                else
                {
                    Debug.LogError("Player does not have an InventoryManager component!");
                }
            }
            else
            {
                Debug.LogError("No GameObject with 'Player' tag found!");
            }
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw a sphere to make the key more visible
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Key collision with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Key trigger with: {other.gameObject.name}, Tag: {other.gameObject.tag}");
    }
} 