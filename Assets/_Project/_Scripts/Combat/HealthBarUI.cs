using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI Assignments")]
    [SerializeField] private Image healthBarFill;   // green fill img here

    private HealthSystem targetHealthSystem;
    private Transform mainCameraTransform;

    void Start()
    {
        // Find the HealthSystem attached to the parent object (Player or Enemy)
        targetHealthSystem = GetComponentInParent<HealthSystem>();
        
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }

        // Subscribe to your existing OnHealthChanged event
        if (targetHealthSystem != null)
        {
            targetHealthSystem.OnHealthChanged += UpdateBar;
        }
    }

    void OnDisable()
    {
        // Unsubscribe safely to secure clear garbage collection lanes
        if (targetHealthSystem != null)
        {
            targetHealthSystem.OnHealthChanged -= UpdateBar;
        }
    }

    void LateUpdate()
    {
        // Simple Billboard effect: Forces the floating UI canvas to always face your monitor's camera
        if (mainCameraTransform != null)
        {
            transform.LookAt(transform.position + mainCameraTransform.forward);
        }
    }

    private void UpdateBar(float currentHealth, float maxHealth)
    {
        if (healthBarFill != null)
        {
            // Set slider fraction scale [0.0f to 1.0f]
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }
}
