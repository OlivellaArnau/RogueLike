using UnityEngine;

[CreateAssetMenu(fileName = "BombChaseState", menuName = "Enemy/States/Bomb/Chase")]
public class BombChaseState : EnemyStateBase
{
    [Tooltip("Estado al que transicionar cuando se pierde al jugador")]
    [SerializeField] private EnemyStateBase lostPlayerState;
    
    [Tooltip("Estado al que transicionar cuando está lo suficientemente cerca del jugador")]
    [SerializeField] private EnemyStateBase explodeState;
    
    [Tooltip("¿Debe acelerar cuando está cerca del jugador?")]
    [SerializeField] private bool accelerateNearPlayer = true;
    
    [Tooltip("Multiplicador de velocidad cuando está cerca del jugador")]
    [SerializeField] private float speedMultiplierNearPlayer = 1.5f;
    
    [Tooltip("Distancia a la que comienza a acelerar")]
    [SerializeField] private float accelerationDistance = 5f;
    
    public override void EnterState(EnemyController enemy)
    {
        // Activar animación de persecución
        if (enemy.Animator != null)
        {
            enemy.Animator.SetBool("IsMoving", true);
            enemy.Animator.SetTrigger("StartChase");
        }
    }
    
    public override void UpdateState(EnemyController enemy)
    {
        // Obtener la dirección hacia el jugador
        Vector2 directionToPlayer = enemy.GetDirectionToPlayer();
        
        // Si no hay jugador, no hacer nada
        if (directionToPlayer == Vector2.zero)
            return;
            
        // Calcular velocidad basada en la distancia al jugador
        float speed = enemy.EnemyData.MoveSpeed;
        
        // Si debe acelerar cuando está cerca del jugador
        if (accelerateNearPlayer)
        {
            float distanceToPlayer = enemy.GetDistanceToPlayer();
            
            if (distanceToPlayer <= accelerationDistance)
            {
                // Aumentar velocidad gradualmente a medida que se acerca al jugador
                float speedFactor = 1f + (1f - distanceToPlayer / accelerationDistance) * (speedMultiplierNearPlayer - 1f);
                speed *= speedFactor;
            }
        }
        
        // Mover hacia el jugador
        if (enemy.Rb != null)
        {
            enemy.Rb.linearVelocity = directionToPlayer * speed;
        }
        
        // Rotar para mirar hacia el jugador
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        enemy.transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Comprobar si es un BombController para funcionalidades específicas
        BombController bombController = enemy as BombController;
        if (bombController != null)
        {
            // Si está en rango de preparación pero aún no en rango de explosión
            if (bombController.IsPlayerInPrimeRange() && enemy.GetDistanceToPlayer() > 0.5f)
            {
                bombController.StartPriming();
            }
        }
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
        
        // Si es un BombController y no va a explotar, detener la preparación
        BombController bombController = enemy as BombController;
        if (bombController != null && !bombController.IsPlayerInPrimeRange())
        {
            bombController.StopPriming();
        }
    }
    
    public override EnemyStateBase CheckTransitions(EnemyController enemy)
    {
        // Obtener datos específicos de la bomba si es posible
        BombEnemyData bombData = enemy.EnemyData as BombEnemyData;
        
        // Si el jugador está fuera de rango y no está configurado para persecución persistente, volver al estado de pérdida
        if (!enemy.IsPlayerInDetectionRange() && (bombData == null || !bombData.PersistentChase))
        {
            return lostPlayerState;
        }
        
        // Si es un BombController y está en rango de preparación, comprobar si debe explotar
        BombController bombController = enemy as BombController;
        if (bombController != null && bombController.IsPlayerInPrimeRange())
        {
            // Si está muy cerca del jugador, explotar
            if (enemy.GetDistanceToPlayer() <= 0.5f)
            {
                return explodeState;
            }
        }
        
        // Si no se cumple ninguna condición, permanecer en este estado
        return null;
    }
}

