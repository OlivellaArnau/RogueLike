using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Configuració")]
    public GameObject backgroundTilePrefab;
    public int initialGridSizeX = 5;
    public int initialGridSizeY = 5;
    public float tileSize = 25.6f;
    public Vector2 scrollSpeed = new Vector2(1f, -1f);

    [Header("Visió i càmera")]
    public Transform cameraTransform;
    public float bufferTiles = 2f;
    private Vector3 lastCameraPosition;

    private HashSet<Vector2Int> spawnedTiles = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, GameObject> activeTiles = new Dictionary<Vector2Int, GameObject>();
    private Queue<GameObject> tilePool = new Queue<GameObject>();

    private void Start()
    {
        if (backgroundTilePrefab == null)
        {
            Debug.LogError(" No s'ha assignat el prefab del tile de fons a BackgroundScroller.");
            enabled = false;
            return;
        }

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        for (int x = -initialGridSizeX / 2; x <= initialGridSizeX / 2; x++)
        {
            for (int y = -initialGridSizeY / 2; y <= initialGridSizeY / 2; y++)
            {
                SpawnTile(new Vector2Int(x, y));
            }
        }
        lastCameraPosition = cameraTransform.position;
    }

    private void Update()
    {
        if (Vector3.Distance(lastCameraPosition, cameraTransform.position) > tileSize * 2f)
        {
            RecenterTiles();
        }
        MoveAllTiles();
        CheckAndSpawnAroundCamera();
        lastCameraPosition = cameraTransform.position;
        RecycleDistantTiles();
    }

    private void MoveAllTiles()
    {
        Vector3 delta = (Vector3)(scrollSpeed * Time.deltaTime);

        foreach (var tile in activeTiles.Values)
        {
            tile.transform.position += delta;
        }
    }

    private void CheckAndSpawnAroundCamera()
    {
        Vector2 camPos = cameraTransform.position;
        float camWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float camHeight = Camera.main.orthographicSize;

        int minX = Mathf.FloorToInt((camPos.x - camWidth) / tileSize) - (int)bufferTiles;
        int maxX = Mathf.CeilToInt((camPos.x + camWidth) / tileSize) + (int)bufferTiles;
        int minY = Mathf.FloorToInt((camPos.y - camHeight) / tileSize) - (int)bufferTiles;
        int maxY = Mathf.CeilToInt((camPos.y + camHeight) / tileSize) + (int)bufferTiles;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                if (!spawnedTiles.Contains(coord))
                {
                    SpawnTile(coord);
                }
            }
        }
    }

    private void SpawnTile(Vector2Int gridCoord)
    {
        Vector3 worldPos = new Vector3(gridCoord.x * tileSize, gridCoord.y * tileSize, 10f);
        GameObject newTile = GetTileFromPool();
        newTile.transform.position = worldPos;
        newTile.transform.SetParent(transform);
        newTile.SetActive(true);

        spawnedTiles.Add(gridCoord);
        activeTiles.Add(gridCoord, newTile);
    }

    private GameObject GetTileFromPool()
    {
        if (tilePool.Count > 0)
        {
            return tilePool.Dequeue();
        }
        else
        {
            return Instantiate(backgroundTilePrefab);
        }
    }
    private void RecenterTiles()
    {
        foreach (var tile in activeTiles.Values)
        {
            tile.SetActive(false);
            tilePool.Enqueue(tile);
        }

        spawnedTiles.Clear();
        activeTiles.Clear();

        Vector2 camPos = cameraTransform.position;
        int halfX = initialGridSizeX / 2;
        int halfY = initialGridSizeY / 2;

        for (int x = -halfX; x <= halfX; x++)
        {
            for (int y = -halfY; y <= halfY; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                SpawnTile(coord + WorldToGrid(camPos));
            }
        }
    }

    private Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / tileSize),
            Mathf.RoundToInt(worldPos.y / tileSize)
        );
    }
    private void RecycleDistantTiles()
    {
        Vector2 camPos = cameraTransform.position;
        float camWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float camHeight = Camera.main.orthographicSize;

        List<Vector2Int> toRemove = new List<Vector2Int>();

        foreach (var kvp in activeTiles)
        {
            Vector3 tilePos = kvp.Value.transform.position;
            if (Mathf.Abs(tilePos.x - camPos.x) > camWidth + tileSize * bufferTiles * 2 ||
                Mathf.Abs(tilePos.y - camPos.y) > camHeight + tileSize * bufferTiles * 2)
            {
                kvp.Value.SetActive(false);
                tilePool.Enqueue(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
        {
            activeTiles.Remove(key);
            spawnedTiles.Remove(key);
        }
    }


}
