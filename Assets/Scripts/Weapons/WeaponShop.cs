using UnityEngine;

/// <summary>
/// Sistema simple de tienda para comprar armas.
/// Se integra con WeaponInventory y PlayerInventory existentes.
/// </summary>
public class WeaponShop : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private WeaponInventory weaponInventory;
    [SerializeField] private PlayerInventory playerInventory;
    
    [Header("UI (Opcional)")]
    [SerializeField] private GameObject shopUI;
    [SerializeField] private bool showDebugMessages = true;
    
    // Estado
    private bool shopOpen = false;
    
    private void Awake()
    {
        // Obtener componentes automáticamente si no están asignados
        if (weaponInventory == null)
        {
            weaponInventory = FindObjectOfType<WeaponInventory>();
        }
        
        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<PlayerInventory>();
        }
    }
    
    private void Start()
    {
        // Cerrar tienda al inicio
        if (shopUI != null)
        {
            shopUI.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Input para abrir/cerrar tienda (E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleShop();
        }
        
        // Input para comprar armas directamente (para testing)
        if (showDebugMessages)
        {
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                TryPurchaseWeapon(0); // Sniper
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                TryPurchaseWeapon(1); // Flamethrower
            }
        }
    }
    
    /// <summary>
    /// Abre o cierra la tienda.
    /// </summary>
    public void ToggleShop()
    {
        shopOpen = !shopOpen;
        
        if (shopUI != null)
        {
            shopUI.SetActive(shopOpen);
        }
        
        if (showDebugMessages)
        {
            Debug.Log($"WeaponShop: Tienda {(shopOpen ? "abierta" : "cerrada")}");
            
            if (shopOpen)
            {
                ShowAvailableWeapons();
            }
        }
    }
    
    /// <summary>
    /// Muestra las armas disponibles en la consola.
    /// </summary>
    private void ShowAvailableWeapons()
    {
        if (weaponInventory == null)
        {
            Debug.Log("WeaponShop: WeaponInventory no disponible");
            return;
        }
        
        Debug.Log("=== TIENDA DE ARMAS ===");
        
        WeaponBase[] weapons = weaponInventory.GetAllWeapons();
        bool[] unlockStatus = weaponInventory.GetUnlockStatus();
        
        for (int i = 0; i < weapons.Length; i++)
        {
            WeaponBase weapon = weapons[i];
            if (weapon != null)
            {
                string status = unlockStatus[i] ? "DESBLOQUEADO" : $"PRECIO: {weapon.ShopPrice} monedas";
                Debug.Log($"{i + 1}. {weapon.WeaponName} - {status}");
                Debug.Log($"   Descripción: {weapon.Description}");
                Debug.Log($"   Daño: {weapon.Damage} | Alcance: {weapon.Range} | Cooldown: {weapon.Cooldown}s");
            }
        }
        
        Debug.Log($"Monedas actuales: {GetCurrentCoins()}");
        Debug.Log("Presiona 7 para comprar Sniper, 8 para comprar Flamethrower");
    }
    
    /// <summary>
    /// Intenta comprar un arma específica.
    /// </summary>
    public bool TryPurchaseWeapon(int weaponIndex)
    {
        if (weaponInventory == null)
        {
            if (showDebugMessages) Debug.LogError("WeaponShop: WeaponInventory no disponible");
            return false;
        }
        
        WeaponBase weapon = weaponInventory.GetWeaponByIndex(weaponIndex);
        if (weapon == null)
        {
            if (showDebugMessages) Debug.Log($"WeaponShop: No hay arma en el índice {weaponIndex}");
            return false;
        }
        
        // Verificar si ya está desbloqueado
        if (weaponInventory.IsWeaponUnlocked(weaponIndex))
        {
            if (showDebugMessages) Debug.Log($"WeaponShop: {weapon.WeaponName} ya está desbloqueado");
            return false;
        }
        
        // Verificar monedas
        int currentCoins = GetCurrentCoins();
        if (currentCoins < weapon.ShopPrice)
        {
            if (showDebugMessages) 
            {
                Debug.Log($"WeaponShop: No hay suficientes monedas para {weapon.WeaponName}");
                Debug.Log($"Necesitas: {weapon.ShopPrice}, Tienes: {currentCoins}");
            }
            return false;
        }
        
        // Realizar compra
        bool purchaseSuccess = weaponInventory.UnlockWeapon(weaponIndex, true);
        
        if (purchaseSuccess)
        {
            if (showDebugMessages)
            {
                Debug.Log($"WeaponShop: ¡{weapon.WeaponName} comprado exitosamente!");
                Debug.Log($"Monedas restantes: {GetCurrentCoins()}");
            }
            
            // Equipar automáticamente el arma comprada
            weaponInventory.EquipRangedWeapon(weaponIndex);
            
            return true;
        }
        else
        {
            if (showDebugMessages) Debug.Log($"WeaponShop: Error al comprar {weapon.WeaponName}");
            return false;
        }
    }
    
    /// <summary>
    /// Obtiene las monedas actuales del jugador.
    /// </summary>
    private int GetCurrentCoins()
    {
        if (playerInventory == null) return 0;
        
        try
        {
            // Intentar obtener monedas usando reflexión
            var coinsField = playerInventory.GetType().GetField("coins", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (coinsField != null)
            {
                return (int)coinsField.GetValue(playerInventory);
            }
            
            // Intentar con propiedad
            var coinsProperty = playerInventory.GetType().GetProperty("Coins", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            if (coinsProperty != null)
            {
                return (int)coinsProperty.GetValue(playerInventory);
            }
        }
        catch (System.Exception e)
        {
            if (showDebugMessages) Debug.LogWarning($"WeaponShop: Error al obtener monedas: {e.Message}");
        }
        
        return 999; // Fallback para testing
    }
    
    /// <summary>
    /// Compra un arma por nombre.
    /// </summary>
    public bool PurchaseWeaponByName(string weaponName)
    {
        if (weaponInventory == null) return false;
        
        WeaponBase[] weapons = weaponInventory.GetAllWeapons();
        
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && weapons[i].WeaponName.Equals(weaponName, System.StringComparison.OrdinalIgnoreCase))
            {
                return TryPurchaseWeapon(i);
            }
        }
        
        if (showDebugMessages) Debug.Log($"WeaponShop: Arma '{weaponName}' no encontrada");
        return false;
    }
    
    /// <summary>
    /// Obtiene información de todas las armas para UI.
    /// </summary>
    public WeaponShopInfo[] GetShopInfo()
    {
        if (weaponInventory == null) return new WeaponShopInfo[0];
        
        WeaponBase[] weapons = weaponInventory.GetAllWeapons();
        bool[] unlockStatus = weaponInventory.GetUnlockStatus();
        WeaponShopInfo[] shopInfo = new WeaponShopInfo[weapons.Length];
        
        for (int i = 0; i < weapons.Length; i++)
        {
            shopInfo[i] = new WeaponShopInfo
            {
                weapon = weapons[i],
                isUnlocked = unlockStatus[i],
                canAfford = weapons[i] != null ? GetCurrentCoins() >= weapons[i].ShopPrice : false,
                weaponIndex = i
            };
        }
        
        return shopInfo;
    }
    
    /// <summary>
    /// Verifica si la tienda está abierta.
    /// </summary>
    public bool IsShopOpen()
    {
        return shopOpen;
    }
    
    /// <summary>
    /// Abre la tienda.
    /// </summary>
    public void OpenShop()
    {
        if (!shopOpen)
        {
            ToggleShop();
        }
    }
    
    /// <summary>
    /// Cierra la tienda.
    /// </summary>
    public void CloseShop()
    {
        if (shopOpen)
        {
            ToggleShop();
        }
    }
    
    /// <summary>
    /// Método para usar desde UI buttons.
    /// </summary>
    public void OnPurchaseButtonClicked(int weaponIndex)
    {
        TryPurchaseWeapon(weaponIndex);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Auto-abrir tienda cuando el jugador se acerca
        if (other.CompareTag("Player"))
        {
            if (showDebugMessages) Debug.Log("WeaponShop: Jugador cerca de la tienda. Presiona E para abrir.");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // Auto-cerrar tienda cuando el jugador se aleja
        if (other.CompareTag("Player"))
        {
            if (shopOpen)
            {
                CloseShop();
            }
        }
    }
}

/// <summary>
/// Información de un arma en la tienda.
/// </summary>
[System.Serializable]
public struct WeaponShopInfo
{
    public WeaponBase weapon;
    public bool isUnlocked;
    public bool canAfford;
    public int weaponIndex;
}

