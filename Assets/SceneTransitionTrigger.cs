using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [Tooltip("Name of the scene to load when interacting")]
    public string sceneToLoad = "SCENE4_Castle";

    [Header("Interaction Settings")]
    [Tooltip("Key to press for interaction")]
    public KeyCode interactionKey = KeyCode.F;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionPrompt;

    private bool isPlayerInTrigger = false;

    private void Start()
    {
        // Ensure interaction prompt is hidden at start
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        // Check if player is in trigger zone and pressed interaction key
        if (isPlayerInTrigger && Input.GetKeyDown(interactionKey))
        {
            TransitionToScene();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;

            // Show interaction prompt if assigned
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the player is leaving the trigger zone
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;

            // Hide interaction prompt if assigned
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void TransitionToScene()
    {
        try
        {
            // Load the specified scene
            SceneManager.LoadScene(sceneToLoad);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading scene {sceneToLoad}: {e.Message}");
        }
    }
}