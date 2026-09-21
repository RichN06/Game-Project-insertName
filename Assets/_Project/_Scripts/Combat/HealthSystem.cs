using UnityEngine;
using System;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour, IDamageable
{
    [Header("Health Core Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    // Fires when health changes, useful later for UI health bars
    public event Action<float, float> OnHealthChanged; // Passes (currentHealth, maxHealth)
    public event Action OnDeath;    // Broadcasts when entity dies

    private Animator animator;

    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    // MANDATORY IMPLEMENTATION: Enforced by the IDamageable interface contract!
    public void TakeDamage(float amount, Vector3 hitDirection)
    {
        if (isDead) return;

        currentHealth -= amount;
        // Clamp health so it never falls below 0
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"{gameObject.name} took {amount} damage! Current HP: {currentHealth}/{maxHealth}");
        
        // Invoke events so UI components or particle effects can listen in
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} has DIED!");
        OnDeath?.Invoke();

        if (animator != null) animator.SetTrigger("Die");

        // Architectural safety checks depending on WHO died
        if (gameObject.CompareTag("Player"))
        {
            // Handle player death (e.g., locking input or displaying game over)
            gameObject.SetActive(false);
        }
        else
        {
            // Handle enemy death: Destroy the object instantly or trigger corpse fading
            Destroy(gameObject, 0.1f);
        }
    }
}
