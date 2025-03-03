using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogueSystem;
    private bool canInteract = false;

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
        if (canInteract && dialogueSystem != null && !dialogueSystem.isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            dialogueSystem.Say(new string[] { "Hello, traveler!", "Welcome to our village." }, "Village Girl", 2f);
        }
    }
}