using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class InventoryChangeEvent : UnityEvent<int> { }
    
    [Header("Eventos")]
    [Tooltip("Evento que se dispara cuando cambia la cantidad de monedas")]
    public InventoryChangeEvent onCoinsChanged = new InventoryChangeEvent();
    
    [Header("Configuración Inicial")]
    [Tooltip("Cantidad inicial de monedas")]
    [SerializeField] private int initialCoins = 0;
    
    // Cantidad actual de monedas
    private int coins;
    
    private void Start()
    {
        // Inicializar monedas
        coins = initialCoins;
        
        // Notificar el valor inicial
        onCoinsChanged?.Invoke(coins);
    }
    public void AddCoins(int amount)
    {
        if (amount > 0)
        {
            coins += amount;
            onCoinsChanged?.Invoke(coins);
            
            Debug.Log($"Añadidas {amount} monedas. Total: {coins}");
        }
    }
    public bool SpendCoins(int amount)
    {
        if (amount <= 0)
            return true;
            
        if (coins >= amount)
        {
            coins -= amount;
            onCoinsChanged?.Invoke(coins);
            
            Debug.Log($"Gastadas {amount} monedas. Restantes: {coins}");
            return true;
        }
        
        Debug.Log($"No hay suficientes monedas. Necesitas: {amount}, Tienes: {coins}");
        return false;
    }
    public int GetCoins()
    {
        return coins;
    }
}

