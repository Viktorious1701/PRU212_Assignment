using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    [SerializeField] private GameObject questIcon; // Reference to the quest icon GameObject

    private void Start()
    {
        // Ensure the quest icon is hidden at the start
        if (questIcon != null)
        {
            questIcon.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            // Show the quest icon
            if (questIcon != null)
            {
                questIcon.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Optional: Hide the quest icon when player leaves the trigger zone
        if (other.CompareTag("Player"))
        {
            if (questIcon != null)
            {
                questIcon.SetActive(false);
            }
        }
    }
}