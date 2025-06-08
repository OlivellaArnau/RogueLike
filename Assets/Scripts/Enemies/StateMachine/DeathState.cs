using UnityEngine;

[CreateAssetMenu(fileName = "DeathState", menuName = "Scriptable Objects/States/Generic/Death")]
public class DeathState : EnemyStateBase
{
    [Tooltip("Tiempo de espera antes de destruir el enemigo (para permitir que se reproduzca la animación)")]
    [SerializeField] private float destroyDelay = 2f;
    
    [Tooltip("Efecto visual al morir")]
    [SerializeField] private GameObject deathEffectPrefab;
    
    [Tooltip("¿Debe generar drops al morir?")]
    [SerializeField] private bool generateDrops = true;
    EnemyCoinDrop enemyCoinDrop;

    public override void EnterState(EnemyController enemy)
    {
        // Desactivar colisiones
        Collider2D[] colliders = enemy.GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        
        // Detener movimiento
        if (enemy.Rb != null)
        {
            enemy.Rb.linearVelocity = Vector2.zero;
            enemy.Rb.bodyType = RigidbodyType2D.Static; // Cambiar a estático para evitar más interacciones físicas
        }
        if (enemy.Animator != null)
        {
            enemy.Animator.SetTrigger("Death");
        }

        // Crear efecto de muerte si existe
        if (deathEffectPrefab != null)
        {
            Object.Instantiate(deathEffectPrefab, enemy.transform.position, Quaternion.identity);
        }
        Object.Destroy(enemy.gameObject, destroyDelay);
    }
    
    public override void UpdateState(EnemyController enemy)
    {
    }
    
    public override void ExitState(EnemyController enemy)
    {
    }
    
    public override EnemyStateBase CheckTransitions(EnemyController enemy)
    {
        return null;
    }
}

