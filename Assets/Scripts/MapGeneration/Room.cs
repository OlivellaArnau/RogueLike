using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Componentes de la Sala")]
    [Tooltip("Transform del suelo de la sala")]
    [SerializeField] private Transform floorTransform;
    
    [Tooltip("Transform de la tienda (si existe)")]
    [SerializeField] private Transform shopTransform;
    
    [Tooltip("Transforms de las paredes (arriba, derecha, abajo, izquierda)")]
    [SerializeField] private Transform[] wallTransforms = new Transform[4];
    
    [Tooltip("Transforms de las puertas (arriba, derecha, abajo, izquierda)")]
    [SerializeField] private Transform[] doorTransforms = new Transform[4];
    
    [Tooltip("Transforms de los bloqueadores de puertas (arriba, derecha, abajo, izquierda)")]
    [SerializeField] private Transform[] doorBlockerTransforms = new Transform[4];
    
    [Header("Configuración de Enemigos")]
    [Tooltip("Prefabs de enemigos que pueden aparecer en la sala")]
    [SerializeField] private GameObject[] enemyPrefabs;
    
    [Tooltip("Número mínimo de enemigos a generar")]
    [SerializeField] private int minEnemies = 2;
    
    [Tooltip("Número máximo de enemigos a generar")]
    [SerializeField] private int maxEnemies = 5;
    
    [Tooltip("Puntos de spawn de enemigos")]
    [SerializeField] private Transform[] enemySpawnPoints;
    
    [Header("Configuración de la Tienda")]
    [Tooltip("Transforms de los items de la tienda")]
    [SerializeField] private Transform[] shopItemTransforms;
    
    [Header("Depuración")]
    [Tooltip("Mostrar información de depuración")]
    [SerializeField] private bool debugMode = false;
    
    // Posición en la matriz de la mazmorra
    private Vector2Int gridPosition;
    
    // Tipo de sala
    private RoomType roomType;
    
    // Estado de la sala
    private bool hasBeenVisited = false;
    private bool enemiesDefeated = false;
    private bool doorsBlocked = false;
    
    // Conexiones con otras salas (arriba, derecha, abajo, izquierda)
    private bool[] connections = new bool[4];
    
    // Lista de enemigos activos en la sala
    private List<GameObject> activeEnemies = new List<GameObject>();
    
    // Propiedades públicas
    public Vector2Int GridPosition => gridPosition;
    public RoomType RoomType => roomType;
    public bool HasBeenVisited => hasBeenVisited;
    public bool EnemiesDefeated => enemiesDefeated;
    
    /// <summary>
    /// Inicializa la sala con su posición y tipo
    /// </summary>
    public void Initialize(Vector2Int position, RoomType type)
    {
        gridPosition = position;
        roomType = type;
        
        // Configurar la sala según su tipo
        ConfigureRoomByType();
        
        if (debugMode)
        {
            Debug.Log($"Sala inicializada en {position} de tipo {type}");
        }
    }
    private void ConfigureRoomByType()
    {
        // Activar/desactivar componentes según el tipo de sala
        if (shopTransform != null)
        {
            shopTransform.gameObject.SetActive(roomType == RoomType.Shop);
        }
        
        // Configurar la sala de inicio
        if (roomType == RoomType.Start)
        {
            // La sala de inicio no tiene enemigos
            minEnemies = 0;
            maxEnemies = 0;
        }
        
        // Configurar la sala de tienda
        if (roomType == RoomType.Shop && shopItemTransforms != null)
        {
            // Activar los items de la tienda
            foreach (Transform itemTransform in shopItemTransforms)
            {
                if (itemTransform != null)
                {
                    itemTransform.gameObject.SetActive(true);
                }
            }
        }
    }
    public void SetConnection(int direction, bool connected)
    {
        if (direction >= 0 && direction < 4)
        {
            connections[direction] = connected;
            
            // Actualizar visualmente la sala
            UpdateRoomVisuals();
        }
    }
    private void UpdateRoomVisuals()
    {
        for (int i = 0; i < 4; i++)
        {
            // Activar/desactivar paredes y puertas según las conexiones
            if (wallTransforms[i] != null)
            {
                wallTransforms[i].gameObject.SetActive(!connections[i]);
            }
            
            if (doorTransforms[i] != null)
            {
                doorTransforms[i].gameObject.SetActive(connections[i]);
            }
            
            // Los bloqueadores de puertas se activan cuando el jugador entra en la sala
            if (doorBlockerTransforms[i] != null)
            {
                doorBlockerTransforms[i].gameObject.SetActive(false);
            }
        }
    }
    public void OnPlayerEnter()
    {
        if (!hasBeenVisited)
        {
            hasBeenVisited = true;
            
            // Generar enemigos si no es una sala de tienda y no es la sala inicial
            if (roomType != RoomType.Shop && roomType != RoomType.Start && !enemiesDefeated)
            {
                StartCoroutine(SpawnEnemiesWithDelay());
            }
            
            if (debugMode)
            {
                Debug.Log($"Jugador entró en la sala {gridPosition}");
            }
        }
    }
    private IEnumerator SpawnEnemiesWithDelay()
    {
        // Esperar un momento antes de generar enemigos
        yield return new WaitForSeconds(0.5f);
        
        // Bloquear las puertas
        BlockDoors();
        
        // Generar enemigos
        SpawnEnemies();
    }
    private void SpawnEnemies()
    {
        // Limpiar enemigos anteriores
        ClearEnemies();
        
        // Determinar cuántos enemigos generar
        int enemyCount = Random.Range(minEnemies, maxEnemies + 1);
        
        // Si no hay prefabs de enemigos o puntos de spawn, salir
        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || 
            enemySpawnPoints == null || enemySpawnPoints.Length == 0)
        {
            Debug.LogWarning("No hay prefabs de enemigos o puntos de spawn configurados");
            return;
        }
        
        // Generar enemigos en puntos de spawn aleatorios
        for (int i = 0; i < enemyCount; i++)
        {
            // Seleccionar un prefab de enemigo aleatorio
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            
            // Seleccionar un punto de spawn aleatorio
            Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
            
            // Instanciar el enemigo
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            enemy.transform.SetParent(transform);
            
            // Añadir el enemigo a la lista de enemigos activos
            activeEnemies.Add(enemy);
            
            // Suscribirse al evento de muerte del enemigo
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.Health.OnDeath.AddListener(() => OnEnemyDefeated(enemy));
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"Generados {enemyCount} enemigos en la sala {gridPosition}");
        }
    }
    private void ClearEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        
        activeEnemies.Clear();
    }
    public void OnEnemyDefeated(GameObject enemy)
    {
        // Remover el enemigo de la lista
        activeEnemies.Remove(enemy);
        
        // Comprobar si todos los enemigos han sido derrotados
        if (activeEnemies.Count == 0)
        {
            enemiesDefeated = true;
            
            // Desbloquear las puertas
            UnblockDoors();
            
            if (debugMode)
            {
                Debug.Log($"Todos los enemigos derrotados en la sala {gridPosition}");
            }

            // Si esta es la sala de evento especial, activar el evento
            if (roomType == RoomType.SpecialEvent)
            {
                if (DungeonGenerator.Instance.AreAllRoomsVisited())
                {
                    Debug.Log("Todas las salas visitadas y enemigos derrotados. Generando el siguiente nivel...");
                    DungeonGenerator.Instance.GenerateNextFloor();
                }
            }
        }
    }
    private void BlockDoors()
    {
        doorsBlocked = true;
        
        for (int i = 0; i < 4; i++)
        {
            if (connections[i] && doorBlockerTransforms[i] != null)
            {
                doorBlockerTransforms[i].gameObject.SetActive(true);
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"Puertas bloqueadas en la sala {gridPosition}");
        }
    }

    private void UnblockDoors()
    {
        doorsBlocked = false;
        
        for (int i = 0; i < 4; i++)
        {
            if (doorBlockerTransforms[i] != null)
            {
                doorBlockerTransforms[i].gameObject.SetActive(false);
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"Puertas desbloqueadas en la sala {gridPosition}");
        }
    }
    private void ActivateSpecialEvent()
    {
        if (debugMode)
        {
            Debug.Log($"Evento especial activado en la sala {gridPosition}");
        }
        
        // Aquí se implementaría la lógica del evento especial
        // Por ejemplo, abrir una puerta especial, dar un item al jugador, etc.
    }
    public void SetRoomType(RoomType type)
    {
        roomType = type;
        ConfigureRoomByType();
    }
    public bool HasConnection(int direction)
    {
        if (direction >= 0 && direction < 4)
        {
            return connections[direction];
        }
        return false;
    }
    public bool AreDoorsBlocked()
    {
        return doorsBlocked;
    }
    public Vector3 GetDoorPosition(int direction)
    {
        if (direction >= 0 && direction < 4 && doorTransforms[direction] != null)
        {
            return doorTransforms[direction].position;
        }
        return transform.position;
    }
}

