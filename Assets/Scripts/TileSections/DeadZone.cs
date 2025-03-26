using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeadZone : MonoBehaviour
{
    [SerializeField] private float respawnDelay = 0f; // Time delay before respawning
    [SerializeField] private Vector3 respawnPosition = Vector3.zero; // Default respawn position

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Handle player death
            Debug.Log("Player fell into the dead zone!");
            StartCoroutine(RespawnPlayer(other.gameObject));
        }
    }

    private IEnumerator RespawnPlayer(GameObject player)
    {
        // Get the Health component
        Health playerHealth = player.GetComponent<Health>();

        // Disable player movement and controls here if needed
        // Example: player.GetComponent<PlayerController>().enabled = false;

        // Wait for the specified delay
        yield return new WaitForSeconds(respawnDelay);

        // Reset player health to maximum
        if (playerHealth != null)
        {
            // Reset to max health (assuming 100 is max)
            float healAmount = playerHealth.GetMaxHealth() - playerHealth.GetCurrentHealth();
            playerHealth.TakeDamage(-healAmount); // Negative damage to heal
        }

        // Reposition player to spawn point
        player.transform.position = respawnPosition;

        // Re-enable player movement and controls if you disabled them
        // Example: player.GetComponent<PlayerController>().enabled = true;
    }
}