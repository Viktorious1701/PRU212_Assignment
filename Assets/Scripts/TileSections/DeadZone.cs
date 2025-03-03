using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Handle player death (customize this part)
            Debug.Log("Player fell into the dead zone!");
            // Option 1: Reset to a checkpoint
            other.transform.position = new Vector3(0, 0, 0); // Replace with your checkpoint position
            // Option 2: Reload the scene
            // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}