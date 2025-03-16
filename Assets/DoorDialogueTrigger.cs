using UnityEngine;

public class DoorDialogueTrigger : MonoBehaviour
{
    [SerializeField] private Dialogue dialogueSystem;
    [SerializeField] private DoorActivation doorActivation;

    private void Awake()
    {
        // Auto-find references if not set
        if (dialogueSystem == null)
        {
            dialogueSystem = FindObjectOfType<Dialogue>();
            if (dialogueSystem == null)
            {
                Debug.LogError("No Dialogue system found in scene! Please assign manually.");
            }
        }

        if (doorActivation == null)
        {
            doorActivation = GetComponent<DoorActivation>();
            if (doorActivation == null)
            {
                Debug.LogError("DoorActivation script not found on this object!");
            }
        }

        // Assign the dialogue system to the door
        if (doorActivation != null && dialogueSystem != null)
        {
            // Use reflection to set the private field - only if you can't modify DoorActivation
            var field = typeof(DoorActivation).GetField("dialogueSystem",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(doorActivation, dialogueSystem);
            }
            else
            {
                Debug.LogWarning("Could not find dialogueSystem field in DoorActivation");
            }
        }
    }
}