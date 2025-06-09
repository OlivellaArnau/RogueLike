using UnityEngine;

/// <summary>
/// Implementación específica de arma melee que se integra perfectamente
/// con Combat_Behaviour existente y corrige interacciones con Health.
/// </summary>
[CreateAssetMenu(fileName = "New Basic Melee", menuName = "Weapons/Basic Melee")]
public class BasicMeleeWeapon : MeleeWeapon
{
    [Header("Configuración Específica")]
    [SerializeField] private bool useCombatBehaviourDamage = true;
    [SerializeField] private int damageMultiplier = 1;
    [SerializeField] private bool debugMode = false;
    
    /// <summary>
    /// Implementación corregida que utiliza Combat_Behaviour apropiadamente.
    /// </summary>
    public override bool UseWeapon(Transform user, Vector3 target)
    {
        WeaponManager weaponManager = user.GetComponent<WeaponManager>();
        if (weaponManager == null)
        {
            if (debugMode) Debug.LogError("BasicMeleeWeapon: WeaponManager no encontrado");
            return false;
        }
        
        Combat_Behaviour combatBehaviour = weaponManager.GetCombatBehaviour();
        if (combatBehaviour == null)
        {
            if (debugMode) Debug.LogError("BasicMeleeWeapon: Combat_Behaviour no encontrado");
            return false;
        }
        
        // Obtener enemigos en rango usando el WeaponManager
        Collider2D[] enemiesInRange = weaponManager.GetEnemiesInArea(user.position, attackRadius);
        
        if (enemiesInRange.Length == 0)
        {
            if (debugMode) Debug.Log("BasicMeleeWeapon: No hay enemigos en rango de ataque");
            return false;
        }
        
        // Procesar ataque a enemigos
        int enemiesHit = 0;
        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            if (enemiesHit >= maxTargets && !canHitMultipleEnemies)
                break;
            
            if (IsValidTarget(enemyCollider))
            {
                bool hitSuccess = AttackEnemyImproved(enemyCollider.gameObject, user.position, combatBehaviour);
                if (hitSuccess)
                {
                    enemiesHit++;
                    
                    if (!canHitMultipleEnemies)
                        break;
                }
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"BasicMeleeWeapon: {weaponName} atacó exitosamente a {enemiesHit} enemigos");
        }
        
        return enemiesHit > 0;
    }
    private bool IsValidTarget(Collider2D collider)
    {
        if (collider == null) return false;
        
        // Verificar tag de enemigo
        if (!collider.CompareTag("Enemy")) return false;
        
        // Verificar que tenga componente Health
        Health enemyHealth = collider.GetComponent<Health>();
        if (enemyHealth == null)
        {
            if (debugMode) Debug.LogWarning($"BasicMeleeWeapon: Enemigo {collider.name} no tiene componente Health");
            return false;
        }
        return true;
    }
    private bool AttackEnemyImproved(GameObject enemy, Vector3 attackerPosition, Combat_Behaviour combatBehaviour)
    {
        if (enemy == null) return false;
        
        Health enemyHealth = enemy.GetComponent<Health>();
        if (enemyHealth == null)
        {
            if (debugMode) Debug.LogError($"BasicMeleeWeapon: {enemy.name} no tiene componente Health");
            return false;
        }
        
        try
        {
 
            int finalDamage = CalculateFinalDamage(combatBehaviour);
            
            enemyHealth.TakeDamage(finalDamage);
            
            if (debugMode)
            {
                Debug.Log($"BasicMeleeWeapon: Aplicó {finalDamage} de daño a {enemy.name}");
            }
            ApplyAdditionalEffects(enemy, attackerPosition);
            
            return true;
        }
        catch (System.Exception e)
        {
            if (debugMode)
            {
                Debug.LogError($"BasicMeleeWeapon: Error al atacar {enemy.name}: {e.Message}");
            }
            return false;
        }
    }

    private int CalculateFinalDamage(Combat_Behaviour combatBehaviour)
    {
        int finalDamage = damage * damageMultiplier;
        
        if (useCombatBehaviourDamage && combatBehaviour != null)
        {
            try
            {
                var damageField = combatBehaviour.GetType().GetField("damage");
                if (damageField != null)
                {
                    int combatDamage = (int)damageField.GetValue(combatBehaviour);
                    finalDamage = Mathf.Max(finalDamage, combatDamage * damageMultiplier);
                }
            }
            catch
            {
                // Si no se puede obtener, usar daño del arma
                if (debugMode) Debug.Log("BasicMeleeWeapon: Usando daño del arma (Combat_Behaviour no accesible)");
            }
        }
        
        return finalDamage;
    }
    private void ApplyAdditionalEffects(GameObject enemy, Vector3 attackerPosition)
    {
        // Aplicar knockback mejorado
        if (knockbackForce > 0f)
        {
            ApplyKnockbackImproved(enemy, attackerPosition);
        }
    }
    
    protected override void ApplyKnockback(GameObject enemy, Vector3 attackerPosition)
    {
        ApplyKnockbackImproved(enemy, attackerPosition);
    }

    private void ApplyKnockbackImproved(GameObject enemy, Vector3 attackerPosition)
    {
        if (enemy == null) return;
        
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb == null)
        {
            if (debugMode) Debug.Log($"BasicMeleeWeapon: {enemy.name} no tiene Rigidbody2D para knockback");
            return;
        }
        Vector2 knockbackDirection = (enemy.transform.position - attackerPosition).normalized;

        if (knockbackDirection.magnitude < 0.1f)
        {
            // Si están muy cerca, usar dirección aleatoria
            knockbackDirection = Random.insideUnitCircle.normalized;
        }

        enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        
        if (debugMode)
        {
            Debug.Log($"BasicMeleeWeapon: Aplicó knockback de {knockbackForce} a {enemy.name}");
        }
    }

    public override void OnEquip(Transform user)
    {
        base.OnEquip(user);
        
        if (debugMode)
        {
            Debug.Log($"BasicMeleeWeapon: {weaponName} equipada por {user.name}");
        }
    }

    public override void OnUnequip(Transform user)
    {
        base.OnUnequip(user);
        
        if (debugMode)
        {
            Debug.Log($"BasicMeleeWeapon: {weaponName} desequipada por {user.name}");
        }
    }
}

