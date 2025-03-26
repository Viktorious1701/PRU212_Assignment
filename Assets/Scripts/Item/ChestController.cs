using UnityEngine;

public class ChestController : MonoBehaviour
{
    [Header("Chest Settings")]
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool openOnStart = false;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 2f;
    
    [Header("Animation")]
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private string openAnimTrigger = "Open";
    
    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    
    private bool isOpen = false;
    private bool playerInRange = false;
    
    void Start()
    {
        // If no spawn point is set, use the chest's position
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        
        // If no animator assigned, try to get it
        if (chestAnimator == null)
        {
            chestAnimator = GetComponent<Animator>();
        }
        
        // Open chest if set to open on start
        if (openOnStart)
        {
            OpenChest();
        }
    }
    
    void Update()
    {
        // Check for player interaction
        if (!isOpen && playerInRange && Input.GetKeyDown(interactKey))
        {
            OpenChest();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
    
    public void OpenChest()
    {
        if (isOpen) return;
        
        isOpen = true;
        
        // Play animation if animator exists
        if (chestAnimator != null)
        {
            chestAnimator.SetTrigger(openAnimTrigger);
        }
        
        // Play sound if assigned
        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
        
        // Spawn key if prefab is assigned
        if (keyPrefab != null)
        {
            // Add a slight delay before spawning the key
            Invoke("SpawnKey", 0.5f);
        }
    }
    
    private void SpawnKey()
    {
        Vector3 spawnPos = spawnPoint.position + new Vector3(0, 0.5f, 0);
        GameObject key = Instantiate(keyPrefab, spawnPos, Quaternion.identity);
        
        // Add force to make the key pop out
        Rigidbody2D keyRb = key.GetComponent<Rigidbody2D>();
        if (keyRb != null)
        {
            // Apply random upward force
            float randomX = Random.Range(-1f, 1f);
            keyRb.AddForce(new Vector2(randomX, 5f), ForceMode2D.Impulse);
        }
    }
} 