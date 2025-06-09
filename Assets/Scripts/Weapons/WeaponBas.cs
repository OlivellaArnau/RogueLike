using UnityEngine;

/// <summary>
/// Clase base para todas las armas del juego.
/// Utiliza ScriptableObject para configuración flexible.
/// </summary>
public abstract class WeaponBase : ScriptableObject
{
    [Header("Información Básica")]
    [SerializeField] protected string weaponName = "Arma Base";
    [SerializeField] protected string description = "Descripción del arma";
    [SerializeField] protected Sprite weaponIcon;
    
    [Header("Estadísticas")]
    [SerializeField] protected int damage = 10;
    [SerializeField] protected float range = 5f;
    [SerializeField] protected float cooldown = 1f;
    
    [Header("Tienda")]
    [SerializeField] protected int shopPrice = 100;
    [SerializeField] protected bool availableFromStart = false;
    
    // Propiedades públicas
    public string WeaponName => weaponName;
    public string Description => description;
    public Sprite WeaponIcon => weaponIcon;
    public float Damage => damage;
    public float Range => range;
    public float Cooldown => cooldown;
    public int ShopPrice => shopPrice;
    public bool AvailableFromStart => availableFromStart;
    
    public abstract bool UseWeapon(Transform user, Vector3 target);
    
    public virtual bool CanUse(float lastUseTime)
    {
        return Time.time - lastUseTime >= cooldown;
    }

    public virtual void OnEquip(Transform user)
    {
        // Implementación base vacía
    }
    public virtual void OnUnequip(Transform user)
    {
        // Implementación base vacía
    }
}

