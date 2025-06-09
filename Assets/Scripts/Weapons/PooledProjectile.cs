using UnityEngine;

/// <summary>
/// Componente que se añade a proyectiles para gestionar su ciclo de vida en el pool.
/// </summary>
public class PooledProjectile : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float speed = 20f;
    [SerializeField] private int damage = 25;
    [SerializeField] private LayerMask enemyLayerMask = -1;
    
    // Referencias
    private SimpleProjectilePool pool;
    private Rigidbody2D rb;
    private float spawnTime;
    private Vector3 direction;
    
    // Estado
    private bool isActive = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
    }
    
    private void OnEnable()
    {
        spawnTime = Time.time;
        isActive = true;
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        // Verificar tiempo de vida
        if (Time.time - spawnTime >= lifetime)
        {
            ReturnToPool();
            return;
        }
        
        // Verificar límites del mundo
        if (IsOutOfBounds())
        {
            ReturnToPool();
            return;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        
        // Verificar si es un enemigo
        if (IsInLayerMask(other.gameObject.layer, enemyLayerMask))
        {
            // Aplicar daño
            Health enemyHealth = other.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"Proyectil causó {damage} de daño a {other.name}");
            }
            
            // Retornar al pool
            ReturnToPool();
        }
        // Verificar obstáculos (paredes, etc.)
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            ReturnToPool();
        }
    }
    
    /// <summary>
    /// Inicializa el proyectil con parámetros específicos.
    /// </summary>
    public void Initialize(Vector3 startPosition, Vector3 fireDirection, int projectileDamage, float projectileSpeed)
    {
        transform.position = startPosition;
        direction = fireDirection.normalized;
        damage = projectileDamage;
        speed = projectileSpeed;
        
        // Configurar movimiento
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        
        // Rotar hacia la dirección de movimiento
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        spawnTime = Time.time;
        isActive = true;
    }
    
    /// <summary>
    /// Establece la referencia al pool.
    /// </summary>
    public void SetPool(SimpleProjectilePool projectilePool)
    {
        pool = projectilePool;
    }
    
    /// <summary>
    /// Retorna el proyectil al pool.
    /// </summary>
    public void ReturnToPool()
    {
        if (!isActive) return;
        
        isActive = false;
        
        // Detener movimiento
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Retornar al pool
        if (pool != null)
        {
            pool.ReturnProjectile(gameObject);
        }
        else
        {
            // Fallback: desactivar si no hay pool
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Verifica si el proyectil está fuera de los límites del mundo.
    /// </summary>
    private bool IsOutOfBounds()
    {
        Vector3 pos = transform.position;
        
        // Límites básicos (pueden ajustarse según el juego)
        float maxDistance = 50f;
        
        return Mathf.Abs(pos.x) > maxDistance || Mathf.Abs(pos.y) > maxDistance;
    }
    
    /// <summary>
    /// Verifica si un layer está en una LayerMask.
    /// </summary>
    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
    
    /// <summary>
    /// Configura el tiempo de vida del proyectil.
    /// </summary>
    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;
    }
    
    /// <summary>
    /// Configura la máscara de capas de enemigos.
    /// </summary>
    public void SetEnemyLayerMask(LayerMask mask)
    {
        enemyLayerMask = mask;
    }
}

