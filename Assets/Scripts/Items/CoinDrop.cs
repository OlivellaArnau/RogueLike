using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinDrop : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int coinValue = 1;
    [SerializeField] private string coinName = "Moneda";
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private bool floatEffect = true;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatAmplitude = 0.1f;
    [SerializeField] private float lifeTime = 30f;
    private Vector3 startPosition;
    
    private void Start()
    {
        // Guardar la posición inicial para el efecto de flotación
        startPosition = transform.position;
        
        // Destruir la moneda después del tiempo de vida (si es mayor que 0)
        if (lifeTime > 0)
        {
            Destroy(gameObject, lifeTime);
        }
    }
    
    private void Update()
    {
        // Aplicar efecto de flotación
        if (floatEffect)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si el objeto que colisionó es el jugador
        if (collision.CompareTag("Player"))
        {
            // Obtener el inventario del jugador
            PlayerInventory inventory = collision.GetComponent<PlayerInventory>();
            
            if (inventory != null)
            {
                // Añadir las monedas al inventario
                inventory.AddCoins(coinValue);
                
                // Reproducir efectos de recogida
                PlayPickupEffects();
                
                // Mostrar mensaje de recogida
                Debug.Log($"Has recogido: {coinName} x{coinValue}");
                
                // Destruir la moneda
                Destroy(gameObject);
            }
        }
    }
    private void PlayPickupEffects()
    {
        // Reproducir sonido de recogida
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }
    public void SetCoinValue(int value)
    {
        coinValue = value;
    }
}

