using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// </summary>
public class ShopItem : MonoBehaviour
{
    [Header("Configuración del Item")]
    [Tooltip("Nombre del item")]
    [SerializeField] private string itemName = "Item";
    
    [Tooltip("Descripción del item")]
    [SerializeField] private string itemDescription = "Un item de la tienda";
    
    [Tooltip("Costo del item en monedas")]
    [SerializeField] private int cost = 10;
    
    [Tooltip("Sprite del item")]
    [SerializeField] private SpriteRenderer itemSprite;
    
    [Header("Efectos")]
    [Tooltip("Efecto visual al comprar el item")]
    [SerializeField] private GameObject purchaseEffect;
    
    [Tooltip("Sonido al comprar el item")]
    [SerializeField] private AudioClip purchaseSound;
    
    [Tooltip("Evento que se dispara cuando se compra el item")]
    [SerializeField] private UnityEvent onPurchase;
    
    [Header("UI")]
    [Tooltip("Texto que muestra el costo del item")]
    [SerializeField] private TMPro.TextMeshPro costText;
    
    [Tooltip("Texto que muestra el nombre del item")]
    [SerializeField] private TMPro.TextMeshPro nameText;
    
    // Indica si el item ya ha sido comprado
    private bool isPurchased = false;
    
    private void Start()
    {
        // Actualizar textos UI
        UpdateUI();
    }
    private void UpdateUI()
    {
        if (costText != null)
        {
            costText.text = cost.ToString();
        }
        
        if (nameText != null)
        {
            nameText.text = itemName;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si el objeto que colisionó es el jugador
        if (collision.CompareTag("Player") && !isPurchased)
        {
            // Mostrar información del item
            ShowItemInfo();
            
            // Comprobar si el jugador presiona la tecla de interacción
            StartCoroutine(CheckForInteraction(collision.gameObject));
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        // Verificar si el objeto que salió es el jugador
        if (collision.CompareTag("Player"))
        {
            // Ocultar información del item
            HideItemInfo();
            
            // Detener la comprobación de interacción
            StopAllCoroutines();
        }
    }
    private IEnumerator CheckForInteraction(GameObject player)
    {
        while (true)
        {
            // Comprobar si se presiona la tecla de interacción (E)
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Intentar comprar el item
                TryPurchase(player);
            }
            
            yield return null;
        }
    }
    private void TryPurchase(GameObject player)
    {
        // Obtener el controlador del jugador
        PlayerController playerController = player.GetComponent<PlayerController>();
        
        if (playerController != null)
        {
            // Intentar comprar el item
            bool success = playerController.BuyItem(cost, itemName);
            
            if (success)
            {
                // Marcar como comprado
                isPurchased = true;
                
                // Reproducir efectos
                PlayPurchaseEffects();
                
                // Invocar evento de compra
                onPurchase?.Invoke();
                
                // Desactivar el item
                gameObject.SetActive(false);
            }
            else
            {
                // Mostrar mensaje de error (no suficientes monedas)
                ShowNotEnoughCoinsMessage();
            }
        }
    }
    private void ShowItemInfo()
    {
        // Aquí se implementaría la lógica para mostrar información detallada del item
        Debug.Log($"Item: {itemName} - {itemDescription} - Costo: {cost}");
    }
    private void HideItemInfo()
    {
        // Aquí se implementaría la lógica para ocultar la información del item
    }
    private void ShowNotEnoughCoinsMessage()
    {
        Debug.Log("No tienes suficientes monedas para comprar este item");
    }
    private void PlayPurchaseEffects()
    {
        // Reproducir sonido de compra
        if (purchaseSound != null)
        {
            AudioSource.PlayClipAtPoint(purchaseSound, transform.position);
        }
        
        // Mostrar efecto visual de compra
        if (purchaseEffect != null)
        {
            Instantiate(purchaseEffect, transform.position, Quaternion.identity);
        }
    }
}

