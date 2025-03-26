using UnityEngine;
using System.Collections;

public class SlidingDoor : DoorBase
{
    [Header("Sliding Settings")]
    [SerializeField] private Transform doorModel;
    [SerializeField] private Vector3 slideDirection = new Vector3(0, 1, 0); // Default: slide upward
    [SerializeField] private float slideDistance = 3f;
    [SerializeField] private float slideSpeed = 2f;
    
    private Vector3 closedPosition;
    private Vector3 openPosition;
    
    protected override void InitializeDoor()
    {
        // Get door model if not assigned
        if (doorModel == null)
        {
            doorModel = transform;
        }
        
        // Store positions
        closedPosition = doorModel.position;
        
        // Calculate the open position
        openPosition = closedPosition + slideDirection.normalized * slideDistance;
    }
    
    protected override void OpenDoor()
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimateDoorSlide());
        }
    }
    
    private IEnumerator AnimateDoorSlide()
    {
        isAnimating = true;
        
        // Play open sound
        PlayOpenSound();
        
        // Animate the door sliding
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * slideSpeed;
            doorModel.position = Vector3.Lerp(closedPosition, openPosition, time);
            yield return null;
        }
        
        // Ensure door is fully open
        doorModel.position = openPosition;
        isOpen = true;
        isAnimating = false;
    }
} 