using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WeaponInventory : MonoBehaviour
{
    public static WeaponInventory Instance { get; private set; }

    [Header("Configuración de Armas")]
    [SerializeField] private WeaponBase[] availableWeapons = new WeaponBase[2]; // Sniper y Flamethrower
    [SerializeField] private bool[] weaponUnlocked = new bool[2]; // Estado de desbloqueo
    [SerializeField] private int currentWeaponIndex = -1; // -1 = melee, 0 = sniper, 1 = flamethrower

    
    [Header("Arma Melee por Defecto")]
    [SerializeField] private BasicMeleeWeapon defaultMeleeWeapon;
    
    // Referencias
    private PlayerInventory playerInventory;
    private Combat_Behaviour combat_Behaviour;
    
    // Estado
    private WeaponBase currentEquippedWeapon;
    
    private void Awake()
    {
        //Patron de SIngleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        Debug.Log("WeaponInventory: Singleton inicializado correctamente");
        // Obtener componentes
        playerInventory = GetComponent<PlayerInventory>();
        combat_Behaviour = GetComponent<Combat_Behaviour>();
        
        if (playerInventory == null)
        {
            Debug.LogError("WeaponInventory: PlayerInventory no encontrado");
        }
        
        if (combat_Behaviour == null)
        {
            Debug.LogError("WeaponInventory: WeaponManager no encontrado");
        }

    }

    private void Start()
    {
        CheckInitialUnlockedWeapons();

        for (int i = 0; i < availableWeapons.Length; i++)
        {
            string weaponName = availableWeapons[i] != null ? availableWeapons[i].WeaponName : "null";
            Debug.Log($"[Start] Slot {i} = {weaponName} | Unlocked: {weaponUnlocked[i]}");
        }

        currentWeaponIndex = -1;
        EquipMeleeWeapon();
    }


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
    public void EquipMeleeWeapon()
    {
        if (defaultMeleeWeapon != null && combat_Behaviour != null)
        {
            combat_Behaviour.EquipWeapon(defaultMeleeWeapon);
            currentEquippedWeapon = defaultMeleeWeapon;
            currentWeaponIndex = -1;
            
            Debug.Log($"WeaponInventory: Equipado arma melee - {defaultMeleeWeapon.WeaponName}");
        }
    }
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
        
        if (combat_Behaviour != null)
        {
            combat_Behaviour.EquipWeapon(weapon);
            currentEquippedWeapon = weapon;
            currentWeaponIndex = weaponIndex;
            
            Debug.Log($"WeaponInventory: Equipado arma ranged - {weapon.WeaponName}");
            return true;
        }
        
        return false;
    }
    public void SwitchToNextWeapon()
    {
        int totalWeapons = availableWeapons.Length;

        int nextIndex = currentWeaponIndex;

        do
        {
            nextIndex++;
            if (nextIndex >= totalWeapons)
                nextIndex = -1; // volver al melee
        }
        while (nextIndex != -1 && !weaponUnlocked[nextIndex]);

        Debug.Log($"[WeaponInventory] Cambiando a índice: {nextIndex}");

        if (nextIndex == -1)
        {
            EquipMeleeWeapon();
        }
        else
        {
            EquipRangedWeapon(nextIndex);
        }
    }

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
    public WeaponBase GetCurrentWeapon()
    {
        return currentEquippedWeapon;
    }
    public int GetCurrentWeaponIndex()
    {
        return currentWeaponIndex;
    }
    public bool IsWeaponUnlocked(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= weaponUnlocked.Length)
            return false;
        
        return weaponUnlocked[weaponIndex];
    }
    public WeaponBase GetWeaponByIndex(int weaponIndex)
    {
        if (weaponIndex == -1)
            return defaultMeleeWeapon;

        if (weaponIndex < 0 || weaponIndex >= availableWeapons.Length)
            return null;

        return availableWeapons[weaponIndex];
    }
    public WeaponBase[] GetAllWeapons()
    {
        return availableWeapons;
    }
    public bool[] GetUnlockStatus()
    {
        return (bool[])weaponUnlocked.Clone();
    }
    public void SetWeapon(int weaponIndex, WeaponBase weapon)
    {
        if (weaponIndex >= 0 && weaponIndex < availableWeapons.Length)
        {
            availableWeapons[weaponIndex] = weapon;
            Debug.Log($"WeaponInventory: Arma {weapon?.WeaponName} asignada al slot {weaponIndex}");
        }
    }
    public void SetDefaultMeleeWeapon(BasicMeleeWeapon meleeWeapon)
    {
        defaultMeleeWeapon = meleeWeapon;
        
        // Si no hay arma equipada, equipar la melee
        if (currentEquippedWeapon == null)
        {
            EquipMeleeWeapon();
        }
    }
    public int GetWeaponIndex(WeaponBase weapon)
    {
        for (int i = 0; i < availableWeapons.Length; i++)
        {
            if (availableWeapons[i] == weapon)
                return i;
        }
        return -1;
    }

}

