using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableGround : MonoBehaviour
{
    [Header("Breakable Tiles Settings")]
    [SerializeField] private Tilemap breakableTilemap;
    [SerializeField] private float breakForceThreshold = 0f; // Set to 0 for testing
    [SerializeField] private float breakDelay = 0.0f; // Changed to 0 for immediate breaking
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private ParticleSystem breakEffect;
    [SerializeField] private AudioClip breakSound;

    [Header("Advanced Settings")]
    [SerializeField] private float checkRadius = 0.3f; // Radius to check for tiles around player's feet
    [SerializeField] private bool breakAllTilesUnderPlayer = true; // Break all tiles under player's collider

    [Header("Debug")]
    [SerializeField] private bool debugMode = true; // Enable for troubleshooting

    private readonly HashSet<Vector3Int> tilesBeingProcessed = new HashSet<Vector3Int>();
    private AudioSource audioSource;
    private float lastCheckTime;
    private float checkInterval = 0.1f; // Check every 0.1 seconds while staying on tiles

    private void Start()
    {
        // Make sure the tilemap is assigned
        if (breakableTilemap == null)
        {
            breakableTilemap = GetComponent<Tilemap>();
            if (debugMode) Debug.Log("Using self as breakable tilemap");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && breakSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Debug info
        if (debugMode)
        {
            Debug.Log($"BreakableGround initialized. Player layer: {LayerMask.LayerToName(Mathf.RoundToInt(Mathf.Log(playerLayer.value, 2)))}");
            Debug.Log($"Tiles in tilemap: {GetTileCount()}");
        }
    }

    private int GetTileCount()
    {
        int count = 0;
        BoundsInt bounds = breakableTilemap.cellBounds;

        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (breakableTilemap.HasTile(pos))
                    count++;
            }
        }

        return count;
    }

    // Check for all tiles under the player's collider
    private void Update()
    {
        // Throttle checks to avoid performance issues
        if (Time.time - lastCheckTime < checkInterval)
            return;

        lastCheckTime = Time.time;

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null && player.IsGrounded())
        {
            if (breakAllTilesUnderPlayer)
            {
                BreakAllTilesUnderCollider(player.gameObject);
            }
            else
            {
                // Get player's feet position
                Vector3 playerPosition = player.transform.position;
                if (player.GetComponent<BoxCollider2D>() != null)
                {
                    BoxCollider2D collider = player.GetComponent<BoxCollider2D>();
                    playerPosition += new Vector3(0, -collider.bounds.extents.y, 0);
                }

                // Check a small radius around the feet
                CheckAndBreakTilesInRadius(playerPosition, checkRadius);
            }
        }
    }

    // Break tiles when player first lands on them
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            // Check if it's the player
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null && player.IsGrounded())
            {
                if (breakAllTilesUnderPlayer)
                {
                    BreakAllTilesUnderCollider(collision.gameObject);
                }
                else
                {
                    foreach (ContactPoint2D contact in collision.contacts)
                    {
                        // Make sure player is above the contact point
                        if (contact.normal.y > 0.5f)
                        {
                            CheckAndBreakTilesInRadius(contact.point, checkRadius);
                        }
                    }
                }
            }
        }
    }

    // Continue to break tiles while player is standing on them
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            // Only process periodically to avoid excessive checks
            if (Time.time - lastCheckTime < checkInterval)
                return;

            lastCheckTime = Time.time;

            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null && player.IsGrounded())
            {
                if (breakAllTilesUnderPlayer)
                {
                    BreakAllTilesUnderCollider(collision.gameObject);
                }
                else
                {
                    foreach (ContactPoint2D contact in collision.contacts)
                    {
                        if (contact.normal.y > 0.5f)
                        {
                            CheckAndBreakTilesInRadius(contact.point, checkRadius);
                        }
                    }
                }
            }
        }
    }

    // Break all tiles under the player's collider
    private void BreakAllTilesUnderCollider(GameObject playerObject)
    {
        Collider2D collider = playerObject.GetComponent<Collider2D>();
        if (collider == null) return;

        // Get the bounds of the collider
        Bounds bounds = collider.bounds;

        // Calculate the bottom of the collider with a small extension
        float bottom = bounds.min.y;
        float offset = 0.1f; // Small offset to ensure we catch the tiles just below the collider

        // Check all tiles in a rectangle under the player's collider
        for (float x = bounds.min.x; x <= bounds.max.x; x += 0.25f)
        {
            Vector3 checkPoint = new Vector3(x, bottom - offset, 0);
            Vector3Int cellPosition = breakableTilemap.WorldToCell(checkPoint);

            if (breakableTilemap.HasTile(cellPosition) && !tilesBeingProcessed.Contains(cellPosition))
            {
                if (debugMode) Debug.Log($"Breaking tile at {cellPosition} under player collider");
                StartCoroutine(BreakTile(cellPosition));
            }
        }
    }

    // Check tiles in a radius and break them
    private void CheckAndBreakTilesInRadius(Vector3 center, float radius)
    {
        for (float x = -radius; x <= radius; x += 0.25f)
        {
            for (float y = -radius; y <= radius; y += 0.25f)
            {
                if (x * x + y * y <= radius * radius) // Check if point is within circle
                {
                    Vector3 checkPoint = center + new Vector3(x, y, 0);
                    Vector3Int cellPosition = breakableTilemap.WorldToCell(checkPoint);

                    if (breakableTilemap.HasTile(cellPosition) && !tilesBeingProcessed.Contains(cellPosition))
                    {
                        if (debugMode) Debug.Log($"Breaking tile at {cellPosition} within radius");
                        StartCoroutine(BreakTile(cellPosition));
                    }
                }
            }
        }
    }

    private IEnumerator BreakTile(Vector3Int cellPosition)
    {
        // Mark this tile as being processed
        if (!tilesBeingProcessed.Add(cellPosition))
        {
            // Tile is already being processed
            yield break;
        }

        if (debugMode) Debug.Log("Breaking tile process started for " + cellPosition);

        // Immediate break with minimal delay or use configured delay
        yield return new WaitForSeconds(breakDelay);

        // Get the world position for effects
        Vector3 worldPos = breakableTilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0);

        // Double-check tile still exists (might have been broken by another process)
        if (!breakableTilemap.HasTile(cellPosition))
        {
            tilesBeingProcessed.Remove(cellPosition);
            yield break;
        }

        // Store the original tile for reference (for debugging)
        TileBase originalTile = breakableTilemap.GetTile(cellPosition);

        if (debugMode) Debug.Log($"About to remove tile: {(originalTile != null ? originalTile.name : "null")}");

        // Remove the tile
        breakableTilemap.SetTile(cellPosition, null);

        // Play particle effect
        if (breakEffect != null)
        {
            ParticleSystem effect = Instantiate(breakEffect, worldPos, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        // Play sound effect
        if (audioSource != null && breakSound != null)
        {
            audioSource.PlayOneShot(breakSound);
        }

        // Remove from processing dictionary after a short delay
        yield return new WaitForSeconds(0.1f);
        tilesBeingProcessed.Remove(cellPosition);

        if (debugMode) Debug.Log("Tile break complete for " + cellPosition);
    }

    // Make a test method that can be called from a button or key press
    public void TestBreakTileAtPlayerPosition()
    {
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            Vector3 playerPosition = player.transform.position;
            Vector3Int cellPosition = breakableTilemap.WorldToCell(playerPosition);

            if (breakableTilemap.HasTile(cellPosition))
            {
                Debug.Log("Test: Breaking tile under player");
                StartCoroutine(BreakTile(cellPosition));
            }
            else
            {
                Debug.Log("Test: No tile found under player");
            }
        }
    }

    // Add visual debugging
    private void OnDrawGizmos()
    {
        if (debugMode && breakableTilemap != null)
        {
            // Draw bounds of the tilemap
            BoundsInt bounds = breakableTilemap.cellBounds;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(breakableTilemap.transform.position + (Vector3)(bounds.center), (Vector3)(bounds.size));

            // Draw positions of all tiles
            Gizmos.color = Color.green;
            for (int x = bounds.min.x; x < bounds.max.x; x++)
            {
                for (int y = bounds.min.y; y < bounds.max.y; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    if (breakableTilemap.HasTile(pos))
                    {
                        Vector3 worldPos = breakableTilemap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0);
                        Gizmos.DrawWireCube(worldPos, Vector3.one * 0.8f);
                    }
                }
            }
        }
    }
}