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
    [SerializeField] private bool isOneTimeOnly = false;
    
    private bool hasTriggeredOnce = false;
    
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
            
            // Only show if it hasn't been triggered yet (for one-time triggers) or if it's repeatable
            if (!hasTriggeredOnce || !isOneTimeOnly)
            {
                // Automatically show the message when player enters the trigger area
                if (dialogueSystem != null && !dialogueSystem.isDialogueActive)
                {
                    dialogueSystem.Say(customMessages, speakerName, messageDuration);
                    hasTriggeredOnce = true;
                }
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
        // Only if it hasn't been triggered yet (for one-time triggers) or if it's repeatable
        if (canInteract && dialogueSystem != null && !dialogueSystem.isDialogueActive && 
            Input.GetKeyDown(KeyCode.F) && (!hasTriggeredOnce || !isOneTimeOnly))
        {
            dialogueSystem.Say(customMessages, speakerName, messageDuration);
            hasTriggeredOnce = true;
        }
    }
    
    // Public methods to force trigger or reset the dialogue trigger
    public void TriggerDialogue()
    {
        if (dialogueSystem != null && (!hasTriggeredOnce || !isOneTimeOnly))
        {
            dialogueSystem.Say(customMessages, speakerName, messageDuration);
            hasTriggeredOnce = true;
        }
    }
    
    public void ResetTrigger()
    {
        hasTriggeredOnce = false;
    }
}