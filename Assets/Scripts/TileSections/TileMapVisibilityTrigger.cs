using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapVisibilityTrigger : MonoBehaviour
{
    [Header("Visibility Settings")]
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private float obscuredRadius = 3f;
    [SerializeField] private Color obscuredColor = new Color(0, 0, 0, 0.8f);

    private GameObject player;
    private bool isPlayerInside = false;
    private Color originalColor;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        // Store the original tilemap color
        if (targetTilemap != null)
        {
            originalColor = targetTilemap.color;
        }
    }

    void Update()
    {
        // Only update obscuring effect when player is inside the area
        if (isPlayerInside)
        {
            UpdateObscuredVisibility();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            // Restore full visibility of the tilemap
            if (targetTilemap != null)
            {
                targetTilemap.color = originalColor;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            ResetToFullVisibility();
        }
    }

    void UpdateObscuredVisibility()
    {
        if (targetTilemap != null)
        {
            targetTilemap.color = obscuredColor;
        }
    }

    void ResetToFullVisibility()
    {
        // Completely restore the tilemap to full visibility
        if (targetTilemap != null)
        {
            targetTilemap.color = originalColor;

            // Ensure all tiles are fully opaque
            foreach (Vector3Int pos in targetTilemap.cellBounds.allPositionsWithin)
            {
                Color tileColor = targetTilemap.GetColor(pos);
                targetTilemap.SetColor(pos, new Color(tileColor.r, tileColor.g, tileColor.b, 1f));
            }
        }
    }
}