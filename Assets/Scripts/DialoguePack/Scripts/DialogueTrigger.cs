using System;
using System.Linq;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogueSystem;
    private bool canInteract = false;
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
            canInteract = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            dialogueSystem.Clear();
        }
    }
    private void Update()
    {
        if (canInteract && !dialogueSystem.isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            dialogueSystem.Say(new string[] { "Hello, traveler!", "Welcome to our village." }, "Village Girl", 2f);
        }
    }
}