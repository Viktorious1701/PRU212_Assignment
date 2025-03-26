using UnityEngine;
using System.Collections;

public class HitEffectController : MonoBehaviour
{
    public static HitEffectController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnPunchEffect(Vector3 startPosition, Vector3 targetPosition, Color effectColor, bool isFacingRight)
    {
        var (mainSprite, glowSprite) = HitEffectGenerator.GenerateHorizontalPunchEffect(effectColor);
        StartCoroutine(AnimatePunchEffect(startPosition, targetPosition, mainSprite, glowSprite, isFacingRight));
    }

    public void SpawnSwordEffect(Vector3 startPosition, Vector3 targetPosition, Color effectColor, bool isFacingRight)
    {
        var (mainSprite, glowSprite) = HitEffectGenerator.GenerateSwordSlashEffect(effectColor);
        StartCoroutine(AnimateSwordEffect(startPosition, targetPosition, mainSprite, glowSprite, isFacingRight));
    }

    private IEnumerator AnimatePunchEffect(Vector3 startPosition, Vector3 targetPosition, Sprite mainSprite, Sprite glowSprite, bool isFacingRight)
    {
        targetPosition.y = startPosition.y;
        GameObject effectObject = new GameObject("PunchEffect");
        effectObject.transform.position = startPosition;

        GameObject glowObject = new GameObject("Glow");
        glowObject.transform.SetParent(effectObject.transform, false);
        glowObject.transform.localPosition = Vector3.zero;
        SpriteRenderer glowRenderer = glowObject.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = glowSprite;
        glowRenderer.sortingLayerName = "Effect";
        glowRenderer.sortingOrder = 0;

        SpriteRenderer mainRenderer = effectObject.AddComponent<SpriteRenderer>();
        mainRenderer.sprite = mainSprite;
        mainRenderer.sortingLayerName = "Effect";
        mainRenderer.sortingOrder = 1;

        float duration = 0.3f;
        float elapsedTime = 0f;
        Vector3 direction = (targetPosition - startPosition).normalized;
        float distance = Vector2.Distance(startPosition, targetPosition);

        if (!isFacingRight)
        {
            effectObject.transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            effectObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            Color mainColor = mainRenderer.color;
            mainColor.a = Mathf.Lerp(1f, 0f, t);
            mainRenderer.color = mainColor;

            Color glowColor = glowRenderer.color;
            glowColor.a = Mathf.Lerp(1f, 0f, t * 1.2f);
            glowRenderer.color = glowColor;

            float scale = Mathf.Lerp(1f, 1.5f, Mathf.Sin(t * Mathf.PI));
            effectObject.transform.localScale = new Vector3(isFacingRight ? scale : -scale, scale, 1f);

            yield return null;
        }

        Destroy(effectObject);
    }

    private IEnumerator AnimateSwordEffect(Vector3 startPosition, Vector3 targetPosition, Sprite mainSprite, Sprite glowSprite, bool isFacingRight)
    {
        targetPosition.y = startPosition.y;
        GameObject effectObject = new GameObject("SwordEffect");
        effectObject.transform.position = startPosition;

        // Glow layer
        GameObject glowObject = new GameObject("Glow");
        glowObject.transform.SetParent(effectObject.transform, false);
        glowObject.transform.localPosition = Vector3.zero;
        SpriteRenderer glowRenderer = glowObject.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = glowSprite;
        glowRenderer.sortingLayerName = "Effect";
        glowRenderer.sortingOrder = 0;

        // Main layer
        SpriteRenderer mainRenderer = effectObject.AddComponent<SpriteRenderer>();
        mainRenderer.sprite = mainSprite;
        mainRenderer.sortingLayerName = "Effect";
        mainRenderer.sortingOrder = 1;

        float duration = 0.2f;
        float elapsedTime = 0f;
        Vector3 direction = (targetPosition - startPosition).normalized;
        float distance = Vector2.Distance(startPosition, targetPosition);

        // Random initial rotation
        float randomStartRotation = Random.Range(-45f, 45f);
        effectObject.transform.rotation = Quaternion.Euler(0f, 0f, -randomStartRotation);

        if (!isFacingRight)
        {
            effectObject.transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Straight-line movement using Lerp for distance
            float moveDistance = Mathf.Lerp(0f, distance, t); // Lerp the distance, not position
            effectObject.transform.position = startPosition + direction * moveDistance;

            Color mainColor = mainRenderer.color;
            mainColor.a = Mathf.Lerp(1f, 0f, t);
            mainRenderer.color = mainColor;

            Color glowColor = glowRenderer.color;
            glowColor.a = Mathf.Lerp(1f, 0f, t * 1.2f);
            glowRenderer.color = glowColor;

            float scale = Mathf.Lerp(1f, 1.2f, Mathf.Sin(t * Mathf.PI));
            effectObject.transform.localScale = new Vector3(isFacingRight ? scale : -scale, scale, 1f);

            yield return null;
        }

        Destroy(effectObject);
    }
}