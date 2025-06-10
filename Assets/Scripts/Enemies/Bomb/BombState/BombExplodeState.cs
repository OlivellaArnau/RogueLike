using UnityEngine;

[CreateAssetMenu(fileName = "BombExplodeState", menuName = "Enemy/States/Bomb/Explode")]
public class BombExplodeState : EnemyStateBase
{
    [Tooltip("Radio de la explosión")]
    [SerializeField] private float explosionRadius = 3f;
    
    [Tooltip("Fuerza de la explosión")]
    [SerializeField] private float explosionForce = 10f;
    
    [Tooltip("Capas afectadas por la explosión")]
    [SerializeField] private LayerMask affectedLayers;
    
    [Tooltip("Prefab del efecto visual de explosión")]
    [SerializeField] private GameObject explosionEffectPrefab;
    
    [Tooltip("Tiempo de preparación antes de explotar")]
    [SerializeField] private float primeTime = 0.5f;
    
    [Tooltip("¿Debe aplicar daño al jugador?")]
    [SerializeField] private bool damagePlayer = true;
    
    [Tooltip("Multiplicador de daño de la explosión respecto al daño base")]
    [SerializeField] private float damageMultiplier = 1.5f;
    
    // Temporizador para la explosión
    private float explosionTimer;
    
    // Flag para controlar si ya ha explotado
    private bool hasExploded = false;
    
    public override void EnterState(EnemyController enemy)
    {
        // Reiniciar variables
        explosionTimer = 0f;
        hasExploded = false;
        
        // Detener movimiento
        if (enemy.Rb != null)
        {
            enemy.Rb.linearVelocity = Vector2.zero;
        }
        
        // Activar animación de preparación para explotar
        if (enemy.Animator != null)
        {
            enemy.Animator.SetTrigger("Prime");
            enemy.Animator.SetBool("IsPrimed", true);
        }
        
        // Si es un BombController, llamar a su método específico
        BombController bombController = enemy as BombController;
        if (bombController != null)
        {
            bombController.StartPriming();
        }
    }
    
    public override void UpdateState(EnemyController enemy)
    {
        // Incrementar el temporizador
        explosionTimer += Time.deltaTime;
        
        // Si ha pasado el tiempo de preparación y aún no ha explotado, explotar
        if (explosionTimer >= primeTime && !hasExploded)
        {
            Explode(enemy);
            hasExploded = true;
        }
    }
    
    public override void ExitState(EnemyController enemy)
    {
        // Este estado no debería salir, ya que el enemigo se destruye
    }
    
    public override EnemyStateBase CheckTransitions(EnemyController enemy)
    {
        // No hay transiciones desde este estado
        return null;
    }

    // MODIFICACIÓN EN BombExplodeState.cs
    private void Explode(EnemyController enemy)
    {
        // Activar animación de explosión
        if (enemy.Animator != null)
        {
            enemy.Animator.SetTrigger("Explode");
        }

        // Crear efecto visual de explosión
        if (explosionEffectPrefab != null)
        {
            Object.Instantiate(explosionEffectPrefab, enemy.transform.position, Quaternion.identity);
        }

        // Detectar objetos en el radio de explosión
        Collider2D[] colliders = Physics2D.OverlapCircleAll(enemy.transform.position, explosionRadius, affectedLayers);

        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject == enemy.gameObject) continue;

            float distance = Vector2.Distance(enemy.transform.position, collider.transform.position);
            float damageRatio = 1f - (distance / explosionRadius);
            int damage = Mathf.RoundToInt(enemy.EnemyData.Damage * damageMultiplier * damageRatio);

            if (collider.CompareTag("Player") && !damagePlayer) continue;

            Health health = collider.GetComponent<Health>();
            if (health != null && damage > 0)
            {
                health.TakeDamage(damage);
            }

            Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
            if (rb != null && damage > 0)
            {
                Vector2 direction = (collider.transform.position - enemy.transform.position);
                float forceMagnitude = explosionForce * (1f - Mathf.Clamp01(distance / explosionRadius));

                if (direction.sqrMagnitude > 0.001f)
                {
                    direction.Normalize();
                    rb.AddForce(direction * forceMagnitude, ForceMode2D.Impulse);
                }
            }
        }

        // Notificar a la sala que el enemigo ha muerto
        Room room = enemy.GetComponentInParent<Room>();
        if (room != null)
        {
            // En lugar de SendMessage, llamamos directamente al método que necesita
            room.OnEnemyDefeated(enemy.gameObject);
        }

        // Reproducir sonido antes de destruir
        if (enemy is BombController bomb)
        {
            bomb.PlayExplosionSound();
        }

        // Destruir el enemigo después de un pequeño retraso para que se vea la animación
        Object.Destroy(enemy.gameObject, 0.2f);
    }
}

