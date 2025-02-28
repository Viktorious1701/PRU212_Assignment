using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogueSystem;

    private void Awake()
    {
        // Find the dialogue system if not assigned in the Inspector
        if (dialogueSystem == null)
            dialogueSystem = FindObjectOfType<Dialogue>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Start the dialogue when the player enters the trigger area
            dialogueSystem.Say(new string[] { "Hello, traveler!", "Welcome to our village." }, "Village Girl", 4f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Clear the dialogue when the player leaves the area
            dialogueSystem.Clear();
        }
    }
}