using UnityEngine;
using System.Collections;
using Cinemachine;

public class HitFeedbackManager : MonoBehaviour
{
    [Header("Screen Shake")]
    [SerializeField] private bool enableScreenShake = true;

    [Header("Hit Stop")]
    [SerializeField] private bool enableHitStop = true;
    [SerializeField] private float hitStopDuration = 0.05f;
    [SerializeField] private float hitStopTimeScale = 0.1f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject impactFlashPrefab;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Audio")]
    [SerializeField] private AudioClip[] lightHitSounds;
    [SerializeField] private AudioClip[] mediumHitSounds;
    [SerializeField] private AudioClip[] heavyHitSounds;
    [SerializeField] private AudioSource audioSource;

    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    private Coroutine activeHitStopCoroutine;

    private static HitFeedbackManager _instance;
    public static HitFeedbackManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        mainCamera = Camera.main;
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }



    private IEnumerator DoHitStop(HitIntensity intensity)
    {
        // Cancel any active hit stop before starting a new one
        if (activeHitStopCoroutine != null)
        {
            StopCoroutine(activeHitStopCoroutine);
            // Immediately restore normal time
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = 0.02f;
        }

        float multiplier = GetIntensityMultiplier(intensity);

        // Set new timescale and adjust fixedDeltaTime
        Time.timeScale = hitStopTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Wait in real time, not game time
        yield return new WaitForSecondsRealtime(hitStopDuration * multiplier);

        // Restore normal time
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;

        activeHitStopCoroutine = null;
    }

    public void TriggerHitFeedback(Vector3 hitPosition, float hitStrength)
    {
        // Determine intensity based on hit strength
        HitIntensity intensity = HitIntensity.Light;

        if (hitStrength > 20f)
            intensity = HitIntensity.Heavy;
        else if (hitStrength > 10f)
            intensity = HitIntensity.Medium;

        // Apply screen shake if enabled
        if (enableScreenShake)
            StartCoroutine(ShakeCamera(intensity));

        // Apply hit stop if enabled
        if (enableHitStop)
            activeHitStopCoroutine = StartCoroutine(DoHitStop(intensity));

        // Play sound and spawn visual effects
        PlayHitSound(intensity);
        SpawnImpactFlash(hitPosition, intensity);
    }

    private IEnumerator ShakeCamera(HitIntensity intensity)
    {
        if (mainCamera == null) yield break;

        GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }

   

    public void ResetTimeScale()
    {
        // Reset to normal time scale (1.0f)
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f; // Default fixed delta time in Unity
    }

    private void PlayHitSound(HitIntensity intensity)
    {
        AudioClip[] clips = lightHitSounds;

        switch (intensity)
        {
            case HitIntensity.Medium:
                clips = mediumHitSounds;
                break;
            case HitIntensity.Heavy:
                clips = heavyHitSounds;
                break;
        }

        if (clips.Length > 0)
        {
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (clip != null)
            {
                float volumeMultiplier = GetIntensityMultiplier(intensity);
                audioSource.PlayOneShot(clip, volumeMultiplier);
            }
        }
    }

    private void SpawnImpactFlash(Vector3 position, HitIntensity intensity)
    {
        if (impactFlashPrefab != null)
        {
            GameObject flash = Instantiate(impactFlashPrefab, position, Quaternion.identity);

            // Scale the flash based on intensity
            float scaleMultiplier = GetIntensityMultiplier(intensity);
            flash.transform.localScale *= scaleMultiplier;

            // Destroy the flash after a duration
            Destroy(flash, flashDuration);
        }
    }

    private float GetIntensityMultiplier(HitIntensity intensity)
    {
        switch (intensity)
        {
            case HitIntensity.Light:
                return 0.7f;
            case HitIntensity.Medium:
                return 1.0f;
            case HitIntensity.Heavy:
                return 1.5f;
            default:
                return 1.0f;
        }
    }

    public enum HitIntensity
    {
        Light,
        Medium,
        Heavy
    }
}