using UnityEngine;
using System.Collections;

public class LadderDoor : DoorBase
{
    [Header("Ladder Settings")]
    [SerializeField] private GameObject doorObject;
    [SerializeField] private float disappearSpeed = 2f;
    [SerializeField] private bool fadeOut = false;
    
    [Header("Debug")]
    [SerializeField] private KeyCode forceOpenKey = KeyCode.O;
    
    private SpriteRenderer[] doorRenderers;
    
    protected override void Update()
    {
        base.Update();
        
        // Debug key to force unlock door
        if (Input.GetKeyDown(forceOpenKey))
        {
            Debug.Log("DEBUG: Force opening door!");
            isLocked = false;
            OpenDoor();
        }
    }
    
    protected override void InitializeDoor()
    {
        // Make sure we have a door object
        if (doorObject == null)
        {
            Debug.LogError("No door object assigned to LadderDoor!");
            return;
        }
        
        // Get all renderers on the door
        doorRenderers = doorObject.GetComponentsInChildren<SpriteRenderer>();
        
        // Initially show the door (opposite of previous behavior)
        doorObject.SetActive(true);
        
        // Ensure door is fully visible
        foreach (SpriteRenderer renderer in doorRenderers)
        {
            Color color = renderer.color;
            color.a = 1f;
            renderer.color = color;
        }
        
        // Debug logging
        if (showDebugLogs)
        {
            Debug.Log($"LadderDoor initialized. Door object: {doorObject.name}");
            Debug.Log($"Found {doorRenderers.Length} sprite renderers on door");
            Debug.Log($"Fade Out is set to: {fadeOut}");
        }
    }
    
    protected override void OpenDoor()
    {
        if (!isAnimating)
        {
            if (fadeOut)
            {
                StartCoroutine(HideDoor());
            }
            else
            {
                // Simply hide the door without animation
                if (showDebugLogs)
                {
                    Debug.Log("Instantly hiding door (no fade)");
                }
                
                // Play open sound
                PlayOpenSound();
                
                // Hide door immediately
                doorObject.SetActive(false);
                
                isOpen = true;
            }
        }
    }
    
    private IEnumerator HideDoor()
    {
        isAnimating = true;
        
        // Play open sound
        PlayOpenSound();
        
        if (fadeOut && doorRenderers.Length > 0)
        {
            // Fade out the door renderers
            float time = 1;
            while (time > 0)
            {
                time -= Time.deltaTime * disappearSpeed;
                
                // Set alpha on all renderers
                foreach (SpriteRenderer renderer in doorRenderers)
                {
                    Color color = renderer.color;
                    color.a = time;
                    renderer.color = color;
                }
                
                yield return null;
            }
            
            // Ensure zero opacity
            foreach (SpriteRenderer renderer in doorRenderers)
            {
                Color color = renderer.color;
                color.a = 0f;
                renderer.color = color;
            }
        }
        
        // Finally disable the door object
        doorObject.SetActive(false);
        
        isOpen = true;
        isAnimating = false;
    }
} 