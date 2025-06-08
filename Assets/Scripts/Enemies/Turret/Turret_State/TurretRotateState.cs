using UnityEngine;

[CreateAssetMenu(fileName = "TurretRotateState", menuName = "Enemy/States/Turret/Rotate")]
public class TurretRotateState : EnemyStateBase
{
    [Tooltip("Estado al que transicionar cuando se pierde al jugador")]
    [SerializeField] private EnemyStateBase lostPlayerState;
    
    [Tooltip("Estado al que transicionar cuando está apuntando al jugador")]
    [SerializeField] private EnemyStateBase aimingAtPlayerState;
    
    [Tooltip("Umbral de ángulo para considerar que está apuntando al jugador (en grados)")]
    [SerializeField] private float angleThreshold = 5f;
    
    public override void EnterState(EnemyController enemy)
    {
        // Activar animación de rotación si existe
        if (enemy.Animator != null)
        {
            enemy.Animator.SetBool("IsRotating", true);
        }
    }
    
    public override void UpdateState(EnemyController enemy)
    {
        // Calcular dirección hacia el jugador
        Vector2 directionToPlayer = enemy.GetDirectionToPlayer();
        
        // Si no hay jugador, no hacer nada
        if (directionToPlayer == Vector2.zero)
            return;
            
        // Calcular ángulo objetivo
        float targetAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        
        // Rotar gradualmente hacia el jugador
        float currentAngle = enemy.transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(
            currentAngle, 
            targetAngle, 
            enemy.EnemyData.RotationSpeed * Time.deltaTime
        );
        
        enemy.transform.rotation = Quaternion.Euler(0, 0, newAngle);
    }
    
    public override void ExitState(EnemyController enemy)
    {
        // Desactivar animación de rotación
        if (enemy.Animator != null)
        {
            enemy.Animator.SetBool("IsRotating", false);
        }
    }
    
    public override EnemyStateBase CheckTransitions(EnemyController enemy)
    {
        // Si el jugador está fuera de rango, volver al estado de pérdida
        if (!enemy.IsPlayerInDetectionRange())
        {
            return lostPlayerState;
        }
        
        // Si está apuntando al jugador, cambiar al estado de disparo
        if (IsAimingAtPlayer(enemy))
        {
            return aimingAtPlayerState;
        }
        
        // Si no se cumple ninguna condición, permanecer en este estado
        return null;
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

