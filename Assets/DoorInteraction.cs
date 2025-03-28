using System.Collections;
using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    [Header("References")]
    public Dialogue dialogueSystem;  // Reference to your Dialogue script
    public Rigidbody2D doorRigidbody; // Reference to the door’s Rigidbody 2D

    [Header("Movement Settings")]
    public float moveDistance = 5f;   // How far the door moves up (in units)
    public float moveSpeed = 2f;      // Speed of the movement (units per second)

    private bool isPlayerNear = false;
    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the trigger area!");
            isPlayerNear = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player exited trigger. Position: " + other.transform.position);
            }
            isPlayerNear = false;
        }
    }

    void Update()
    {
        // Check if player is near, presses F, dialogue isn’t active, and hasn’t triggered yet
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F) && !dialogueSystem.isDialogueActive && !hasTriggered)
        {
            StartDialogue();
            hasTriggered = true; // Prevent triggering again
        }
    }

    void StartDialogue()
    {
        // Replace with your desired dialogue
        string[] dialogueText = { "The door rumbles...", "It will open soon." };
        dialogueSystem.Say(dialogueText);
        StartCoroutine(WaitForDialogueEnd());
    }

    IEnumerator WaitForDialogueEnd()
    {
        // Wait until dialogue is no longer active
        while (dialogueSystem.isDialogueActive)
        {
            yield return null;
        }
        // Move the door after dialogue ends
        StartCoroutine(MoveDoorUp());
    }

    IEnumerator MoveDoorUp()
    {
        Vector2 startPosition = doorRigidbody.position;
        Vector2 targetPosition = startPosition + Vector2.up * moveDistance;

        // Smoothly move the door to the target position
        while (Vector2.Distance(doorRigidbody.position, targetPosition) > 0.01f)
        {
            doorRigidbody.MovePosition(Vector2.MoveTowards(doorRigidbody.position, targetPosition, moveSpeed * Time.deltaTime));
            yield return null;
        }
        // Snap to exact position to avoid floating-point drift
        doorRigidbody.position = targetPosition;
    }
}