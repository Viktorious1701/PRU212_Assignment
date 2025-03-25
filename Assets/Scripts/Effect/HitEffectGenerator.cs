using UnityEngine;

public class HitEffectGenerator : MonoBehaviour
{
    // Generate a basic hit effect texture
    public static Texture2D GenerateHitEffectTexture(Color baseColor, int size = 512, bool isBlunt = true)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        // Fill the texture with a gradient/radial effect
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Calculate distance from center
                float centerX = size / 2f;
                float centerY = size / 2f;
                float distanceFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                float normalizedDistance = distanceFromCenter / (size / 2f);

                // Create radial gradient
                float alpha = Mathf.Clamp01(1f - normalizedDistance * 1.5f);

                // Modify effect based on weapon type
                if (isBlunt)
                {
                    // Blunt weapons (fists, hammers) - more dispersed, softer edges
                    alpha = Mathf.Pow(alpha, 0.7f);
                }
                else
                {
                    // Sharp weapons (swords) - more concentrated, sharper edges
                    alpha = Mathf.Pow(alpha, 1.5f);
                }

                // Create color with gradient
                Color pixelColor = baseColor;
                pixelColor.a = alpha;

                texture.SetPixel(x, y, pixelColor);
            }
        }

        texture.Apply();
        return texture;
    }
   

    // Overloaded method to create hit effect sprite
    public static Sprite CreateHitEffectSprite(Color baseColor, int size = 128, bool isBlunt = true)
    {
        Texture2D texture = GenerateHitEffectTexture(baseColor, size, isBlunt);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    // Method to generate multiple frames for a simple hit effect animation
    public static Sprite[] GenerateHitEffectAnimation(Color baseColor, int frameCount = 5, int size = 64, bool isBlunt = true)
    {
        Sprite[] animationFrames = new Sprite[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            // Create a texture with decreasing alpha and potentially changing size
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            float progress = (float)i / (frameCount - 1);
            float currentSize = size * (1f - progress * 0.5f);
            int currentSizeInt = Mathf.RoundToInt(currentSize);

            for (int y = 0; y < currentSizeInt; y++)
            {
                for (int x = 0; x < currentSizeInt; x++)
                {
                    float centerX = currentSizeInt / 2f;
                    float centerY = currentSizeInt / 2f;
                    float distanceFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    float normalizedDistance = distanceFromCenter / (currentSizeInt / 2f);

                    // Reduce alpha over time
                    float alpha = Mathf.Clamp01(1f - normalizedDistance * 1.5f) * (1f - progress);

                    Color pixelColor = baseColor;
                    pixelColor.a = alpha;

                    texture.SetPixel(x, y, pixelColor);
                }
            }

            texture.Apply();
            animationFrames[i] = Sprite.Create(texture, new Rect(0, 0, currentSizeInt, currentSizeInt), new Vector2(0.5f, 0.5f));
        }

        return animationFrames;
    }

    public static (Sprite mainSprite, Sprite glowSprite) GenerateHorizontalPunchEffect(Color baseColor, int width = 128, int height = 32)
    {
        // Main sprite
        Texture2D mainTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        mainTexture.filterMode = FilterMode.Bilinear;

        // Glow sprite (larger for outline effect)
        int glowWidth = width + 32; // Extra space for glow
        int glowHeight = height + 16;
        Texture2D glowTexture = new Texture2D(glowWidth, glowHeight, TextureFormat.RGBA32, false);
        glowTexture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < glowHeight; y++)
        {
            for (int x = 0; x < glowWidth; x++)
            {
                // Offset for glow texture
                float mainX = x - 8f; // Center the main effect within glow
                float mainY = y - 8f;

                // Main effect calculation
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    float normalizedX = (float)x / width;
                    float normalizedY = Mathf.Abs(y - height / 2f) / (height / 2f);
                    float alpha = Mathf.Clamp01(1f - normalizedX * 1.2f) * Mathf.Clamp01(1f - normalizedY * 2f);
                    alpha = Mathf.Pow(alpha, 0.8f);

                    Color pixelColor = baseColor;
                    pixelColor.a = alpha;
                    mainTexture.SetPixel(x, y, pixelColor);
                }

                // Glow effect with black outline
                float glowNormX = (float)mainX / width;
                float glowNormY = Mathf.Abs(mainY - height / 2f) / (height / 2f);
                float glowAlpha = Mathf.Clamp01(1f - glowNormX * 1.5f) * Mathf.Clamp01(1f - glowNormY * 3f);
                glowAlpha = Mathf.Pow(glowAlpha, 0.5f); // Softer fade for glow

                Color glowColor = baseColor;
                glowColor.a = glowAlpha;

                // Add black outline by checking distance from edge
                float edgeDistance = Mathf.Min(glowNormX, 1f - glowNormX, glowNormY, 1f - glowNormY);
                if (edgeDistance < 0.1f && glowAlpha > 0.1f) // Outline thickness
                {
                    glowTexture.SetPixel(x, y, Color.black);
                }
                else
                {
                    glowTexture.SetPixel(x, y, glowColor);
                }
            }
        }

        mainTexture.Apply();
        glowTexture.Apply();

        Sprite mainSprite = Sprite.Create(mainTexture, new Rect(0, 0, width, height), new Vector2(0f, 0.5f));
        Sprite glowSprite = Sprite.Create(glowTexture, new Rect(0, 0, glowWidth, glowHeight), new Vector2(0f, 0.5f));
        return (mainSprite, glowSprite);
    }

    // Sword effect with glow
    public static (Sprite mainSprite, Sprite glowSprite) GenerateSwordSlashEffect(Color baseColor, int width = 512, int height = 32)
    {
        // Main sprite
        Texture2D mainTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        mainTexture.filterMode = FilterMode.Bilinear;

        // Glow sprite
        int glowWidth = width + 128;
        int glowHeight = height + 16;
        Texture2D glowTexture = new Texture2D(glowWidth, glowHeight, TextureFormat.RGBA32, false);
        glowTexture.filterMode = FilterMode.Bilinear;
        // First pass: Calculate main and glow without outline
        for (int y = 0; y < glowHeight; y++)
        {
            for (int x = 0; x < glowWidth; x++)
            {
                // Offset for glow texture
                float mainX = x - 8f;
                float mainY = y - 8f;

                // Main effect
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    float normalizedX = (float)x / width;
                    float normalizedY = (float)y / height;
                    float curve = Mathf.Sin(normalizedX * Mathf.PI * 2f);
                    float distanceFromCurve = Mathf.Abs(normalizedY - 0.5f) - curve * 0.2f;
                    float alpha = Mathf.Clamp01(1f - distanceFromCurve * 10f) * (1f - normalizedX * 0.8f);
                    alpha = Mathf.Pow(alpha, 1.5f);

                    Color pixelColor = baseColor;
                    pixelColor.a = alpha;
                    mainTexture.SetPixel(x, y, pixelColor);
                }

                // Glow effect (no outline yet)
                float glowNormX = (float)mainX / width;
                float glowNormY = (float)mainY / height;
                float glowCurve = Mathf.Sin(glowNormX * Mathf.PI * 2f);
                float glowDistance = Mathf.Abs(glowNormY - 0.5f) - glowCurve * 0.25f;
                float glowAlpha = Mathf.Clamp01(1f - glowDistance * 8f) * (1f - glowNormX * 1.2f);
                glowAlpha = Mathf.Pow(glowAlpha, 0.6f);

                Color glowColor = baseColor;
                glowColor.a = glowAlpha;
                glowTexture.SetPixel(x, y, glowColor);
            }
        }

        // Second pass: Add black outline to glow texture
        Color[] glowPixels = glowTexture.GetPixels();
        for (int y = 0; y < glowHeight; y++)
        {
            for (int x = 0; x < glowWidth; x++)
            {
                int index = y * glowWidth + x;
                float currentAlpha = glowPixels[index].a;

                // Check neighboring pixels for outline
                if (currentAlpha > 0.1f)
                {
                    bool isEdge = false;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && nx < glowWidth && ny >= 0 && ny < glowHeight)
                            {
                                int neighborIndex = ny * glowWidth + nx;
                                if (glowPixels[neighborIndex].a < 0.5f)
                                {
                                    isEdge = true;
                                    break;
                                }
                            }
                            else
                            {
                                isEdge = true; // Edge of texture
                                break;
                            }
                        }
                        if (isEdge) break;
                    }

                    if (isEdge)
                    {
                        glowTexture.SetPixel(x, y, Color.black);
                    }
                }
            }
        }

        mainTexture.Apply();
        glowTexture.Apply();

        Sprite mainSprite = Sprite.Create(mainTexture, new Rect(0, 0, width, height), new Vector2(0f, 0.5f));
        Sprite glowSprite = Sprite.Create(glowTexture, new Rect(0, 0, glowWidth, glowHeight), new Vector2(0f, 0.5f));
        return (mainSprite, glowSprite);
    }
}