using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Weapon", menuName = "Weapons/Melee Weapon")]
public class MeleeWeapon : WeaponBase
{
    [Header("Configuración Melee")]
    [SerializeField] protected float attackRadius = 1.5f;
    [SerializeField] protected float knockbackForce = 5f;
    [SerializeField] protected bool canHitMultipleEnemies = true;
    [SerializeField] protected int maxTargets = 3;
    
    public float AttackRadius => attackRadius;
    public float KnockbackForce => knockbackForce;
    public bool CanHitMultipleEnemies => canHitMultipleEnemies;
    public int MaxTargets => maxTargets;

    public override bool UseWeapon(Transform user, Vector3 target)
    {
        Combat_Behaviour combatBehaviour = user.GetComponent<Combat_Behaviour>();
        if (combatBehaviour == null)
        {
            Debug.LogError("MeleeWeapon: Combat_Behaviour no encontrado en el usuario");
            return false;
        }

        Collider2D[] enemiesInRange = combatBehaviour.GetEnemiesInArea(user.position, attackRadius);

        if (enemiesInRange.Length == 0)
        {
            Debug.Log("MeleeWeapon: No hay enemigos en rango");
            return false;
        }

        int enemiesHit = 0;
        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            if (enemiesHit >= maxTargets && !canHitMultipleEnemies)
                break;

            if (enemyCollider.CompareTag("Enemy"))
            {
                AttackEnemy(enemyCollider.gameObject, user.position);
                enemiesHit++;

                if (!canHitMultipleEnemies)
                    break;
            }
        }

        Debug.Log($"MeleeWeapon: {weaponName} atacó a {enemiesHit} enemigos");
        return enemiesHit > 0;
    }

    protected virtual void AttackEnemy(GameObject enemy, Vector3 attackerPosition)
    {
        // Aplicar daño usando el componente Health
        Health enemyHealth = enemy.GetComponent<Health>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            Debug.Log($"MeleeWeapon: Causó {damage} de daño a {enemy.name}");
        }
        
        // Aplicar knockback si está configurado
        if (knockbackForce > 0f)
        {
            ApplyKnockback(enemy, attackerPosition);
        }
    }

    protected virtual void ApplyKnockback(GameObject enemy, Vector3 attackerPosition)
    {
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            Vector2 knockbackDirection = (enemy.transform.position - attackerPosition).normalized;
            enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }
}

