using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Extensión del PlayerInventory para gestionar armas.
/// Se integra con el PlayerInventory existente sin modificarlo.
/// </summary>
public class WeaponInventory : MonoBehaviour
{
    [Header("Configuración de Armas")]
    [SerializeField] private WeaponBase[] availableWeapons = new WeaponBase[2]; // Sniper y Flamethrower
    [SerializeField] private bool[] weaponUnlocked = new bool[2]; // Estado de desbloqueo
    [SerializeField] private int currentWeaponIndex = -1; // -1 = melee, 0 = sniper, 1 = flamethrower
    
    [Header("Arma Melee por Defecto")]
    [SerializeField] private BasicMeleeWeapon defaultMeleeWeapon;
    
    // Referencias
    private PlayerInventory playerInventory;
    private WeaponManager weaponManager;
    
    // Estado
    private WeaponBase currentEquippedWeapon;
    
    private void Awake()
    {
        // Obtener componentes
        playerInventory = GetComponent<PlayerInventory>();
        weaponManager = GetComponent<WeaponManager>();
        
        if (playerInventory == null)
        {
            Debug.LogError("WeaponInventory: PlayerInventory no encontrado");
        }
        
        if (weaponManager == null)
        {
            Debug.LogError("WeaponInventory: WeaponManager no encontrado");
        }
    }
    
    private void Start()
    {
        // Equipar arma melee por defecto
        if (defaultMeleeWeapon != null)
        {
            EquipMeleeWeapon();
        }
        
        // Verificar armas desbloqueadas desde el inicio
        CheckInitialUnlockedWeapons();
    }
    
    /// <summary>
    /// Verifica qué armas están desbloqueadas desde el inicio.
    /// </summary>
    private void CheckInitialUnlockedWeapons()
    {
        for (int i = 0; i < availableWeapons.Length; i++)
        {
            if (availableWeapons[i] != null && availableWeapons[i].AvailableFromStart)
            {
                weaponUnlocked[i] = true;
                Debug.Log($"WeaponInventory: {availableWeapons[i].WeaponName} desbloqueado desde el inicio");
            }
        }
    }
    
    /// <summary>
    /// Equipa el arma melee por defecto.
    /// </summary>
    public void EquipMeleeWeapon()
    {
        if (defaultMeleeWeapon != null && weaponManager != null)
        {
            weaponManager.EquipWeapon(defaultMeleeWeapon);
            currentEquippedWeapon = defaultMeleeWeapon;
            currentWeaponIndex = -1;
            
            Debug.Log($"WeaponInventory: Equipado arma melee - {defaultMeleeWeapon.WeaponName}");
        }
    }
    
