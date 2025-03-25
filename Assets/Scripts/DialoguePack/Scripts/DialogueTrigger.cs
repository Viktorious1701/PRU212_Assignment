using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogueSystem;
    private bool canInteract = false;
    [SerializeField] private string[] customMessages = new string[] 
    {
        "Take the wheelbarrow to the end of the stage!",
        "Press 'E' near the wheelbarrow to grab it.",
        "Good luck on your journey!"
    };
    [SerializeField] private string speakerName = "Billboard";
    [SerializeField] private float messageDuration = 3f;

    private void Awake()
    {
        if (dialogueSystem == null)
        {
            Debug.LogError("dialogueSystem not assigned in DialogueTrigger on " + gameObject.name + ". Please assign it in the Inspector.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            // Automatically show the message when player enters the trigger area
            if (dialogueSystem != null && !dialogueSystem.isDialogueActive)
            {
                dialogueSystem.Say(customMessages, speakerName, messageDuration);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            if (dialogueSystem != null)
                dialogueSystem.Clear();
        }
    }

    private void Update()
    {
        // Optional: Allow player to press E to show the message again while in range
        if (canInteract && dialogueSystem != null && !dialogueSystem.isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            dialogueSystem.Say(customMessages, speakerName, messageDuration);
        }
    }
}