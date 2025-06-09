using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;
using System.Collections;
using Unity.VisualScripting;

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
    }

    private IEnumerator Start()
    {
        // Esperar un frame para asegurar que el DungeonGenerator se haya inicializado
        yield return null;

        // Generar el nivel inicial si está configurado
        if (generateOnStart)
        {
            StartNewLevel();
            generateOnStart = false; // Evitar regenerar en cada inicio
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

            if (startRoom != null)
            {
                Debug.Log("Instanciando jugador en la sala inicial");
                // Instanciar el jugador en la sala inicial
                player = Instantiate(playerPrefab, startRoom.transform.position, Quaternion.identity);

                // Configurar la cámara
                SetupCamera(player);

                // Notificar a la sala que el jugador ha entrado
                startRoom.OnPlayerEnter();

                // Asignar la sala actual al jugador
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.SetCurrentRoom(startRoom);
                    playerController.OnPlayerDeath += OnPlayerDeath;
                }
            }
        }
    }

    private void SetupCamera(GameObject player)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No se encontró una cámara principal en la escena");
                return;
            }
        }
        // Asegurar que la posición Z se mantenga (importante para cámaras 2D)
        float originalZ = mainCamera.transform.position.z;
        // Configurar la posición de la cámara para que siga al jugador

        mainCamera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, originalZ);

        // Configurar el seguimiento suave si es necesario
        CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
        if (cameraFollow == null)
        {
            cameraFollow = mainCamera.AddComponent<CameraFollow>();
        }
        cameraFollow.SetTarget(player.transform);
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
            onPlayerDeath?.Invoke();
            Debug.Log("Jugador ha muerto");

            if (player != null)
                player.GetComponent<PlayerController>()?.DisableInput();

            // En lugar de mostrar un menú, volvemos al MainMenu tras un retraso opcional
            StartCoroutine(LoadMainMenuAfterDelay(restartDelay));
            PauseGame(); // Si quieres que se congele el juego antes del cambio
        }
    }
    private IEnumerator LoadMainMenuAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Usa tiempo real si el juego está pausado
        Time.timeScale = 1f; // Asegura que el tiempo vuelva a la normalidad
        SceneManager.LoadScene("MainMenu"); // Asegúrate de que esta escena existe y está incluida en Build Settings
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
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        dungeonGenerator = UnityEngine.Object.FindFirstObjectByType<DungeonGenerator>();
        if (dungeonGenerator != null)
        {
            generateOnStart = true;
            StartCoroutine(DelayedStartNewLevel());
        }
    }

    private IEnumerator DelayedStartNewLevel()
    {
        yield return null; // esperar un frame
        StartNewLevel();
    }

}
