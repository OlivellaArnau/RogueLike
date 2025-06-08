using UnityEngine;

[CreateAssetMenu(fileName = "ChaseState", menuName = "Scriptable Objects/States/Generic/Chase")]
public class ChaseState : EnemyStateBase
{
    [Tooltip("Estado al que transicionar cuando se pierde al jugador")]
    [SerializeField] private EnemyStateBase lostPlayerState;
    
    [Tooltip("Estado al que transicionar cuando se está lo suficientemente cerca del jugador")]
    [SerializeField] private EnemyStateBase closeToPlayerState;
    
    [Tooltip("Distancia mínima para considerar que está cerca del jugador")]
    [SerializeField] private float minDistanceToPlayer = 1.5f;
    
    [Tooltip("¿Debe el enemigo rotar para mirar hacia el jugador?")]
    [SerializeField] private bool rotateTowardsPlayer = true;
    
    public override void EnterState(EnemyController enemy)
    {
        // Activar animación de movimiento si existe
        if (enemy.Animator != null)
        {
            enemy.Animator.SetBool("IsMoving", true);
        }
    }
    
    public override void UpdateState(EnemyController enemy)
    {
        // Obtener la dirección hacia el jugador
        Vector2 directionToPlayer = enemy.GetDirectionToPlayer();
        
        // Si no hay jugador, no hacer nada
        if (directionToPlayer == Vector2.zero)
            return;
            
        // Mover hacia el jugador
        if (enemy.Rb != null)
        {
            enemy.Rb.linearVelocity = directionToPlayer * enemy.EnemyData.MoveSpeed;
        }
        
        // Rotar para mirar hacia el jugador si está configurado
        if (rotateTowardsPlayer)
        {
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            
            // Aplicar rotación gradual
            float currentAngle = enemy.transform.eulerAngles.z;
            float newAngle = Mathf.MoveTowardsAngle(
                currentAngle, 
                angle, 
                enemy.EnemyData.RotationSpeed * Time.deltaTime
            );
            
            enemy.transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
        
        // Actualizar animaciones basadas en la dirección si es necesario
        UpdateAnimationDirection(enemy, directionToPlayer);
    }
    
    public override void ExitState(EnemyController enemy)
    {
        // Detener el movimiento al salir del estado
        if (enemy.Rb != null)
        {
            enemy.Rb.linearVelocity = Vector2.zero;
        }
        
        // Desactivar animación de movimiento
        if (enemy.Animator != null)
        {
            enemy.Animator.SetBool("IsMoving", false);
        }
    }
    
    public override EnemyStateBase CheckTransitions(EnemyController enemy)
    {
        // Si el jugador está fuera de rango, volver al estado de pérdida
        if (!enemy.IsPlayerInDetectionRange())
        {
            return lostPlayerState;
        }
        
        // Si está lo suficientemente cerca del jugador, cambiar al estado correspondiente
        if (closeToPlayerState != null && enemy.GetDistanceToPlayer() <= minDistanceToPlayer)
        {
            return closeToPlayerState;
        }
        
        // Si no se cumple ninguna condición, permanecer en este estado
        return null;
    }
    private void UpdateAnimationDirection(EnemyController enemy, Vector2 direction)
    {
        if (enemy.Animator == null)
            return;
            
        // Ejemplo de cómo actualizar un blend tree basado en la dirección
        // Esto asume que tienes un blend tree configurado con parámetros MoveX y MoveY
        enemy.Animator.SetFloat("MoveX", direction.x);
        enemy.Animator.SetFloat("MoveY", direction.y);
    }
}

