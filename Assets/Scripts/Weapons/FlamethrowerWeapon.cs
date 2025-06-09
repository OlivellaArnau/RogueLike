using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Arma Flamethrower que utiliza ParticleSystem para daño continuo.
/// Aplica daño mientras las partículas colisionan con enemigos.
/// </summary>
[CreateAssetMenu(fileName = "New Flamethrower Weapon", menuName = "Weapons/Flamethrower Weapon")]
public class FlamethrowerWeapon : RangedWeapon
{
    [Header("Configuración Flamethrower")]
    [SerializeField] private GameObject flamethrowerPrefab;
    [SerializeField] private int damagePerSecond = 15;
    [SerializeField] private float maxFlameRange = 8f;
    [SerializeField] private float flameWidth = 2f;
    [SerializeField] private int damageTickRate = 1; // Cada cuánto aplicar daño
    
    [Header("Efectos")]
    [SerializeField] private bool applyBurnEffect = true;
    [SerializeField] private float burnDuration = 2f;
    [SerializeField] private int burnDamagePerSecond = 3;
    
    // Estado del arma
    private GameObject activeFlamethrower;
    private FlamethrowerController flamethrowerController;
    private bool isActive = false;
    
    /// <summary>
    /// Implementación del uso del lanzallamas.
    /// </summary>
    public override bool UseWeapon(Transform user, Vector3 target)
    {
        if (isActive)
        {
            // Si ya está activo, continuar el daño
            ContinueFlamethrower(user, target);
            return true;
        }
        else
        {
            // Activar lanzallamas
            return StartFlamethrower(user, target);
        }
    }
    
    /// <summary>
    /// Inicia el lanzallamas.
    /// </summary>
    private bool StartFlamethrower(Transform user, Vector3 target)
    {
        // Crear o activar el lanzallamas
        if (activeFlamethrower == null)
        {
            if (flamethrowerPrefab != null)
            {
                Vector3 firePosition = GetFirePosition(user);
                activeFlamethrower = Object.Instantiate(flamethrowerPrefab, firePosition, Quaternion.identity);
                activeFlamethrower.transform.SetParent(user);
            }
            else
            {
                // Crear lanzallamas proceduralmente
                activeFlamethrower = CreateFlamethrowerObject(user);
            }
            
            flamethrowerController = activeFlamethrower.GetComponent<FlamethrowerController>();
            if (flamethrowerController == null)
            {
                flamethrowerController = activeFlamethrower.AddComponent<FlamethrowerController>();
            }
        }
        
        // Configurar el controlador
        if (flamethrowerController != null)
        {
            flamethrowerController.Initialize(this, user);
            flamethrowerController.StartFlame();
            isActive = true;
            
            Debug.Log($"FlamethrowerWeapon: {weaponName} activado");
            return true;
        }
        
        Debug.LogError("FlamethrowerWeapon: No se pudo crear FlamethrowerController");
        return false;
    }
    
    /// <summary>
    /// Continúa el efecto del lanzallamas.
    /// </summary>
    private void ContinueFlamethrower(Transform user, Vector3 target)
    {
        if (flamethrowerController != null)
        {
            Vector3 fireDirection = GetFireDirection(user, target);
            flamethrowerController.UpdateDirection(fireDirection);
        }
    }
    
    /// <summary>
    /// Detiene el lanzallamas.
    /// </summary>
    public void StopFlamethrower()
    {
        if (flamethrowerController != null)
        {
            flamethrowerController.StopFlame();
        }
        
        isActive = false;
        Debug.Log($"FlamethrowerWeapon: {weaponName} desactivado");
    }
    
    /// <summary>
    /// Crea un objeto lanzallamas proceduralmente.
    /// </summary>
    private GameObject CreateFlamethrowerObject(Transform user)
    {
        GameObject flamethrowerGO = new GameObject("Flamethrower");
        flamethrowerGO.transform.SetParent(user);
        flamethrowerGO.transform.localPosition = Vector3.zero;
        
        // Añadir ParticleSystem
        ParticleSystem particles = flamethrowerGO.AddComponent<ParticleSystem>();
        ConfigureParticleSystem(particles);
        
        return flamethrowerGO;
    }
    
    /// <summary>
    /// Configura el ParticleSystem para el lanzallamas.
    /// </summary>
    private void ConfigureParticleSystem(ParticleSystem particles)
    {
        var main = particles.main;
        main.startLifetime = maxFlameRange / projectileSpeed;
        main.startSpeed = projectileSpeed;
        main.startSize = 0.5f;
        main.startColor = Color.red;
        main.maxParticles = 100;
        
        var emission = particles.emission;
        emission.rateOverTime = 50;
        
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = flameWidth * 10f; // Convertir a ángulo
        
        var collision = particles.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision2D;
        
        var trigger = particles.trigger;
        trigger.enabled = true;
        trigger.inside = ParticleSystemOverlapAction.Callback;
    }
    
    /// <summary>
    /// Verifica si el arma puede ser usada.
    /// </summary>
    public override bool CanUse(float lastUseTime)
    {
        // El lanzallamas puede usarse continuamente
        return true;
    }
    
    /// <summary>
    /// Método llamado cuando el arma es equipada.
    /// </summary>
    public override void OnEquip(Transform user)
    {
        base.OnEquip(user);
        Debug.Log($"FlamethrowerWeapon: {weaponName} equipado");
    }
    
    /// <summary>
    /// Método llamado cuando el arma es desequipada.
    /// </summary>
    public override void OnUnequip(Transform user)
    {
        base.OnUnequip(user);
        
        // Detener lanzallamas al desequipar
        if (isActive)
        {
            StopFlamethrower();
        }
        
        // Limpiar objeto del lanzallamas
        if (activeFlamethrower != null)
        {
            Object.Destroy(activeFlamethrower);
            activeFlamethrower = null;
            flamethrowerController = null;
        }
        
        Debug.Log($"FlamethrowerWeapon: {weaponName} desequipado");
    }
    
    // Propiedades públicas para el controlador
    public int DamagePerSecond => damagePerSecond;
    public int DamageTickRate => damageTickRate;
    public float MaxFlameRange => maxFlameRange;
    public float FlameWidth => flameWidth;
    
    /// <summary>
    /// Verifica si el lanzallamas está activo.
    /// </summary>
    public bool IsActive => isActive;
}

