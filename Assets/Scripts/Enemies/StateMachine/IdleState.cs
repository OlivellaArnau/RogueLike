using UnityEngine;

[CreateAssetMenu(fileName = "IdleState", menuName = "Scriptable Objects/States/Generic/Idle")]
public class IdleState : EnemyStateBase
{
    [Tooltip("Intervalo de tiempo entre comprobaciones de detección")]
    [SerializeField] private float detectionInterval = 0.5f;
    
    [Tooltip("Estado al que transicionar cuando se detecta al jugador")]
    [SerializeField] private EnemyStateBase detectedPlayerState;
    
    // Variables de tiempo para optimizar las comprobaciones
    private float detectionTimer;
    
    public override void EnterState(EnemyController enemy)
    {
        // Reiniciar el temporizador de detección
        detectionTimer = 0f;
        
        // Detener el movimiento del enemigo si tiene Rigidbody
        if (enemy.Rb != null)
        {
            enemy.Rb.linearVelocity = Vector2.zero;
        }
        
        // Activar animación de idle si existe
        if (enemy.Animator != null)
        {
            enemy.Animator.SetBool("IsMoving", false);
            enemy.Animator.SetTrigger("Idle");
        }
    }
    
    public override void UpdateState(EnemyController enemy)
    {
        // Incrementar el temporizador
        detectionTimer += Time.deltaTime;
        
        if (detectionTimer >= detectionInterval)
        {
            detectionTimer = 0f;
        }
    }
    
    public override void ExitState(EnemyController enemy)
    {

    }
    
    public override EnemyStateBase CheckTransitions(EnemyController enemy)
    {
        if (enemy.IsPlayerInDetectionRange())
        {
            return detectedPlayerState;
        }
        return null;
    }
}

