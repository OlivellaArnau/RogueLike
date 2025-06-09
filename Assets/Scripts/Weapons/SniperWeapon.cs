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
        Debug.Log("[Sniper] Intentando disparar");
        if (!poolInitialized) InitializePool();

        Combat_Behaviour combat = user.GetComponent<Combat_Behaviour>();
        if (combat == null) return false;

        Vector3 firePos = combat.GetPointerPosition();
        Vector3 dir = combat.GetPointerDirection();

        GameObject proj = SimpleProjectilePool.Instance.GetProjectile();
        if (proj == null) return false;

        var pooled = proj.GetComponent<PooledProjectile>();
        if (pooled != null)
        {
            pooled.Initialize(firePos, dir, damage, projectileSpeed);
            pooled.SetLifetime(projectileLifetime);
            pooled.SetEnemyLayerMask(combat.GetEnemyLayerMask());
            return true;
        }

        return false;
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
    private LayerMask GetEnemyLayerMask(Transform user)
    {
        Combat_Behaviour combatBehaviour = user.GetComponent<Combat_Behaviour>();
        if (combatBehaviour != null)
        {
            // Usar reflexión para obtener enemyLayerMask si es accesible
            var field = combatBehaviour.GetType().GetField("enemyLayerMask", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                return (LayerMask)field.GetValue(combatBehaviour);
            }
        }
        
        // Fallback: usar máscara por defecto
        return LayerMask.GetMask("Enemy");
    }
    public override void OnEquip(Transform user)
    {
        base.OnEquip(user);
        
        if (initializePoolOnEquip && !poolInitialized)
        {
            InitializePool();
        }
        
        Debug.Log($"SniperWeapon: {weaponName} equipado");
    }
    public override void OnUnequip(Transform user)
    {
        base.OnUnequip(user);
        
        Debug.Log($"SniperWeapon: {weaponName} desequipado");
    }
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
    public void SetProjectilePrefab(GameObject prefab)
    {
        projectilePrefab = prefab;
        poolInitialized = false; // Reinicializar pool con nuevo prefab
    }
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

