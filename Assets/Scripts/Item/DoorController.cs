using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private string requiredKeyId = "default_key";
    [SerializeField] private bool destroyKeyOnUse = true;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private Transform doorModel;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private bool isLocked = true;
    
    [Header("Visual and Audio")]
    [SerializeField] private GameObject lockedEffect;
    [SerializeField] private GameObject unlockEffect;
    [SerializeField] private AudioClip lockedSound;
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioClip openSound;
    
    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionPrompt;
    
    private bool isOpen = false;
    private bool isAnimating = false;
    private InventoryManager playerInventory;
    private Transform player;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Start()
    {
        // Find player and inventory
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerInventory = playerObj.GetComponent<InventoryManager>();
        }
        
        // Hide the interaction prompt initially
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Store initial rotation as closed state
        if (doorModel != null)
        {
            closedRotation = doorModel.rotation;
            openRotation = Quaternion.Euler(doorModel.eulerAngles + new Vector3(0, openAngle, 0));
        }
        else
        {
            doorModel = transform; // Use this GameObject if no specific door model is assigned
        }
    }
    
    private void Update()
    {
        // Check if player is nearby
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Show or hide interaction prompt based on distance
            if (distanceToPlayer <= interactionDistance && !isOpen && !isAnimating)
            {
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(true);
                }
                
                // Check for interaction key press (E by default)
                if (Input.GetKeyDown(KeyCode.E))
                {
                    TryOpenDoor();
                }
            }
            else if (interactionPrompt != null && interactionPrompt.activeSelf)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
    
    public void TryOpenDoor()
    {
        // If the door is already open or animating, do nothing
        if (isOpen || isAnimating) return;
        
        // If the door is locked, check for key
        if (isLocked)
        {
            if (playerInventory != null && playerInventory.HasKey(requiredKeyId))
            {
                // Unlock the door
                isLocked = false;
                
                // Remove key from inventory if configured to do so
                if (destroyKeyOnUse)
                {
                    playerInventory.UseKey(requiredKeyId);
                }
                
                // Play unlock effects
                PlayUnlockEffects();
                
                // Open the door after unlocking
                StartCoroutine(AnimateDoorOpen());
            }
            else
            {
                // Play locked effects (door is locked and player doesn't have the key)
                PlayLockedEffects();
            }
        }
        else
        {
            // Door is unlocked, open it directly
            StartCoroutine(AnimateDoorOpen());
        }
    }
    
    private IEnumerator AnimateDoorOpen()
    {
        isAnimating = true;
        
        // Play open sound
        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
        
        // Animate the door opening
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime * openSpeed;
            doorModel.rotation = Quaternion.Lerp(closedRotation, openRotation, time);
            yield return null;
        }
        
        doorModel.rotation = openRotation; // Ensure door is fully open
        isOpen = true;
        isAnimating = false;
    }
    
    private void PlayLockedEffects()
    {
        // Play locked sound
        if (lockedSound != null)
        {
            AudioSource.PlayClipAtPoint(lockedSound, transform.position);
        }
        
        // Show locked visual effect
        if (lockedEffect != null)
        {
            Instantiate(lockedEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log("Door is locked and requires key: " + requiredKeyId);
    }
    
    private void PlayUnlockEffects()
    {
        // Play unlock sound
        if (unlockSound != null)
        {
            AudioSource.PlayClipAtPoint(unlockSound, transform.position);
        }
        
        // Show unlock visual effect
        if (unlockEffect != null)
        {
            Instantiate(unlockEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log("Door unlocked with key: " + requiredKeyId);
    }
} 