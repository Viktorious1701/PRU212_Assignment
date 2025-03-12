using UnityEngine;

public class SawbladeTrigger : MonoBehaviour
{
    [SerializeField] private Sawblade[] sawblades;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered sawblade trigger zone!");

            // Activate all connected sawblades
            foreach (var sawblade in sawblades)
            {
                sawblade.BeginMovement();
            }
        }
    }
}