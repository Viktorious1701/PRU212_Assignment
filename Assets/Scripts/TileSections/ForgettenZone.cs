using System.Collections;
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
    private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
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
            audioSource.PlayOneShot(audioClip);
        }
        
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInZone = false;
            ResetVignette();
            timeInZone = 0f;
            damageTimer = 0f;
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

            if(playerHealth.GetCurrentHealth() <= 0)
            {
                playerInZone = false;
                ResetVignette();
                audioSource.Stop();
            }

            if(audioSource != null && !audioSource.isPlaying)
            {
                audioSource.PlayOneShot(audioClip);
            }
        }
    }

    void ResetVignette()
    {
        if (vignette != null)
        {
            StartCoroutine(EaseOutVignette());
        }
    }

    private IEnumerator EaseOutVignette()
    {
        float t = 0f;
        float startIntensity = vignette.intensity.value;
        float endIntensity = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * (vignetteSpeed); 
            vignette.intensity.value = Mathf.Lerp(startIntensity, endIntensity, t);
            yield return null;
        }
    }
}