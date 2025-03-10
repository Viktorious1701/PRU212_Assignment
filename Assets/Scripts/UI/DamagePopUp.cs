using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro damageText;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private AnimationCurve scaleCurve;

    private Vector3 initialScale;
    private float timeAlive;
    private Color initialColor;

    private void Awake()
    {
        initialScale = transform.localScale;

        // Store initial color
        if (damageText != null)
        {
            initialColor = damageText.color;
        }
    }

    public void Setup(float damageAmount, bool isCritical = false)
    {
        // Check for null reference
        if (damageText == null)
        {
            damageText = GetComponent<TextMeshPro>();

            if (damageText == null)
            {
                Debug.LogError("No TextMeshPro component found on damage popup!");
                return;
            }

            initialColor = damageText.color;
        }

        // Set text
        damageText.text = damageAmount.ToString("0");

        // Reset time alive
        timeAlive = 0f;

        // Reset color to full alpha
        damageText.color = initialColor;

        // Critical hit formatting
        if (isCritical)
        {
            damageText.color = Color.red;
            transform.localScale = initialScale * 1.5f;
        }
        else
        {
            transform.localScale = initialScale;
        }

    }

    private void Update()
    {
        // Move upward
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // Track lifetime
        timeAlive += Time.deltaTime;

        // Calculate progress (0 to 1)
        float progress = timeAlive / lifetime;

        // Scale animation based on curve
        if (scaleCurve != null)
        {
            transform.localScale = initialScale * scaleCurve.Evaluate(progress);
        }

        // Handle alpha/fade
        if (damageText != null)
        {
            // Start fading only after half the lifetime
            if (progress > 0.5f)
            {
                // Calculate fade (1 to 0) for the second half of lifetime
                float fadeProgress = (progress - 0.5f) * 2f; // Rescale from 0-0.5 to 0-1
                Color textColor = damageText.color;
                textColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
                damageText.color = textColor;
            }
        }

        // Make text face camera
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }

        //// Debug info
        //if (progress > 0.9f && progress < 0.91f)
        //{
        //    Debug.Log($"Damage popup about to be destroyed, current alpha: {damageText.color.a}");
        //}

        // Destroy when lifetime ends
        if (timeAlive >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}