using UnityEngine;

public class DestructibleBoxPiece : MonoBehaviour
{
    [SerializeField] private float destroyAfterSeconds = 2f;
    [SerializeField] private bool fadeOut = true;
    [SerializeField] private float fadeSpeed = 1f;

    private SpriteRenderer spriteRenderer;
    private float destroyTimer;
    private bool isFading = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        destroyTimer = destroyAfterSeconds;
    }

    private void Update()
    {
        destroyTimer -= Time.deltaTime;

        // Start fading when half the time has passed
        if (fadeOut && destroyTimer < destroyAfterSeconds / 2 && !isFading)
        {
            isFading = true;
        }

        // Fade out the sprite
        if (isFading && spriteRenderer != null)
        {
            Color currentColor = spriteRenderer.color;
            currentColor.a -= fadeSpeed * Time.deltaTime;
            spriteRenderer.color = currentColor;
        }

        // Destroy the piece
        if (destroyTimer <= 0)
        {
            Destroy(gameObject);
        }
    }
}