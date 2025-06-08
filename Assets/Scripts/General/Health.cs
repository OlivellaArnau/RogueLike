using System;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Tooltip("Salud m�xima de la entidad")]
    [SerializeField] private int maxHealth = 100;

    [Tooltip("Salud actual de la entidad")]
    [SerializeField] private int currentHealth;

    [Tooltip("�Es invulnerable esta entidad?")]
    [SerializeField] private bool isInvulnerable = false;

    [Header("Eventos")]
    [Tooltip("Se invoca cuando cambia la salud (salud actual, salud m�xima)")]
    public UnityEvent<int, int> OnHealthChanged;

    [Tooltip("Se invoca cuando la entidad muere")]
    public UnityEvent OnDeath;

    [Tooltip("Se invoca cuando la entidad recibe da�o (cantidad de da�o)")]
    public UnityEvent<int> OnDamageTaken;

    [Tooltip("Se invoca cuando la entidad se cura (cantidad curada)")]
    public UnityEvent<int> OnHealed;

    private void Start()
    {
        // Inicializar la salud actual al valor m�ximo
        currentHealth = maxHealth;

        // Notificar el estado inicial de la salud
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    public void SetMaxHealth(int value)
    {
        maxHealth = Mathf.Max(1, value); // Asegurar que la salud m�xima sea al menos 1
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    public void TakeDamage(int damage)
    {
        // Si es invulnerable, no recibe da�o
        if (isInvulnerable)
            return;
        int actualDamage = Mathf.Min(currentHealth, damage);
        currentHealth -= actualDamage;
        OnDamageTaken?.Invoke(actualDamage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"Entity took {actualDamage} damage. Current health: {currentHealth}/{maxHealth}");
        // Comprobar si ha muerto
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Heal(int amount)
    {
        if (currentHealth <= 0)
            return;

        int healthBefore = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        int actualHealAmount = currentHealth - healthBefore;
        if (actualHealAmount > 0)
        {
            OnHealed?.Invoke(actualHealAmount);
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
    public void Kill()
    {
        if (isInvulnerable)
            return;

        int oldHealth = currentHealth;
        currentHealth = 0;

        OnDamageTaken?.Invoke(oldHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Die();
    }
    private void Die()
    {
        // Evitar llamar a OnDeath m�ltiples veces
        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();

            // Si es un enemigo, llamar a su m�todo Die()
            EnemyController enemyController = GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.Die();
            }
        }
    }
    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }

    // Propiedades p�blicas para acceder a los campos privados
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthPercentage => (float)currentHealth / maxHealth;
    public bool IsInvulnerable => isInvulnerable;
}

