using UnityEngine;

[CreateAssetMenu(fileName = "TurretShootState", menuName = "Enemy/States/Turret/Shoot")]
public class TurretShootState : EnemyStateBase
{
    [Tooltip("Estado al que transicionar cuando se pierde al jugador")]
    [SerializeField] private EnemyStateBase lostPlayerState;
    
    [Tooltip("Estado al que transicionar cuando ya no está apuntando al jugador")]
    [SerializeField] private EnemyStateBase rotateState;
    
    [Tooltip("Prefab del proyectil a disparar")]
    [SerializeField] private GameObject projectilePrefab;
    
    [Tooltip("Velocidad del proyectil")]
    [SerializeField] private float projectileSpeed = 10f;
    
    [Tooltip("Punto de origen del proyectil (si es null, se usa la posición del enemigo)")]
    [SerializeField] private string projectileSpawnPointTag = "ProjectileSpawnPoint";
    
    [Tooltip("Umbral de ángulo para considerar que está apuntando al jugador (en grados)")]
    [SerializeField] private float angleThreshold = 5f;
    
    [Tooltip("Efecto visual al disparar")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    
    // Temporizador para controlar la cadencia de disparo
    private float shootTimer;
    
    public override void EnterState(EnemyController enemy)
    {
        // Reiniciar el temporizador de disparo
        shootTimer = 0f;
        
        // Activar animación de preparación de disparo si existe
        if (enemy.Animator != null)
        {
            enemy.Animator.SetBool("IsShooting", true);
        }
    }
    public override void UpdateState(EnemyController enemy)
    {
        // Incrementar el temporizador
        shootTimer += Time.deltaTime;
        
        // Si el temporizador supera el tiempo de enfriamiento, disparar
        if (shootTimer >= enemy.EnemyData.AttackCooldown)
        {
            Shoot(enemy);
            shootTimer = 0f;
        }
    }
    public override void ExitState(EnemyController enemy)
    {
        // Desactivar animación de disparo
        if (enemy.Animator != null)
        {
            enemy.Animator.SetBool("IsShooting", false);
        }
    }
    public override EnemyStateBase CheckTransitions(EnemyController enemy)
    {
        // Si el jugador está fuera de rango, volver al estado de pérdida
        if (!enemy.IsPlayerInDetectionRange())
        {
            return lostPlayerState;
        }
        
        // Si ya no está apuntando al jugador, volver al estado de rotación
        if (!IsAimingAtPlayer(enemy))
        {
            return rotateState;
        }
        
        // Si no se cumple ninguna condición, permanecer en este estado
        return null;
    }

    private void Shoot(EnemyController enemy)
    {
        // Activar animación de disparo
        if (enemy.Animator != null)
        {
            enemy.Animator.SetTrigger("Shoot");
        }
        
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
            projectileRb.linearVelocity = enemy.transform.right * projectileSpeed;
        }
        
        // Configurar el daño del proyectil si tiene el componente adecuado
        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent != null)
        {
            projectileComponent.SetDamage(enemy.EnemyData.Damage);
        }
        
        // Crear efecto de disparo si existe
        if (muzzleFlashPrefab != null)
        {
            GameObject muzzleFlash = Object.Instantiate(
                muzzleFlashPrefab, 
                spawnPosition, 
                enemy.transform.rotation
            );
            
            // Destruir el efecto después de un tiempo
            Object.Destroy(muzzleFlash, 0.5f);
        }
    }
    private bool IsAimingAtPlayer(EnemyController enemy)
    {
        Vector2 directionToPlayer = enemy.GetDirectionToPlayer();
        
        // Si no hay jugador, no está apuntando
        if (directionToPlayer == Vector2.zero)
            return false;
            
        float targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        
        float currentAngle = enemy.transform.eulerAngles.z;
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));
        
        return angleDifference <= angleThreshold;
    }
}

