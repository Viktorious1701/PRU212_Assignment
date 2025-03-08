using UnityEngine;

public class SawbladeTriggerZone : MonoBehaviour
{
    [SerializeField] private SawbladeMovement sawblade; // Drag your Sawblade here in the Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Make sure your player is tagged "Player"
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered Sawblade trigger zone!");
            // Tell the sawblade to start moving
            sawblade.BeginMovement();
        }
    }
}
