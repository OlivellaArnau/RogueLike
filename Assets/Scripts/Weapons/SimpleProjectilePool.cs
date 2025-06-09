using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema simple de object pooling para proyectiles.
/// Optimiza rendimiento evitando creación/destrucción constante.
/// </summary>
public class SimpleProjectilePool : MonoBehaviour
{
    [Header("Configuración del Pool")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 50;
    [SerializeField] private Transform poolParent;
    
    // Pool de proyectiles
    private Queue<GameObject> availableProjectiles = new Queue<GameObject>();
    private List<GameObject> activeProjectiles = new List<GameObject>();
    
    // Singleton simple
    private static SimpleProjectilePool instance;
    public static SimpleProjectilePool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SimpleProjectilePool>();
                if (instance == null)
                {
                    GameObject poolGO = new GameObject("ProjectilePool");
                    instance = poolGO.AddComponent<SimpleProjectilePool>();
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void InitializePool()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("SimpleProjectilePool: projectilePrefab no asignado");
            return;
        }
        
        // Crear parent si no existe
        if (poolParent == null)
        {
            poolParent = new GameObject("PooledProjectiles").transform;
            poolParent.SetParent(transform);
        }
        
        // Crear proyectiles iniciales
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewProjectile();
        }
        
        Debug.Log($"SimpleProjectilePool: Inicializado con {initialPoolSize} proyectiles");
    }
    private GameObject CreateNewProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, poolParent);
        projectile.SetActive(false);
        
        // Añadir componente de retorno al pool si no existe
        PooledProjectile pooledComponent = projectile.GetComponent<PooledProjectile>();
        if (pooledComponent == null)
        {
            pooledComponent = projectile.AddComponent<PooledProjectile>();
        }
        pooledComponent.SetPool(this);
        
        availableProjectiles.Enqueue(projectile);
        return projectile;
    }
    public GameObject GetProjectile()
    {
        GameObject projectile;
        
        if (availableProjectiles.Count > 0)
        {
            projectile = availableProjectiles.Dequeue();
        }
        else if (activeProjectiles.Count < maxPoolSize)
        {
            projectile = CreateNewProjectile();
            availableProjectiles.Dequeue(); // Remover del queue ya que lo vamos a usar
        }
        else
        {
            // Pool lleno, reutilizar el más antiguo
            projectile = activeProjectiles[0];
            ReturnProjectile(projectile);
            projectile = availableProjectiles.Dequeue();
        }
        
        projectile.SetActive(true);
        activeProjectiles.Add(projectile);
        
        return projectile;
    }
    
    public void ReturnProjectile(GameObject projectile)
    {
        if (projectile == null) return;
        
        projectile.SetActive(false);
        projectile.transform.SetParent(poolParent);
        
        activeProjectiles.Remove(projectile);
        availableProjectiles.Enqueue(projectile);
    }
    public void SetProjectilePrefab(GameObject prefab)
    {
        projectilePrefab = prefab;
    }
    public void PrintPoolStats()
    {
        Debug.Log($"Pool Stats - Disponibles: {availableProjectiles.Count}, Activos: {activeProjectiles.Count}, Total: {availableProjectiles.Count + activeProjectiles.Count}");
    }
}

