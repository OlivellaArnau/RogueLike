using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase responsable de la generación procedural de mazmorras.
/// Crea una matriz de salas y determina las conexiones entre ellas.
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("Configuración de la Mazmorra")]
    [Tooltip("Prefab de la sala que se utilizará para generar la mazmorra")]
    [SerializeField] private GameObject roomPrefab;
    
    [Tooltip("Tamaño de la sala en unidades de mundo")]
    [SerializeField] private Vector2 roomSize = new Vector2(20f, 11f);
    
    [Tooltip("Número mínimo de salas a generar")]
    [SerializeField] private int minRooms = 5;
    
    [Tooltip("Número máximo de salas a generar")]
    [SerializeField] private int maxRooms = 15;
    
    [Tooltip("Semilla para la generación aleatoria (0 para semilla aleatoria)")]
    [SerializeField] private int seed = 0;
    
    [Header("Configuración de Eventos Especiales")]
    [Tooltip("Distancia mínima de la sala inicial para colocar la tienda")]
    [SerializeField] private int minDistanceForShop = 2;
    
    [Tooltip("Distancia mínima de la sala inicial para colocar el evento especial")]
    [SerializeField] private int minDistanceForSpecialEvent = 3;
    
    [Header("Depuración")]
    [Tooltip("Mostrar información de depuración en la consola")]
    [SerializeField] private bool debugMode = false;
    
    // Matriz de salas
    private Room[,] dungeonGrid;
    
    // Dimensiones de la matriz
    private int gridWidth;
    private int gridHeight;
    
    // Posición de la sala inicial
    private Vector2Int startRoomPos;
    
    // Posiciones de salas especiales
    private Vector2Int shopRoomPos;
    private Vector2Int specialEventRoomPos;
    
    // Lista de todas las salas generadas
    private List<Room> allRooms = new List<Room>();
    
    // Sistema de generación de números aleatorios
    private System.Random random;
    
    // Direcciones posibles (arriba, derecha, abajo, izquierda)
    private Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // Arriba
        new Vector2Int(1, 0),   // Derecha
        new Vector2Int(0, -1),  // Abajo
        new Vector2Int(-1, 0)   // Izquierda
    };
    
    // Singleton para acceder desde otros scripts
    public static DungeonGenerator Instance { get; private set; }
    
    private void Awake()
    {
        // Configurar singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Inicializar el generador de números aleatorios
        if (seed == 0)
        {
            seed = System.Environment.TickCount;
        }
        random = new System.Random(seed);
        
        // Inicializar dimensiones de la matriz
        gridWidth = maxRooms * 2 - 1;  // Asegurar espacio suficiente para la expansión
        gridHeight = maxRooms * 2 - 1;
        
        // Inicializar la matriz de salas
        dungeonGrid = new Room[gridWidth, gridHeight];
        
        // Establecer la posición de la sala inicial en el centro de la matriz
        startRoomPos = new Vector2Int(gridWidth / 2, gridHeight / 2);
    }
    
    private void Start()
    {
        // Generar la mazmorra
        GenerateDungeon();
    }
    public void GenerateDungeon()
    {
        if (debugMode)
        {
            Debug.Log($"Generando mazmorra con semilla: {seed}");
        }
        
        // Limpiar cualquier generación anterior
        ClearDungeon();
        
        // Generar la matriz de salas
        GenerateRooms();
        
        // Seleccionar salas especiales
        SelectSpecialRooms();
        
        // Configurar las conexiones entre salas
        SetupRoomConnections();
        
        if (debugMode)
        {
            Debug.Log($"Mazmorra generada con {allRooms.Count} salas");
            Debug.Log($"Sala inicial en: {startRoomPos}");
            Debug.Log($"Sala de tienda en: {shopRoomPos}");
            Debug.Log($"Sala de evento especial en: {specialEventRoomPos}");
        }
    }
    private void ClearDungeon()
    {
        // Destruir todas las salas existentes
        foreach (Room room in allRooms)
        {
            if (room != null)
            {
                Destroy(room.gameObject);
            }
        }
        
        // Limpiar las listas y la matriz
        allRooms.Clear();
        dungeonGrid = new Room[gridWidth, gridHeight];
    }
    private void GenerateRooms()
    {
        // Crear la sala inicial
        CreateRoom(startRoomPos, RoomType.Start);
        
        // Lista de posiciones candidatas para expandir
        List<Vector2Int> candidates = new List<Vector2Int>();
        
        // Añadir las posiciones adyacentes a la sala inicial como candidatas
        foreach (Vector2Int dir in directions)
        {
            candidates.Add(startRoomPos + dir);
        }
        
        // Determinar cuántas salas generar
        int roomsToGenerate = random.Next(minRooms, maxRooms + 1);
        
        // Generar salas hasta alcanzar el número deseado o quedarse sin candidatos
        while (allRooms.Count < roomsToGenerate && candidates.Count > 0)
        {
            // Seleccionar un candidato aleatorio
            int index = random.Next(candidates.Count);
            Vector2Int pos = candidates[index];
            candidates.RemoveAt(index);
            
            // Verificar si la posición está dentro de los límites y no hay una sala ya
            if (IsPositionValid(pos) && GetRoom(pos) == null)
            {
                // Verificar si hay al menos una sala adyacente
                if (HasAdjacentRoom(pos))
                {
                    // Crear una nueva sala
                    CreateRoom(pos, RoomType.Normal);
                    
                    // Añadir las posiciones adyacentes como candidatas
                    foreach (Vector2Int dir in directions)
                    {
                        Vector2Int newPos = pos + dir;
                        if (IsPositionValid(newPos) && GetRoom(newPos) == null && !candidates.Contains(newPos))
                        {
                            candidates.Add(newPos);
                        }
                    }
                }
            }
        }
        
        // Si no se generaron suficientes salas, forzar la creación de más
        if (allRooms.Count < minRooms)
        {
            ForceGenerateMoreRooms(minRooms - allRooms.Count);
        }
    }
    private void ForceGenerateMoreRooms(int count)
    {
        // Crear una lista de todas las posiciones posibles adyacentes a salas existentes
        List<Vector2Int> possiblePositions = new List<Vector2Int>();
        
        foreach (Room room in allRooms)
        {
            Vector2Int roomPos = room.GridPosition;
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int newPos = roomPos + dir;
                if (IsPositionValid(newPos) && GetRoom(newPos) == null && !possiblePositions.Contains(newPos))
                {
                    possiblePositions.Add(newPos);
                }
            }
        }
        
        // Generar salas adicionales
        int roomsCreated = 0;
        while (roomsCreated < count && possiblePositions.Count > 0)
        {
            int index = random.Next(possiblePositions.Count);
            Vector2Int pos = possiblePositions[index];
            possiblePositions.RemoveAt(index);
            
            CreateRoom(pos, RoomType.Normal);
            roomsCreated++;
        }
    }
    private void SelectSpecialRooms()
    {
        // Crear listas de salas candidatas para tienda y evento especial
        List<Room> shopCandidates = new List<Room>();
        List<Room> specialEventCandidates = new List<Room>();
        
        foreach (Room room in allRooms)
        {
            // Ignorar la sala inicial
            if (room.GridPosition == startRoomPos)
                continue;
            
            // Calcular la distancia Manhattan desde la sala inicial
            int distance = Mathf.Abs(room.GridPosition.x - startRoomPos.x) + 
                          Mathf.Abs(room.GridPosition.y - startRoomPos.y);
            
            // Añadir a candidatos para tienda si cumple la distancia mínima
            if (distance >= minDistanceForShop)
            {
                shopCandidates.Add(room);
            }
            
            // Añadir a candidatos para evento especial si cumple la distancia mínima
            if (distance >= minDistanceForSpecialEvent)
            {
                specialEventCandidates.Add(room);
            }
        }
        
        // Seleccionar sala de tienda
        if (shopCandidates.Count > 0)
        {
            Room shopRoom = shopCandidates[random.Next(shopCandidates.Count)];
            shopRoom.SetRoomType(RoomType.Shop);
            shopRoomPos = shopRoom.GridPosition;
            
            // Remover la sala de tienda de los candidatos para evento especial
            specialEventCandidates.Remove(shopRoom);
        }
        else if (debugMode)
        {
            Debug.LogWarning("No se encontraron candidatos válidos para la sala de tienda");
        }
        
        // Seleccionar sala para evento especial
        if (specialEventCandidates.Count > 0)
        {
            Room specialEventRoom = specialEventCandidates[random.Next(specialEventCandidates.Count)];
            specialEventRoom.SetRoomType(RoomType.SpecialEvent);
            specialEventRoomPos = specialEventRoom.GridPosition;
        }
        else if (debugMode)
        {
            Debug.LogWarning("No se encontraron candidatos válidos para la sala de evento especial");
        }
    }
    private void SetupRoomConnections()
    {
        foreach (Room room in allRooms)
        {
            // Para cada dirección, verificar si hay una sala adyacente
            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int neighborPos = room.GridPosition + directions[i];
                Room neighborRoom = GetRoom(neighborPos);
                
                // Si hay una sala adyacente, crear una conexión
                if (neighborRoom != null)
                {
                    room.SetConnection(i, true);
                }
                else
                {
                    room.SetConnection(i, false);
                }
            }
        }
    }
    private Room CreateRoom(Vector2Int gridPos, RoomType type)
    {
        // Calcular la posición en el mundo
        Vector3 worldPos = new Vector3(
            gridPos.x * roomSize.x,
            gridPos.y * roomSize.y,
            0
        );
        
        // Instanciar el prefab de la sala
        GameObject roomObj = Instantiate(roomPrefab, worldPos, Quaternion.identity);
        roomObj.name = $"Room_{gridPos.x}_{gridPos.y}";
        
        // Añadir el componente Room si no existe
        Room room = roomObj.GetComponent<Room>();
        if (room == null)
        {
            room = roomObj.AddComponent<Room>();
        }
        
        // Configurar la sala
        room.Initialize(gridPos, type);
        
        // Añadir la sala a la matriz y a la lista
        dungeonGrid[gridPos.x, gridPos.y] = room;
        allRooms.Add(room);
        
        return room;
    }
    private bool IsPositionValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
    }
    private bool HasAdjacentRoom(Vector2Int pos)
    {
        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = pos + dir;
            if (IsPositionValid(neighborPos) && GetRoom(neighborPos) != null)
            {
                return true;
            }
        }
        return false;
    }
    public Room GetRoom(Vector2Int pos)
    {
        if (IsPositionValid(pos))
        {
            return dungeonGrid[pos.x, pos.y];
        }
        return null;
    }
    public Room GetStartRoom()
    {
        return GetRoom(startRoomPos);
    }
    public Room GetShopRoom()
    {
        return GetRoom(shopRoomPos);
    }
    public Room GetSpecialEventRoom()
    {
        return GetRoom(specialEventRoomPos);
    }
    public List<Room> GetAllRooms()
    {
        return allRooms;
    }
    public bool AreAllRoomsVisited()
    {
        foreach (Room room in allRooms)
        {
            if (!room.HasBeenVisited)
            {
                return false;
            }
        }
        return true;
    }
    public void RegenerateDungeon()
    {
        seed = System.Environment.TickCount;
        random = new System.Random(seed);
        GenerateDungeon();
    }
    public void RegenerateDungeon(int newSeed)
    {
        seed = newSeed;
        random = new System.Random(seed);
        GenerateDungeon();
    }
}
public enum RoomType
{
    Start,
    Normal,
    Shop,
    SpecialEvent
}