    /// <summary>
    /// Equipa un arma ranged por índice.
    /// </summary>
    public bool EquipRangedWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= availableWeapons.Length)
        {
            Debug.LogWarning($"WeaponInventory: Índice de arma inválido: {weaponIndex}");
            return false;
        }
        
        if (!weaponUnlocked[weaponIndex])
        {
            Debug.Log($"WeaponInventory: Arma en índice {weaponIndex} no está desbloqueada");
            return false;
        }
        
        WeaponBase weapon = availableWeapons[weaponIndex];
        if (weapon == null)
        {
            Debug.LogWarning($"WeaponInventory: No hay arma asignada en índice {weaponIndex}");
            return false;
        }
        
        if (weaponManager != null)
        {
            weaponManager.EquipWeapon(weapon);
            currentEquippedWeapon = weapon;
            currentWeaponIndex = weaponIndex;
            
            Debug.Log($"WeaponInventory: Equipado arma ranged - {weapon.WeaponName}");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Cambia a la siguiente arma disponible.
    /// </summary>
    public void SwitchToNextWeapon()
    {
        int nextIndex = currentWeaponIndex + 1;
        
        // Buscar la siguiente arma desbloqueada
        for (int i = 0; i < availableWeapons.Length + 1; i++) // +1 para incluir melee
        {
            int checkIndex = (nextIndex + i) % (availableWeapons.Length + 1) - 1; // -1 para melee
            
            if (checkIndex == -1)
            {
                // Arma melee
                EquipMeleeWeapon();
                return;
            }
            else if (checkIndex >= 0 && checkIndex < availableWeapons.Length && weaponUnlocked[checkIndex])
            {
                EquipRangedWeapon(checkIndex);
                return;
            }
        }
        
        Debug.Log("WeaponInventory: No hay más armas disponibles");
    }
    
    /// <summary>
    /// Cambia a un slot específico de arma.
    /// </summary>
    public void SwitchToWeaponSlot(int slotIndex)
    {
        if (slotIndex == 0)
        {
            // Slot 0 = Melee
            EquipMeleeWeapon();
        }
        else if (slotIndex >= 1 && slotIndex <= availableWeapons.Length)
        {
            // Slots 1-2 = Armas ranged
            EquipRangedWeapon(slotIndex - 1);
        }
        else
        {
            Debug.LogWarning($"WeaponInventory: Slot inválido: {slotIndex}");
        }
    }
    
    /// <summary>
    /// Desbloquea un arma mediante compra.
    /// </summary>
    public bool UnlockWeapon(int weaponIndex, bool spendCoins = true)
    {
        if (weaponIndex < 0 || weaponIndex >= availableWeapons.Length)
        {
            Debug.LogWarning($"WeaponInventory: Índice de arma inválido para desbloqueo: {weaponIndex}");
            return false;
        }
        
        if (weaponUnlocked[weaponIndex])
        {
            Debug.Log($"WeaponInventory: Arma en índice {weaponIndex} ya está desbloqueada");
            return true;
        }
        
        WeaponBase weapon = availableWeapons[weaponIndex];
        if (weapon == null)
        {
            Debug.LogWarning($"WeaponInventory: No hay arma asignada en índice {weaponIndex}");
            return false;
        }
        
        // Verificar si tiene suficientes monedas
        if (spendCoins && playerInventory != null)
        {
            if (!HasEnoughCoins(weapon.ShopPrice))
            {
                Debug.Log($"WeaponInventory: No hay suficientes monedas para {weapon.WeaponName} (Precio: {weapon.ShopPrice})");
                return false;
            }
            
            // Gastar monedas
            SpendCoins(weapon.ShopPrice);
        }
        
        // Desbloquear arma
        weaponUnlocked[weaponIndex] = true;
        Debug.Log($"WeaponInventory: {weapon.WeaponName} desbloqueado exitosamente");
        
        return true;
    }
    
    /// <summary>
    /// Verifica si el jugador tiene suficientes monedas.
    /// </summary>
    private bool HasEnoughCoins(int requiredCoins)
    {
        if (playerInventory == null) return false;
        
        // Intentar obtener monedas usando reflexión (ya que no conocemos la implementación exacta)
        try
        {
            var coinsField = playerInventory.GetType().GetField("coins", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (coinsField != null)
            {
                int currentCoins = (int)coinsField.GetValue(playerInventory);
                return currentCoins >= requiredCoins;
            }
            
            // Intentar con propiedad
            var coinsProperty = playerInventory.GetType().GetProperty("Coins", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            if (coinsProperty != null)
            {
                int currentCoins = (int)coinsProperty.GetValue(playerInventory);
                return currentCoins >= requiredCoins;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"WeaponInventory: Error al verificar monedas: {e.Message}");
        }
        
        // Fallback: asumir que tiene suficientes monedas
        Debug.LogWarning("WeaponInventory: No se pudo verificar monedas, asumiendo suficientes");
        return true;
    }
    
    /// <summary>
    /// Gasta monedas del inventario.
    /// </summary>
    private void SpendCoins(int amount)
    {
        if (playerInventory == null) return;
        
        try
        {
            // Intentar método SpendCoins
            var spendMethod = playerInventory.GetType().GetMethod("SpendCoins", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            if (spendMethod != null)
            {
                spendMethod.Invoke(playerInventory, new object[] { amount });
                return;
            }
            
            // Intentar modificar campo directamente
            var coinsField = playerInventory.GetType().GetField("coins", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (coinsField != null)
            {
                int currentCoins = (int)coinsField.GetValue(playerInventory);
                coinsField.SetValue(playerInventory, currentCoins - amount);
                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"WeaponInventory: Error al gastar monedas: {e.Message}");
        }
        
        Debug.LogWarning("WeaponInventory: No se pudieron gastar las monedas");
    }
    
    /// <summary>
    /// Obtiene información del arma actual.
    /// </summary>
    public WeaponBase GetCurrentWeapon()
    {
        return currentEquippedWeapon;
    }
    
    /// <summary>
    /// Obtiene el índice del arma actual.
    /// </summary>
    public int GetCurrentWeaponIndex()
    {
        return currentWeaponIndex;
    }
    
    /// <summary>
    /// Verifica si un arma está desbloqueada.
    /// </summary>
    public bool IsWeaponUnlocked(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= weaponUnlocked.Length)
            return false;
        
        return weaponUnlocked[weaponIndex];
    }
    
    /// <summary>
    /// Obtiene información de un arma por índice.
    /// </summary>
    public WeaponBase GetWeaponByIndex(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= availableWeapons.Length)
            return null;
        
        return availableWeapons[weaponIndex];
    }
    
    /// <summary>
    /// Obtiene todas las armas disponibles.
    /// </summary>
    public WeaponBase[] GetAllWeapons()
    {
        return availableWeapons;
    }
    
    /// <summary>
    /// Obtiene el estado de desbloqueo de todas las armas.
    /// </summary>
    public bool[] GetUnlockStatus()
    {
        return (bool[])weaponUnlocked.Clone();
    }
    
    /// <summary>
    /// Configura un arma en un slot específico.
    /// </summary>
    public void SetWeapon(int weaponIndex, WeaponBase weapon)
    {
        if (weaponIndex >= 0 && weaponIndex < availableWeapons.Length)
        {
            availableWeapons[weaponIndex] = weapon;
            Debug.Log($"WeaponInventory: Arma {weapon?.WeaponName} asignada al slot {weaponIndex}");
        }
    }
    
    /// <summary>
    /// Configura el arma melee por defecto.
    /// </summary>
    public void SetDefaultMeleeWeapon(BasicMeleeWeapon meleeWeapon)
    {
        defaultMeleeWeapon = meleeWeapon;
        
        // Si no hay arma equipada, equipar la melee
        if (currentEquippedWeapon == null)
        {
            EquipMeleeWeapon();
        }
    }
}

