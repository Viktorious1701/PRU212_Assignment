using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private GameObject damageFlashPanel;

    [Header("Settings")]
    [SerializeField] private Health playerHealth; // Reference to player health
    [SerializeField] private float flashDuration = 0.2f;

    private float flashTimer;

    private void Start()
    {
        if (playerHealth == null)
        {
            // Try to find player health if not set
            playerHealth = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Health>();
        }

        if (playerHealth != null)
        {
            // Subscribe to health changes
            UpdateHealthUI(playerHealth.GetCurrentHealth());

            // Use Unity Events from your Health script to update UI
            playerHealth.onHealthChanged.AddListener(UpdateHealthUI);
        }

        // Hide flash panel initially
        if (damageFlashPanel != null)
            damageFlashPanel.SetActive(false);
    }

    private void Update()
    {
        // Handle damage flash effect
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;

            if (flashTimer <= 0 && damageFlashPanel != null)
                damageFlashPanel.SetActive(false);
        }
    }

    public void UpdateHealthUI(float currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = playerHealth.GetMaxHealth();
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{playerHealth.GetMaxHealth()}";
        }

        // Flash the screen when taking damage
        PlayDamageFlash();
    }

    private void PlayDamageFlash()
    {
        if (damageFlashPanel != null)
        {
            damageFlashPanel.SetActive(true);
            flashTimer = flashDuration;
        }
    }
}