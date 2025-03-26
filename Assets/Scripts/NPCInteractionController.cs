using UnityEngine;
using System.Linq;

public class NPCInteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject barrel;
    [SerializeField] private GameObject wheelbarrow;
    [SerializeField] private Transform playerDetectionPoint;
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Settings")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool removeOnlyIfBarrelNotFallen = true;
    
    private bool hasTriggered = false;
    private BarrelFallDetector barrelFallDetector;
    
    private void Start()
    {
        // Get references if not assigned
        if (barrel != null && barrelFallDetector == null)
        {
            barrelFallDetector = barrel.GetComponent<BarrelFallDetector>();
        }
        
        if (playerDetectionPoint == null)
        {
            playerDetectionPoint = transform;
        }
    }
    
    private void Update()
    {
        // Skip if already triggered
        if (hasTriggered) return;
        
        // Check if player is nearby
        if (IsPlayerNearby())
        {
            bool shouldHideObjects = true;
            
            // If we should check barrel status
            if (removeOnlyIfBarrelNotFallen && barrelFallDetector != null)
            {
                shouldHideObjects = !HasBarrelFallen();
            }
            
            if (shouldHideObjects)
            {
                // Hide objects
                HideObjects();
                hasTriggered = true;
                
                Debug.Log("Objects hidden due to player approaching NPC");
            }
        }
    }
    
    private bool IsPlayerNearby()
    {
        // Use Physics2D circle cast to detect player
        Collider2D[] colliders = Physics2D.OverlapCircleAll(playerDetectionPoint.position, detectionRadius);
        
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag(playerTag))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private bool HasBarrelFallen()
    {
        // Use reflection to access the private field in BarrelFallDetector
        if (barrelFallDetector != null)
        {
            System.Reflection.FieldInfo field = typeof(BarrelFallDetector).GetField("hasFallen", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (field != null)
            {
                return (bool)field.GetValue(barrelFallDetector);
            }
        }
        
        // Default return if we can't determine
        return false;
    }
    
    private void HideObjects()
    {
        // Hide barrel
        if (barrel != null)
        {
            barrel.SetActive(false);
        }
        
        // Hide the specific wheelbarrow reference
        if (wheelbarrow != null)
        {
            wheelbarrow.SetActive(false);
        }
        
        // Find and hide ALL wheelbarrow objects in the scene, including clones
        GameObject[] allWheelbarrows = GameObject.FindObjectsOfType<GameObject>()
            .Where(go => go.name.Contains("Wheelbarrow") || go.name.Contains("wheelbarrow"))
            .ToArray();
        
        foreach (GameObject wb in allWheelbarrows)
        {
            Debug.Log($"Found and hiding wheelbarrow: {wb.name}");
            wb.SetActive(false);
        }
    }
    
    // Method to manually trigger the hiding 
    public void TriggerHideObjects()
    {
        HideObjects();
        hasTriggered = true;
    }
    
    private void OnDrawGizmos()
    {
        if (showDebugGizmos && playerDetectionPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerDetectionPoint.position, detectionRadius);
        }
    }
} 