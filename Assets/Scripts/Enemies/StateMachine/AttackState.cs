using UnityEngine;


[CreateAssetMenu(fileName = "AttackState", menuName = "Scriptable Objects/States/Generic/Attack")]
public class AttackState : EnemyStateBase
{
    [SerializeField] private EnemyStateBase outOfRangeState;
    
    [SerializeField] private GameObject projectilePrefab;
    
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private string projectileSpawnPointTag = "ProjectileSpawnPoint";
    
    [SerializeField] private bool isMeleeAttack = false;
    
    [SerializeField] private float meleeAttackRadius = 1.5f;
    
    [SerializeField] private LayerMask meleeAttackLayers;
    
    // Temporizador para controlar la cadencia de ataque
    private float attackTimer;
    
    public override void EnterState(EnemyController enemy)
    {
        // Reiniciar el temporizador de ataque
        attackTimer = enemy.EnemyData.AttackCooldown;
        
        // Detener el movimiento si tiene Rigidbody
        if (enemy.Rb != null)
        {
            enemy.Rb.linearVelocity = Vector2.zero;
        }
        
        // Activar animación de preparación de ataque si existe
        if (enemy.Animator != null)
        {
            enemy.Animator.SetBool("IsAttacking", true);
        }
    }
    
    public override void UpdateState(EnemyController enemy)
    {
        attackTimer -= Time.deltaTime;
        
        // Si el temporizador llega a cero, realizar ataque
        if (attackTimer <= 0)
        {
            // Realizar el ataque
            PerformAttack(enemy);
            
            // Reiniciar el temporizador
            attackTimer = enemy.EnemyData.AttackCooldown;
        }
    }
    
    public override void ExitState(EnemyController enemy)
    {
        // Desactivar animación de ataque
        if (enemy.Animator != null)
        {
            enemy.Animator.SetBool("IsAttacking", false);
        }
    }
    
    public override EnemyStateBase CheckTransitions(EnemyController enemy)
    {
        // Si el jugador está fuera de rango, cambiar al estado correspondiente
        if (!IsPlayerInAttackRange(enemy))
        {
            return outOfRangeState;
        }
        return null;
    }
    
    private void PerformAttack(EnemyController enemy)
    {
        // Activar animación de ataque
        if (enemy.Animator != null)
        {
            enemy.Animator.SetTrigger("Attack");
        }
        
        // Según el tipo de ataque, realizar una acción diferente
        if (isMeleeAttack)
        {
            PerformMeleeAttack(enemy);
        }
        else
        {
            PerformRangedAttack(enemy);
        }
    }
    
    private void PerformMeleeAttack(EnemyController enemy)
    {
        // Detectar objetivos en el radio de ataque
        Collider2D[] targets = Physics2D.OverlapCircleAll(
            enemy.transform.position, 
            meleeAttackRadius, 
            meleeAttackLayers
        );
        
        foreach (Collider2D target in targets)
        {
            // Si el objetivo tiene componente de salud, aplicar daño
            Health health = target.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(enemy.EnemyData.Damage);
            }
            
            // Si el objetivo tiene Rigidbody, aplicar fuerza de empuje
            Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                Vector2 pushDirection = (target.transform.position - enemy.transform.position).normalized;
                targetRb.AddForce(pushDirection * 5f, ForceMode2D.Impulse);
            }
        }
    }
    
    private void PerformRangedAttack(EnemyController enemy)
    {
        if (projectilePrefab == null)
            return;
            
        // Determinar el punto de origen del proyectil
        Vector3 spawnPosition = enemy.transform.position;
        Transform spawnPoint = enemy.transform.Find(projectileSpawnPointTag);
        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
        }
        
        // Crear el proyectil
        GameObject projectile = Object.Instantiate(
            projectilePrefab, 
            spawnPosition, 
            enemy.transform.rotation
        );
        
        // Configurar el proyectil
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb != null)
        {
            // Dirección hacia el jugador
            Vector2 direction = enemy.GetDirectionToPlayer();
            projectileRb.linearVelocity = direction * projectileSpeed;
        }
        
        // Configurar el daño del proyectil si tiene el componente adecuado
        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent != null)
        {
            projectileComponent.SetDamage(enemy.EnemyData.Damage);
        }
    }
    
    private bool IsPlayerInAttackRange(EnemyController enemy)
    {
        float attackRange = isMeleeAttack ? meleeAttackRadius : enemy.EnemyData.DetectionRange;
        return enemy.GetDistanceToPlayer() <= attackRange;
    }
    
    // Dibujar gizmos para visualizar el rango de ataque
    private void OnDrawGizmosSelected()
    {
        if (isMeleeAttack)
        {
            Gizmos.color = Color.red;
            //Gizmos.DrawWireSphere(transform.position, meleeAttackRadius);
        }
    }
}

