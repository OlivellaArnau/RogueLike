using UnityEngine;

/// <summary>
/// Clase base para armas de combate a distancia.
/// Utiliza el pointer para apuntado y dirección.
/// </summary>
public abstract class RangedWeapon : WeaponBase
{
    [Header("Configuración Ranged")]
    [SerializeField] protected float projectileSpeed = 10f;
    [SerializeField] protected bool usePointerDirection = true;
    [SerializeField] protected float spread = 0f; // Dispersión del disparo
    
    public float ProjectileSpeed => projectileSpeed;
    public bool UsePointerDirection => usePointerDirection;
    public float Spread => spread;
    
    protected Vector3 GetFireDirection(Transform user, Vector3 target)
    {
        Combat_Behaviour combatBehaviour = user.GetComponent<Combat_Behaviour>();
        Vector3 direction;
        
        if (usePointerDirection && combatBehaviour != null)
        {
            // Usar dirección del pointer
            direction = combatBehaviour.GetPointerDirection();
        }
        else
        {
            // Usar dirección hacia el target
            direction = (target - user.position).normalized;
        }
        
        // Aplicar dispersión si está configurada
        if (spread > 0f)
        {
            float randomAngle = Random.Range(-spread, spread);
            direction = Quaternion.AngleAxis(randomAngle, Vector3.forward) * direction;
        }
        
        return direction;
    }

    protected Vector3 GetFirePosition(Transform user)
    {
        Combat_Behaviour combatBehaviour = user.GetComponent<Combat_Behaviour>();
        
        if (combatBehaviour != null)
        {
            return combatBehaviour.GetPointerPosition();
        }
        
        return user.position;
    }

    protected bool HasClearLineOfSight(Vector3 origin, Vector3 direction, float distance, LayerMask obstacleLayer)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, obstacleLayer);
        return hit.collider == null;
    }
}

