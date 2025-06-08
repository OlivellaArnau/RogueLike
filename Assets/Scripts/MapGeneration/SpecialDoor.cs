using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clase que gestiona una puerta especial que se abre cuando todas las salas han sido visitadas.
/// </summary>
public class SpecialDoor : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Mensaje que se muestra cuando la puerta está cerrada")]
    [SerializeField] private string lockedMessage = "Esta puerta se abrirá cuando hayas explorado todas las salas";
    
    [Tooltip("Mensaje que se muestra cuando la puerta se desbloquea")]
    [SerializeField] private string unlockedMessage = "La puerta se ha desbloqueado";
    
    [Header("Referencias")]
    [Tooltip("Sprite de la puerta cerrada")]
    [SerializeField] private Sprite lockedSprite;
    
    [Tooltip("Sprite de la puerta abierta")]
    [SerializeField] private Sprite unlockedSprite;
    
    [Tooltip("SpriteRenderer de la puerta")]
    [SerializeField] private SpriteRenderer doorRenderer;
    
    [Tooltip("Collider de la puerta")]
    [SerializeField] private Collider2D doorCollider;
    
    [Header("Efectos")]
    [Tooltip("Efecto visual cuando la puerta se desbloquea")]
    [SerializeField] private GameObject unlockEffect;
    
    [Tooltip("Sonido cuando la puerta se desbloquea")]
    [SerializeField] private AudioClip unlockSound;
    
    [Tooltip("Tiempo de espera antes de cambiar el sprite")]
    [SerializeField] private float unlockDelay = 1f;
    
    // Estado de la puerta
    private bool isUnlocked = false;
    
    // Referencia al generador de mazmorras
    private DungeonGenerator dungeonGenerator;
    
    private void Start()
    {
        // Obtener referencia al generador de mazmorras
        dungeonGenerator = DungeonGenerator.Instance;
        
        // Configurar el estado inicial de la puerta
        SetDoorState(false);
        
        // Iniciar la comprobación periódica del estado de las salas
        StartCoroutine(CheckRoomsVisited());
    }
    
    /// <summary>
    /// Comprueba periódicamente si todas las salas han sido visitadas
    /// </summary>
    private IEnumerator CheckRoomsVisited()
    {
        while (!isUnlocked)
        {
            // Esperar un tiempo antes de la siguiente comprobación
            yield return new WaitForSeconds(1f);
            
            // Comprobar si todas las salas han sido visitadas
            if (dungeonGenerator != null && dungeonGenerator.AreAllRoomsVisited())
            {
                // Desbloquear la puerta
                UnlockDoor();
                break;
            }
        }
    }
    
    /// <summary>
    /// Desbloquea la puerta
    /// </summary>
    public void UnlockDoor()
    {
        if (!isUnlocked)
        {
            isUnlocked = true;
            
            // Mostrar mensaje de desbloqueo
            Debug.Log(unlockedMessage);
            
            // Reproducir efectos de desbloqueo
            PlayUnlockEffects();
            
            // Cambiar el estado de la puerta después de un retraso
            StartCoroutine(DelayedDoorStateChange());
        }
    }
    
    /// <summary>
    /// Cambia el estado de la puerta después de un retraso
    /// </summary>
    private IEnumerator DelayedDoorStateChange()
    {
        yield return new WaitForSeconds(unlockDelay);
        SetDoorState(true);
    }
    
    /// <summary>
    /// Establece el estado visual y funcional de la puerta
    /// </summary>
    private void SetDoorState(bool unlocked)
    {
        // Cambiar el sprite
        if (doorRenderer != null)
        {
            doorRenderer.sprite = unlocked ? unlockedSprite : lockedSprite;
        }
        
        // Activar/desactivar el collider
        if (doorCollider != null)
        {
            doorCollider.isTrigger = unlocked;
        }
    }
    
    /// <summary>
    /// Reproduce los efectos de desbloqueo
    /// </summary>
    private void PlayUnlockEffects()
    {
        // Reproducir sonido de desbloqueo
        if (unlockSound != null)
        {
            AudioSource.PlayClipAtPoint(unlockSound, transform.position);
        }
        
        // Mostrar efecto visual de desbloqueo
        if (unlockEffect != null)
        {
            Instantiate(unlockEffect, transform.position, Quaternion.identity);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si el objeto que colisionó es el jugador
        if (collision.CompareTag("Player") && isUnlocked)
        {
            // Aquí se implementaría la lógica para pasar al siguiente nivel o completar el juego
            Debug.Log("¡Has completado el nivel!");
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si el objeto que colisionó es el jugador
        if (collision.gameObject.CompareTag("Player") && !isUnlocked)
        {
            // Mostrar mensaje de puerta cerrada
            Debug.Log(lockedMessage);
        }
    }
}

