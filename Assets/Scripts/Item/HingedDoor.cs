using UnityEngine;
using System.Collections;

public class HingedDoor : DoorBase
{
    [Header("Hinge Settings")]
    [SerializeField] private Transform doorModel;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private Vector3 rotationAxis = new Vector3(0, 1, 0); // Default: rotate around Y axis
    
    private Quaternion closedRotation;
    private Quaternion openRotation;
    
    protected override void InitializeDoor()
    {
        // Get door model if not assigned
        if (doorModel == null)
        {
            doorModel = transform;
        }
        
        // Store rotations
        closedRotation = doorModel.rotation;
        
        // Calculate the open rotation by adding openAngle along rotationAxis
        openRotation = closedRotation * Quaternion.Euler(rotationAxis * openAngle);
    }
    
    protected override void OpenDoor()
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimateDoorOpen());
        }
    }
    
    private IEnumerator AnimateDoorOpen()
    {
        isAnimating = true;
        
        // Play open sound
        PlayOpenSound();
        
        // Animate the door opening
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * openSpeed;
            doorModel.rotation = Quaternion.Slerp(closedRotation, openRotation, time);
            yield return null;
        }
        
        // Ensure door is fully open
        doorModel.rotation = openRotation;
        isOpen = true;
        isAnimating = false;
    }
} 