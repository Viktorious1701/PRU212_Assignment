using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogueSystem;

    void Start()
    {
        Debug.Log("Dialogue Triggered!");
        // Find the dialogue system if not assigned
        if (dialogueSystem == null)
            dialogueSystem = FindObjectOfType<Dialogue>();

        // Test the dialogue
        if (dialogueSystem != null)
        {
            Debug.Log("Dialogue system found!");
            dialogueSystem.Say("Hello! This is a test dialogue.", "Character");
        }
         
        else
            Debug.LogError("Dialogue system not found!");
    }
}