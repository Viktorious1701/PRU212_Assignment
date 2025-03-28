using UnityEngine;

public class CrateSpawner : MonoBehaviour
{
    [SerializeField] private GameObject cratePrefab; // Drag the Large Crate prefab here in the inspector
    [SerializeField] private float spawnInterval = 5f; // Time between spawns
    [SerializeField] private Transform[] spawnPoints; // Array of possible spawn locations

    private void Start()
    {
        // Start the spawning coroutine
        StartCoroutine(SpawnCrates());
    }

    private System.Collections.IEnumerator SpawnCrates()
    {
        while (true) // Infinite loop to continuously spawn crates
        {
            // Wait for the specified interval
            yield return new WaitForSeconds(spawnInterval);

            // Choose a random spawn point
            if (spawnPoints.Length > 0)
            {
                Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

                // Instantiate the crate at the random spawn point's position
                Instantiate(cratePrefab, randomSpawnPoint.position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("No spawn points assigned to CrateSpawner!");
            }
        }
    }
}