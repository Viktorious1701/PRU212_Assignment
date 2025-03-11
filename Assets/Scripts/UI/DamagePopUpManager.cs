using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    [SerializeField] private GameObject damagePopupPrefab;

    private static DamagePopupManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        DamageSystem.OnDamageApplied += HandleDamageEvent;
    }

    private void OnDisable()
    {
        DamageSystem.OnDamageApplied -= HandleDamageEvent;
    }

    private void HandleDamageEvent(GameObject target, DamageInfo damageInfo)
    {
        // If target is null, return
        if (target == null) return;

        // Get the enemy's position instead of the hit point
        // This will use the center of the game object
        Vector3 enemyPosition = target.transform.position;

        // Get the renderer bounds to find the top of the enemy
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Get the top-most point of the enemy based on their sprite/mesh
            Vector3 topPoint = renderer.bounds.center;
            topPoint.y = renderer.bounds.max.y;
            CreateDamagePopup(topPoint, damageInfo.damageAmount);
        }
        else
        {
            // Fallback if no renderer: use position + offset
            CreateDamagePopup(enemyPosition + new Vector3(0, 1f, 0), damageInfo.damageAmount);
        }
    }

    public static void CreateDamagePopup(Vector3 position, float damage, bool isCritical = false)
    {
        if (instance == null || instance.damagePopupPrefab == null) return;

        // Add a small random offset to prevent overlap if multiple hits occur
        float randomX = Random.Range(-0.3f, 0.3f);
        Vector3 popupPosition = position + new Vector3(randomX, 0.2f, 0);

        // Convert the world position to screen space for UI
        GameObject popupObj = Instantiate(instance.damagePopupPrefab, popupPosition, Quaternion.identity, instance.transform);
        DamagePopup popup = popupObj.GetComponent<DamagePopup>();

        if (popup != null)
        {
            popup.Setup(damage, isCritical);
        }
    }
}