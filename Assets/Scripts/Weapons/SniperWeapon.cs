using UnityEngine;

/// <summary>
/// Arma Sniper que utiliza object pooling para proyectiles.
/// Se integra con el pointer existente para apuntado.
/// </summary>
[CreateAssetMenu(fileName = "New Sniper Weapon", menuName = "Weapons/Sniper Weapon")]
public class SniperWeapon : RangedWeapon
{
    [Header("Configuración Sniper")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private bool penetrateEnemies = false;
    [SerializeField] private int maxPenetrations = 1;
    
    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 20;
    [SerializeField] private bool initializePoolOnEquip = true;
    
    // Estado del arma
    private bool poolInitialized = false;
    
    /// <summary>
    /// Implementación del disparo sniper con pooling.
    /// </summary>
    public override bool UseWeapon(Transform user, Vector3 target)
    {
        // Verificar que el pool esté inicializado
        if (!poolInitialized)
        {
            InitializePool();
        }
        
        // Obtener posición y dirección de disparo
        Vector3 firePosition = GetFirePosition(user);
        Vector3 fireDirection = GetFireDirection(user, target);
        
        // Obtener proyectil del pool
        GameObject projectile = SimpleProjectilePool.Instance.GetProjectile();
        if (projectile == null)
        {
            Debug.LogError("SniperWeapon: No se pudo obtener proyectil del pool");
            return false;
        }
        
        // Configurar proyectil
        PooledProjectile pooledComponent = projectile.GetComponent<PooledProjectile>();
        if (pooledComponent != null)
        {
            pooledComponent.Initialize(firePosition, fireDirection, damage, projectileSpeed);
            pooledComponent.SetLifetime(projectileLifetime);
            pooledComponent.SetEnemyLayerMask(GetEnemyLayerMask(user));
        }
        else
        {
            Debug.LogError("SniperWeapon: Proyectil no tiene componente PooledProjectile");
            SimpleProjectilePool.Instance.ReturnProjectile(projectile);
            return false;
        }
        
        Debug.Log($"SniperWeapon: {weaponName} disparado hacia {fireDirection}");
        return true;
    }
    
    /// <summary>
    /// Inicializa el pool de proyectiles.
    /// </summary>
    private void InitializePool()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("SniperWeapon: projectilePrefab no asignado");
            return;
        }
        
        // Configurar el pool
        SimpleProjectilePool.Instance.SetProjectilePrefab(projectilePrefab);
        poolInitialized = true;
        
        Debug.Log($"SniperWeapon: Pool inicializado para {weaponName}");
    }
    
    /// <summary>
    /// Obtiene la máscara de capas de enemigos del WeaponManager.
    /// </summary>
    private LayerMask GetEnemyLayerMask(Transform user)
    {
        WeaponManager weaponManager = user.GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            // Usar reflexión para obtener enemyLayerMask si es accesible
            var field = weaponManager.GetType().GetField("enemyLayerMask", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (LayerMask)field.GetValue(weaponManager);
            }
        }
        
        // Fallback: usar máscara por defecto
        return LayerMask.GetMask("Enemy");
    }
    
    /// <summary>
    /// Método llamado cuando el arma es equipada.
    /// </summary>
    public override void OnEquip(Transform user)
    {
        base.OnEquip(user);
        
        if (initializePoolOnEquip && !poolInitialized)
        {
            InitializePool();
        }
        
        Debug.Log($"SniperWeapon: {weaponName} equipado");
    }
    
    /// <summary>
    /// Método llamado cuando el arma es desequipada.
    /// </summary>
    public override void OnUnequip(Transform user)
    {
        base.OnUnequip(user);
        
        Debug.Log($"SniperWeapon: {weaponName} desequipado");
    }
    
    /// <summary>
    /// Verifica si el arma puede ser usada considerando factores específicos del sniper.
    /// </summary>
    public override bool CanUse(float lastUseTime)
    {
        if (!base.CanUse(lastUseTime))
            return false;
        
        // Verificaciones adicionales específicas del sniper
        if (!poolInitialized)
        {
            InitializePool();
        }
        
        return poolInitialized;
    }
    
    /// <summary>
    /// Obtiene información de estado del arma.
    /// </summary>
    public string GetStatusInfo()
    {
        string status = $"Arma: {weaponName}\n";
        status += $"Daño: {damage}\n";
        status += $"Alcance: {range}\n";
        status += $"Velocidad: {projectileSpeed}\n";
        status += $"Cooldown: {cooldown}s\n";
        status += $"Pool: {(poolInitialized ? "Inicializado" : "No inicializado")}";
        
        return status;
    }
    
    /// <summary>
    /// Configura el prefab del proyectil.
    /// </summary>
    public void SetProjectilePrefab(GameObject prefab)
    {
        projectilePrefab = prefab;
        poolInitialized = false; // Reinicializar pool con nuevo prefab
    }
    
    /// <summary>
    /// Obtiene estadísticas del pool.
    /// </summary>
    public void PrintPoolStatistics()
    {
        if (SimpleProjectilePool.Instance != null)
        {
            SimpleProjectilePool.Instance.PrintPoolStats();
        }
        else
        {
            Debug.Log("SniperWeapon: Pool no disponible");
        }
    }
}

