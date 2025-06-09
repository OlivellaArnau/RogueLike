using UnityEngine;

/// <summary>
/// Gestor principal de armas que se integra con los componentes existentes.
/// Mantiene compatibilidad con PlayerController, CombatBehaviour y Look_Behaviour.
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Referencias Requeridas")]
    [SerializeField] private Transform pointer; // Referencia al pointer para apuntado
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Combat_Behaviour combatBehaviour;
    [SerializeField] private Look_Behaviour lookBehaviour;
    
    [Header("Configuración")]
    [SerializeField] private WeaponBase currentWeapon;
    [SerializeField] private LayerMask enemyLayerMask = -1;
    
    // Variables de estado
    private float lastWeaponUseTime = 0f;
    private bool isUsingWeapon = false;
    
    // Referencias a armas específicas para optimización
    private MeleeWeapon currentMeleeWeapon;
    private RangedWeapon currentRangedWeapon;
    
    private void Awake()
    {
        // Obtener componentes automáticamente si no están asignados
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        
        if (playerInventory == null)
            playerInventory = GetComponent<PlayerInventory>();
            
        if (combatBehaviour == null)
            combatBehaviour = GetComponent<Combat_Behaviour>();
            
        if (lookBehaviour == null)
            lookBehaviour = GetComponent<Look_Behaviour>();
        
        // Buscar el pointer si no está asignado
        if (pointer == null)
        {
            Transform pointerTransform = transform.Find("pointer");
            if (pointerTransform != null)
                pointer = pointerTransform;
        }
    }
    
    private void Start()
    {
        // Verificar componentes requeridos
        ValidateComponents();
        
        // Configurar arma inicial si está disponible
        if (currentWeapon != null)
        {
            EquipWeapon(currentWeapon);
        }
    }
    
    private void Update()
    {
        // Manejar input de armas
        HandleWeaponInput();
    }
    
    /// <summary>
    /// Valida que todos los componentes requeridos estén presentes.
    /// </summary>
    private void ValidateComponents()
    {
        if (playerController == null)
        {
            Debug.LogError("WeaponManager: PlayerController no encontrado!");
        }
        
        if (playerInventory == null)
        {
            Debug.LogError("WeaponManager: PlayerInventory no encontrado!");
        }
        
        if (combatBehaviour == null)
        {
            Debug.LogError("WeaponManager: Combat_Behaviour no encontrado!");
        }
        
        if (lookBehaviour == null)
        {
            Debug.LogError("WeaponManager: Look_Behaviour no encontrado!");
        }
        
        if (pointer == null)
        {
            Debug.LogWarning("WeaponManager: Pointer no encontrado! Las armas ranged pueden no funcionar correctamente.");
        }
    }
    
    /// <summary>
    /// Maneja el input del jugador para usar armas.
    /// </summary>
    private void HandleWeaponInput()
    {
        if (currentWeapon == null || isUsingWeapon)
            return;
        
        // Input para usar arma (click izquierdo o espacio)
        bool useWeaponInput = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
        
        if (useWeaponInput && CanUseCurrentWeapon())
        {
            UseCurrentWeapon();
        }
        
        // Input para cambiar armas (Q o números)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchToNextWeapon();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchToWeaponSlot(0);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchToWeaponSlot(1);
        }
    }
    
    /// <summary>
    /// Verifica si el arma actual puede ser usada.
    /// </summary>
    private bool CanUseCurrentWeapon()
    {
        if (currentWeapon == null)
            return false;
        
        return currentWeapon.CanUse(lastWeaponUseTime);
    }
    
    /// <summary>
    /// Usa el arma actual.
    /// </summary>
    private void UseCurrentWeapon()
    {
        if (currentWeapon == null)
            return;
        
        Vector3 targetPosition = GetTargetPosition();
        
        isUsingWeapon = true;
        bool weaponUsed = currentWeapon.UseWeapon(transform, targetPosition);
        
        if (weaponUsed)
        {
            lastWeaponUseTime = Time.time;
        }
        
        isUsingWeapon = false;
    }
    
    /// <summary>
    /// Obtiene la posición objetivo basada en el tipo de arma.
    /// </summary>
    private Vector3 GetTargetPosition()
    {
        // Para armas melee, usar posición del jugador
        if (currentWeapon is MeleeWeapon)
        {
            return transform.position;
        }
        
        // Para armas ranged, usar dirección del pointer
        if (pointer != null)
        {
            // Usar la dirección hacia donde apunta el pointer
            Vector3 pointerDirection = pointer.right; // Asumiendo que el pointer apunta hacia la derecha
            return transform.position + pointerDirection * currentWeapon.Range;
        }
        
        // Fallback: usar posición del mouse en mundo
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z;
        return mousePosition;
    }
    
    /// <summary>
    /// Equipa un arma específica.
    /// </summary>
    public void EquipWeapon(WeaponBase weapon)
    {
        if (weapon == null)
            return;
        
        // Desequipar arma actual
        if (currentWeapon != null)
        {
            currentWeapon.OnUnequip(transform);
        }
        
        // Equipar nueva arma
        currentWeapon = weapon;
        currentWeapon.OnEquip(transform);
        
        // Actualizar referencias específicas para optimización
        currentMeleeWeapon = weapon as MeleeWeapon;
        currentRangedWeapon = weapon as RangedWeapon;
        
        Debug.Log($"Arma equipada: {weapon.WeaponName}");
    }
    
    /// <summary>
    /// Cambia a la siguiente arma disponible.
    /// </summary>
    public void SwitchToNextWeapon()
    {
        WeaponInventory weaponInventory = GetComponent<WeaponInventory>();
        if (weaponInventory != null)
        {
            weaponInventory.SwitchToNextWeapon();
        }
        else
        {
            Debug.Log("WeaponManager: WeaponInventory no encontrado");
        }
    }
    
    /// <summary>
    /// Cambia a un slot específico de arma.
    /// </summary>
    public void SwitchToWeaponSlot(int slotIndex)
    {
        WeaponInventory weaponInventory = GetComponent<WeaponInventory>();
        if (weaponInventory != null)
        {
            weaponInventory.SwitchToWeaponSlot(slotIndex);
        }
        else
        {
            Debug.Log($"WeaponManager: WeaponInventory no encontrado para slot {slotIndex}");
        }
    }
    
    /// <summary>
    /// Obtiene el arma actualmente equipada.
    /// </summary>
    public WeaponBase GetCurrentWeapon()
    {
        return currentWeapon;
    }
    
    /// <summary>
    /// Obtiene el componente Combat_Behaviour para uso de armas melee.
    /// </summary>
    public Combat_Behaviour GetCombatBehaviour()
    {
        return combatBehaviour;
    }
    
    /// <summary>
    /// Obtiene la dirección hacia donde apunta el pointer.
    /// </summary>
    public Vector3 GetPointerDirection()
    {
        if (pointer != null)
        {
            return pointer.right; // Asumiendo que el pointer apunta hacia la derecha
        }
        
        return Vector3.right; // Dirección por defecto
    }
    
    /// <summary>
    /// Obtiene la posición del pointer.
    /// </summary>
    public Vector3 GetPointerPosition()
    {
        if (pointer != null)
        {
            return pointer.position;
        }
        
        return transform.position;
    }
    
    /// <summary>
    /// Verifica si hay enemigos en un área específica.
    /// </summary>
    public Collider2D[] GetEnemiesInArea(Vector3 center, float radius)
    {
        return Physics2D.OverlapCircleAll(center, radius, enemyLayerMask);
    }
    
    /// <summary>
    /// Verifica si hay enemigos en una dirección específica.
    /// </summary>
    public RaycastHit2D GetEnemyInDirection(Vector3 origin, Vector3 direction, float distance)
    {
        return Physics2D.Raycast(origin, direction, distance, enemyLayerMask);
    }
}

