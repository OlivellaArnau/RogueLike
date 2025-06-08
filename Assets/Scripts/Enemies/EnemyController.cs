using UnityEngine;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour
{
    [Header("Estado")]
    [Tooltip("Estado inicial del enemigo")]
    [SerializeField] private EnemyStateBase initialState;
    
    [Header("Datos")]
    [Tooltip("Datos configurables del enemigo")]
    [SerializeField] private EnemyDataSO enemyData;
    
    [Header("Eventos")]
    [SerializeField] private UnityEvent onDeath;
    [SerializeField] private UnityEvent<int> onDamageTaken;
    
    // Estado actual en la máquina de estados
    private EnemyStateBase currentState;
    
    // Componentes
    private Health health;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    // Referencia al jugador
    private Transform playerTransform;
    
    // Propiedades públicas para acceder a componentes y datos
    public Rigidbody2D Rb => rb;
    public Health Health => health;
    public EnemyDataSO EnemyData => enemyData;
    public Animator Animator => animator;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public Transform PlayerTransform => playerTransform;
    
    // Variables para debugging
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private string currentStateName;
    
    protected void Awake()
    {
        // Obtener referencias a componentes
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Buscar al jugador
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Configurar la salud máxima desde los datos del enemigo
        if (health != null && enemyData != null)
        {
            health.SetMaxHealth(enemyData.MaxHealth);
        }
    }
    
    private void Start()
    {
        // Inicializar con el estado inicial
        if (initialState != null)
        {
            currentState = initialState;
            currentState.EnterState(this);
            
            if (showDebugInfo)
            {
                currentStateName = currentState.StateName;
            }
        }
        else
        {
            Debug.LogError("No se ha asignado un estado inicial al enemigo: " + gameObject.name);
        }
    }
    
    private void Update()
    {
        if (currentState != null)
        {
            // Actualizar el estado actual
            currentState.UpdateState(this);
            
            // Comprobar si debe cambiar de estado
            EnemyStateBase nextState = currentState.CheckTransitions(this);
            if (nextState != null && nextState != currentState)
            {
                TransitionToState(nextState);
            }
        }
    }
    public void TransitionToState(EnemyStateBase newState)
    {
        if (newState == null)
            return;
            
        // Salir del estado actual
        currentState.ExitState(this);
        
        // Entrar al nuevo estado
        currentState = newState;
        currentState.EnterState(this);
        
        // Actualizar información de debugging
        if (showDebugInfo)
        {
            currentStateName = currentState.StateName;
        }
    }
    public void TakeDamage(int damage)
    {
        if (health != null)
        {
            health.TakeDamage(damage);
            onDamageTaken?.Invoke(damage);
            if (animator != null)
            {
                animator.SetTrigger("Hurt");
            }
        }
    }
    public void Die()
    {
        {
            Destroy(gameObject);
        }
        onDeath?.Invoke();
    }
    public bool IsPlayerInDetectionRange()
    {
        if (playerTransform == null || enemyData == null)
            return false;
            
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= enemyData.DetectionRange;
    }

    public Vector2 GetDirectionToPlayer()
    {
        if (playerTransform == null)
            return Vector2.zero;
            
        Vector2 direction = playerTransform.position - transform.position;
        return direction.normalized;
    }
    public float GetDistanceToPlayer()
    {
        if (playerTransform == null)
            return Mathf.Infinity;
            
        return Vector2.Distance(transform.position, playerTransform.position);
    }
}

