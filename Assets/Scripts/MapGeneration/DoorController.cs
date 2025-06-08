using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Configuración de la Puerta")]
    [Tooltip("Dirección de la puerta (0: arriba, 1: derecha, 2: abajo, 3: izquierda)")]
    [SerializeField] private int doorDirection;
    
    [Tooltip("Offset de posición para el jugador al entrar por esta puerta")]
    [SerializeField] private Vector2 playerOffset = Vector2.zero;
    
    [Header("Efectos")]
    [Tooltip("Efecto visual al usar la puerta")]
    [SerializeField] private GameObject transitionEffect;
    
    [Tooltip("Sonido al usar la puerta")]
    [SerializeField] private AudioClip doorSound;
    
    [Tooltip("Duración de la transición en segundos")]
    [SerializeField] private float transitionDuration = 0.5f;
    
    // Referencia a la sala a la que pertenece esta puerta
    private Room parentRoom;
    
    // Referencia al controlador de la mazmorra
    private DungeonGenerator dungeonGenerator;
    
    // Dirección opuesta para posicionar al jugador en la sala destino
    private int oppositeDirection;
    
    // Mapa de direcciones opuestas
    private static readonly int[] oppositeDirections = { 2, 3, 0, 1 }; // abajo, izquierda, arriba, derecha
    
    private void Awake()
    {
        // Obtener la referencia a la sala padre
        parentRoom = GetComponentInParent<Room>();
        
        // Obtener la referencia al generador de mazmorras
        dungeonGenerator = DungeonGenerator.Instance;
        
        // Calcular la dirección opuesta
        oppositeDirection = oppositeDirections[doorDirection];
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si el objeto que colisionó es el jugador
        if (collision.CompareTag("Player"))
        {
            // Verificar si la puerta está bloqueada
            if (parentRoom != null && parentRoom.AreDoorsBlocked())
            {
                // Mostrar mensaje o efecto de puerta bloqueada
                ShowBlockedDoorEffect();
                return;
            }
            
            // Iniciar la transición a la siguiente sala
            StartCoroutine(TransitionToNextRoom(collision.gameObject));
        }
    }
    private IEnumerator TransitionToNextRoom(GameObject player)
    {
        // Verificar si tenemos todas las referencias necesarias
        if (parentRoom == null || dungeonGenerator == null)
        {
            Debug.LogError("Faltan referencias necesarias para la transición");
            yield break;
        }
        
        // Obtener la posición de la sala actual en la matriz
        Vector2Int currentRoomPos = parentRoom.GridPosition;
        
        // Calcular la posición de la sala destino
        Vector2Int nextRoomPos = CalculateNextRoomPosition(currentRoomPos);
        
        // Obtener la sala destino
        Room nextRoom = dungeonGenerator.GetRoom(nextRoomPos);
        
        // Verificar si la sala destino existe
        if (nextRoom == null)
        {
            Debug.LogError($"No se encontró una sala en la posición {nextRoomPos}");
            yield break;
        }
        
        // Reproducir sonido de puerta
        if (doorSound != null)
        {
            AudioSource.PlayClipAtPoint(doorSound, transform.position);
        }
        
        // Mostrar efecto de transición
        if (transitionEffect != null)
        {
            GameObject effect = Instantiate(transitionEffect, transform.position, Quaternion.identity);
            Destroy(effect, transitionDuration);
        }
        
        // Desactivar el control del jugador durante la transición
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Esperar la duración de la transición
        yield return new WaitForSeconds(transitionDuration);
        
        // Calcular la posición de destino del jugador
        Vector3 targetPosition = CalculatePlayerTargetPosition(nextRoom);
        
        // Mover al jugador a la nueva sala
        player.transform.position = targetPosition;
        
        // Reactivar el control del jugador
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        // Notificar a la nueva sala que el jugador ha entrado
        nextRoom.OnPlayerEnter();
    }
    private Vector2Int CalculateNextRoomPosition(Vector2Int currentPos)
    {
        Vector2Int offset = Vector2Int.zero;
        
        // Determinar el offset según la dirección de la puerta
        switch (doorDirection)
        {
            case 0: // Arriba
                offset = new Vector2Int(0, 1);
                break;
            case 1: // Derecha
                offset = new Vector2Int(1, 0);
                break;
            case 2: // Abajo
                offset = new Vector2Int(0, -1);
                break;
            case 3: // Izquierda
                offset = new Vector2Int(-1, 0);
                break;
        }
        
        return currentPos + offset;
    }
    private Vector3 CalculatePlayerTargetPosition(Room targetRoom)
    {
        // Obtener la posición de la puerta opuesta en la sala destino
        Vector3 doorPosition = targetRoom.GetDoorPosition(oppositeDirection);
        
        // Aplicar un offset para que el jugador no aparezca exactamente en la puerta
        Vector3 offset = Vector3.zero;
        
        switch (oppositeDirection)
        {
            case 0: // Arriba
                offset = new Vector3(playerOffset.x, -playerOffset.y, 0);
                break;
            case 1: // Derecha
                offset = new Vector3(-playerOffset.x, playerOffset.y, 0);
                break;
            case 2: // Abajo
                offset = new Vector3(playerOffset.x, playerOffset.y, 0);
                break;
            case 3: // Izquierda
                offset = new Vector3(playerOffset.x, playerOffset.y, 0);
                break;
        }
        
        return doorPosition + offset;
    }
    private void ShowBlockedDoorEffect()
    {
        // Aquí se implementaría el efecto visual o sonoro de puerta bloqueada
        Debug.Log("Puerta bloqueada");
    }
}

