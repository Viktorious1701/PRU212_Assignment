using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private GameObject damageFlashPanel;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI arrowCountText;
    [SerializeField] private Image arrowIcon;

    [Header("Settings")]
    [SerializeField] private Health playerHealth; // Reference to player health
    [SerializeField] private PlayerCombat playerCombat; // Reference to player combat

    [SerializeField] private float flashDuration = 0.2f;

    private float flashTimer;


    private void Start()
    {
        if (playerHealth == null)
        {
            // Try to find player health if not set
            playerHealth = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Health>();
        }

        if (playerCombat == null)
        {
            // Try to find player combat if not set
            playerCombat = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerCombat>();
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

        // Initialize mana and arrow UI
        UpdateManaUI();
        UpdateArrowUI();
    }

    private void Update()
    {
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;

            if (flashTimer <= 0 && damageFlashPanel != null)
                damageFlashPanel.SetActive(false);
        }

        // Update mana and arrow UI every frame (for mana regeneration)
        if (playerCombat != null)
        {
            UpdateManaUI();
            UpdateArrowUI();
        }
    }
    private void UpdateManaUI()
    {
        if (manaSlider != null && playerCombat != null)
        {
            manaSlider.maxValue = playerCombat.GetMaxMana();
            manaSlider.value = playerCombat.GetCurrentMana();
        }

        if (manaText != null && playerCombat != null)
        {
            manaText.text = $"{Mathf.Floor(playerCombat.GetCurrentMana())}/{playerCombat.GetMaxMana()}";
        }
    }

    private void UpdateArrowUI()
    {
        if (arrowCountText != null && playerCombat != null)
        {
            arrowCountText.text = $"{playerCombat.GetCurrentArrows()}";

            // Optional: Change color if out of arrows
            if (playerCombat.GetCurrentArrows() <= 0)
            {
                arrowCountText.color = Color.red;
                if (arrowIcon != null)
                    arrowIcon.color = new Color(1f, 0.5f, 0.5f);
            }
            else
            {
                arrowCountText.color = Color.white;
                if (arrowIcon != null)
                    arrowIcon.color = Color.white;
            }
        }
    }

    // Add this method to your class if you want to trigger UI updates from outside
    public void RefreshAllUI()
    {
        if (playerHealth != null)
            UpdateHealthUI(playerHealth.GetCurrentHealth());

        UpdateManaUI();
        UpdateArrowUI();
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