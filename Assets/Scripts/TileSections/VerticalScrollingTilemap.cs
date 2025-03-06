using UnityEngine;
using UnityEngine.Tilemaps;

public class VerticalScrollingTilemap : MonoBehaviour
{
    public float scrollSpeed = 1.0f;
    public float resetThreshold = 10.0f;  // How far up objects go before respawning
    public float spawnOffset = 10.0f;     // How far down to spawn new objects

    private Tilemap tilemap;
    private Vector3Int[] tilePositions;
    private TileBase[] tiles;
    private Vector3 startPosition;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        startPosition = tilemap.transform.position;

        // Store the initial tilemap layout
        StoreTilemapData();
    }

    void Update()
    {
        // Move the tilemap upward
        tilemap.transform.position += Vector3.up * scrollSpeed * Time.deltaTime;

        // Check if we've moved beyond the threshold
        if (tilemap.transform.position.y >= startPosition.y + resetThreshold)
        {
            // Reset position and respawn tiles
            ResetAndRespawn();
        }
    }

    void StoreTilemapData()
    {
        // Get bounds of the tilemap
        BoundsInt bounds = tilemap.cellBounds;

        // Create arrays to store the tile data
        int tileCount = 0;
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(pos))
                {
                    tileCount++;
                }
            }
        }

        tilePositions = new Vector3Int[tileCount];
        tiles = new TileBase[tileCount];

        // Fill the arrays with the tile data
        int index = 0;
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(pos))
                {
                    tilePositions[index] = pos;
                    tiles[index] = tilemap.GetTile(pos);
                    index++;
                }
            }
        }
    }

    void ResetAndRespawn()
    {
        // Reset position to below the screen
        Vector3 newPosition = startPosition;
        newPosition.y -= spawnOffset;
        tilemap.transform.position = newPosition;

        // Option: you could also clear and respawn the tiles in a different pattern here
        // tilemap.ClearAllTiles();
        // Repopulate with the original tiles or a new pattern
    }
}