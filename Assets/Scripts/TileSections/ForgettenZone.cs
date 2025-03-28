using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ForgottenZone : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float vignetteMaxIntensity = 0.7f;
    [SerializeField] private float vignetteSpeed = 0.5f;

    private float timeInZone = 0f;
    private bool playerInZone = false;
    private GameObject player;
    private Health playerHealth;
    private float damageTimer = 0f;
    private Volume volume;
    private Vignette vignette;

    void Start()
    {
        volume = FindObjectOfType<Volume>();
        if (volume != null)
        {
            if (volume.profile.TryGet(out vignette))
            {
                Debug.Log("Vignette found in Volume profile!");
            }
            else
            {
                Debug.LogError("Vignette effect not found in Volume profile! Add it manually.");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            player = collision.gameObject;
            playerHealth = player.GetComponent<Health>();
            playerInZone = true;
            timeInZone = 0f;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInZone = false;
            ResetVignette();
        }
    }

    void Update()
    {
        if (playerInZone && playerHealth != null)
        {
            timeInZone += Time.deltaTime;
            damageTimer += Time.deltaTime;

            if (damageTimer >= damageInterval && vignette.intensity.value >= 0.8f)
            {
                playerHealth.TakeDamage(damagePerSecond);
                damageTimer = 0f;
            }

            if (vignette != null)
            {
                float intensity = Mathf.Clamp01(timeInZone * vignetteSpeed);
                vignette.intensity.value = Mathf.Lerp(0f, vignetteMaxIntensity, intensity);
            }
        }
    }

    void ResetVignette()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0f;
        }
    }
}