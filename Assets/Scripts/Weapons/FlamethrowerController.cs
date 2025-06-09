using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Controlador del lanzallamas que gestiona ParticleSystem y daño continuo.
/// </summary>
public class FlamethrowerController : MonoBehaviour
{
    // Referencias
    private FlamethrowerWeapon weaponData;
    private Transform user;
    private ParticleSystem particles;
    
    // Estado
    private bool isFlaming = false;
    private Dictionary<GameObject, float> enemiesInFlame = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, Coroutine> burnCoroutines = new Dictionary<GameObject, Coroutine>();
    
    // Configuración
    private LayerMask enemyLayerMask;
    private float lastDamageTime = 0f;
    
    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
        if (particles == null)
        {
            particles = gameObject.AddComponent<ParticleSystem>();
        }
        
        enemyLayerMask = LayerMask.GetMask("Enemy");
    }
    
    /// <summary>
    /// Inicializa el controlador con datos del arma.
    /// </summary>
    public void Initialize(FlamethrowerWeapon weapon, Transform weaponUser)
    {
        weaponData = weapon;
        user = weaponUser;
        
        ConfigureParticleSystem();
        
        Debug.Log("FlamethrowerController: Inicializado");
    }
    
    /// <summary>
    /// Configura el ParticleSystem según los datos del arma.
    /// </summary>
    private void ConfigureParticleSystem()
    {
        if (particles == null || weaponData == null) return;
        
        var main = particles.main;
        main.startLifetime = weaponData.MaxFlameRange / weaponData.ProjectileSpeed;
        main.startSpeed = weaponData.ProjectileSpeed;
        main.startSize = 0.3f;
        main.startColor = new Color(1f, 0.5f, 0f, 0.8f); // Naranja
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = particles.emission;
        emission.rateOverTime = 100;
        
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = weaponData.FlameWidth * 15f;
        shape.radius = 0.1f;
        
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        
        var collision = particles.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision2D;
        collision.sendCollisionMessages = true;
        
        var trigger = particles.trigger;
        trigger.enabled = true;
        trigger.inside = ParticleSystemOverlapAction.Callback;
        trigger.outside = ParticleSystemOverlapAction.Callback;
        trigger.enter = ParticleSystemOverlapAction.Callback;
        trigger.exit = ParticleSystemOverlapAction.Callback;
    }
    
    /// <summary>
    /// Inicia el efecto de llama.
    /// </summary>
    public void StartFlame()
    {
        if (particles != null)
        {
            particles.Play();
            isFlaming = true;
            StartCoroutine(DamageCoroutine());
            
            Debug.Log("FlamethrowerController: Llama iniciada");
        }
    }
    
    /// <summary>
    /// Detiene el efecto de llama.
    /// </summary>
    public void StopFlame()
    {
        if (particles != null)
        {
            particles.Stop();
            isFlaming = false;
            
            // Limpiar enemigos en llama
            enemiesInFlame.Clear();
            
            Debug.Log("FlamethrowerController: Llama detenida");
        }
    }
    
    /// <summary>
    /// Actualiza la dirección del lanzallamas.
    /// </summary>
    public void UpdateDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    /// <summary>
    /// Corrutina que aplica daño continuo.
    /// </summary>
    private IEnumerator DamageCoroutine()
    {
        while (isFlaming)
        {
            ApplyDamageToEnemiesInFlame();
            yield return new WaitForSeconds(weaponData.DamageTickRate);
        }
    }
    
    /// <summary>
    /// Aplica daño a todos los enemigos en la llama.
    /// </summary>
    private void ApplyDamageToEnemiesInFlame()
    {
        if (weaponData == null) return;
        
        List<GameObject> enemiesToRemove = new List<GameObject>();
        
        foreach (var kvp in enemiesInFlame)
        {
            GameObject enemy = kvp.Key;
            
            if (enemy == null)
            {
                enemiesToRemove.Add(enemy);
                continue;
            }
            
            // Verificar si el enemigo sigue en rango
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance > weaponData.MaxFlameRange)
            {
                enemiesToRemove.Add(enemy);
                continue;
            }
            
            // Aplicar daño
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                int damageThisTick = weaponData.DamagePerSecond * weaponData.DamageTickRate;
                enemyHealth.TakeDamage(damageThisTick);
                
                Debug.Log($"FlamethrowerController: Aplicó {damageThisTick} de daño a {enemy.name}");
                
                // Aplicar efecto de quemadura si está habilitado
            }
        }
        
        // Remover enemigos que ya no están en rango
        foreach (GameObject enemy in enemiesToRemove)
        {
            enemiesInFlame.Remove(enemy);
        }
    }  
    /// <summary>
    /// Detecta cuando las partículas entran en contacto con enemigos.
    /// </summary>
    private void OnParticleTrigger()
    {
        if (!isFlaming || particles == null) return;
        
        // Obtener partículas que están en trigger
        List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
        int numEnter = particles.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
        
        for (int i = 0; i < numEnter; i++)
        {
            ParticleSystem.Particle particle = enter[i];
            
            // Buscar enemigos cerca de la partícula
            Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(particle.position, 0.5f, enemyLayerMask);
            
            foreach (Collider2D enemyCollider in nearbyEnemies)
            {
                if (enemyCollider.CompareTag("Enemy"))
                {
                    GameObject enemy = enemyCollider.gameObject;
                    
                    // Añadir enemigo a la lista si no está ya
                    if (!enemiesInFlame.ContainsKey(enemy))
                    {
                        enemiesInFlame[enemy] = Time.time;
                        Debug.Log($"FlamethrowerController: Enemigo {enemy.name} entró en la llama");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Método alternativo para detectar enemigos usando OverlapCircle.
    /// </summary>
    private void Update()
    {
        if (!isFlaming) return;
        
        // Detectar enemigos en el área de la llama usando OverlapCircle
        Vector3 flameDirection = transform.right;
        float checkDistance = weaponData.MaxFlameRange * 0.5f;
        Vector3 checkPosition = transform.position + flameDirection * checkDistance;
        
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(checkPosition, weaponData.FlameWidth, enemyLayerMask);
        
        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            if (enemyCollider.CompareTag("Enemy"))
            {
                GameObject enemy = enemyCollider.gameObject;
                
                // Verificar si está en el cono de la llama
                Vector3 toEnemy = (enemy.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(flameDirection, toEnemy);
                
                if (angle <= weaponData.FlameWidth * 15f) // Convertir a ángulo
                {
                    if (!enemiesInFlame.ContainsKey(enemy))
                    {
                        enemiesInFlame[enemy] = Time.time;
                    }
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        // Detener todas las corrutinas de quemadura
        foreach (var coroutine in burnCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        
        burnCoroutines.Clear();
        enemiesInFlame.Clear();
    }
}

