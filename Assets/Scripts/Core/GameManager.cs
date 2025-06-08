using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class GameEvent : UnityEvent { }
    public Camera mainCamera;
    [Header("Eventos")]
    [Tooltip("Evento que se dispara cuando se inicia un nuevo nivel")]
    public GameEvent onLevelStart = new GameEvent();

    [Tooltip("Evento que se dispara cuando se completa un nivel")]
    public GameEvent onLevelComplete = new GameEvent();

    [Tooltip("Evento que se dispara cuando se completa un nivel")]
    public GameEvent onGamePaused = new GameEvent();

    [Tooltip("Evento que se dispara cuando el jugador muere")]
    public GameEvent onPlayerDeath = new GameEvent();
    
    [Header("Configuración")]
    [Tooltip("Prefab del jugador")]
    [SerializeField] private GameObject playerPrefab;

    [Tooltip("Tiempo de espera antes de enviar al main menu despues de morir")]
    [SerializeField] private float restartDelay = 2f;

    [Tooltip("¿Generar un nuevo nivel al iniciar?")]
    [SerializeField] private bool generateOnStart = true;
    public bool IsPaused = false;

    // Singleton para acceder desde otros scripts
    public static GameManager Instance { get; private set; }

    // Referencia al jugador
    private GameObject player;

    // Referencia al generador de mazmorras
    private DungeonGenerator dungeonGenerator;

    // Estado del juego
    private bool isGameOver = false;
    private int currentLevel = 1;

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
        // Configurar la cámara principal
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No se encontró una cámara principal en la escena");
            }
        }

        // Obtener referencia al generador de mazmorras
        dungeonGenerator = FindObjectOfType<DungeonGenerator>();
    }

    private void Start()
    {
        // Generar el nivel inicial si está configurado
        if (generateOnStart)
        {
            StartNewLevel();
        }
    }
    public void StartNewLevel()
    {
        // Reiniciar el estado del juego
        isGameOver = false;

        // Generar una nueva mazmorra
        if (dungeonGenerator != null)
        {
            dungeonGenerator.RegenerateDungeon();
        }
        else
        {
            Debug.LogError("No se encontró un DungeonGenerator en la escena");
            return;
        }

        // Crear el jugador si no existe
        if (player == null)
        {
            Debug.Log("Instanciando jugador en el inicio del nivel");
            SpawnPlayer();
        }
        else
        {
            Debug.Log("Jugador ya existe, reposicionando en la sala inicial");
            Room startRoom = dungeonGenerator.GetStartRoom();
            if (startRoom != null)
            {
                player.transform.position = startRoom.transform.position;
                mainCamera.transform.SetParent(player.transform);
                mainCamera.transform.localPosition = Vector3.zero;

                startRoom.OnPlayerEnter();
            }
        }

        // Disparar evento de inicio de nivel
        onLevelStart?.Invoke();

        Debug.Log($"Nivel {currentLevel} iniciado");
    }
    private void SpawnPlayer()
    {
        if (playerPrefab != null && dungeonGenerator != null)
        {
            Room startRoom = dungeonGenerator.GetStartRoom();
            Debug.Log(GetPlayer() + " " + startRoom + " " + playerPrefab + " " + dungeonGenerator);
            if (startRoom != null)
            {
                Debug.Log("Instanciando jugador en la sala inicial");
                // Instanciar el jugador en la sala inicial
                player = Instantiate(playerPrefab, startRoom.transform.position, Quaternion.identity);

                // Notificar a la sala que el jugador ha entrado
                startRoom.OnPlayerEnter();
            }
            else
            {
                Debug.LogError("No se encontró una sala inicial");
            }
        }
        else
        {
            Debug.LogError("No se ha asignado un prefab de jugador o no se encontró un DungeonGenerator");
        }
    }
    public void CompleteLevel()
    {
        if (!isGameOver)
        {
            // Disparar evento de nivel completado
            onLevelComplete?.Invoke();

            Debug.Log($"Nivel {currentLevel} completado");

            // Incrementar el nivel
            currentLevel++;

            // Iniciar el siguiente nivel después de un retraso
            StartCoroutine(StartNextLevelAfterDelay(1f));
        }
    }
    private IEnumerator StartNextLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNewLevel();
    }
    public void OnPlayerDeath()
    {
        if (!isGameOver)
        {
            isGameOver = true;

            // Disparar evento de muerte del jugador
            onPlayerDeath?.Invoke();

            Debug.Log("Jugador ha muerto");

            // Reiniciar el juego después de un retraso
            ReturnToMainMenu();
        }
    }  
    
   public void PauseGame()
    {
        Time.timeScale = 0f;
        IsPaused = true;
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        IsPaused = false;
    }
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    public GameObject GetPlayer()
    {
        return player;
    }
    public bool IsGameOver()
    {
        return isGameOver;
    }
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        StartCoroutine(RestartGameAfterDelay());
    }
    private IEnumerator RestartGameAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);

        // Reiniciar el nivel actual
        currentLevel = 1;
        StartNewLevel();
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
