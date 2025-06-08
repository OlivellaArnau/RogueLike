using UnityEngine;

public class TurretController : EnemyController
{
    [Header("Torreta - Componentes")]
    [Tooltip("Transformación de la parte giratoria de la torreta")]
    [SerializeField] private Transform turretHead;
    
    [Tooltip("Punto de origen de los proyectiles")]
    [SerializeField] private Transform firePoint;
    
    [Tooltip("Línea de visión para detectar obstáculos")]
    [SerializeField] private LayerMask obstacleLayerMask;
    
    [Header("Torreta - Efectos")]
    [Tooltip("Efecto visual al detectar al jugador")]
    [SerializeField] private GameObject detectionEffect;
    
    [Tooltip("Efecto visual al perder al jugador")]
    [SerializeField] private GameObject lostTargetEffect;
    
    // Referencia a los datos específicos de la torreta
    private TurretEnemyData turretData;
    
    // Propiedades adicionales
    public Transform TurretHead => turretHead;
    public Transform FirePoint => firePoint;
    
    public new void Awake()
    {
        base.Awake();
        turretData = EnemyData as TurretEnemyData;
    }

    public bool HasLineOfSightToPlayer()
    {
        if (PlayerTransform == null)
            return false;
            
        Vector2 directionToPlayer = PlayerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        
        // Lanzar un raycast hacia el jugador para detectar obstáculos
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            directionToPlayer.normalized, 
            distanceToPlayer, 
            obstacleLayerMask
        );
        
        // Si no hay hit, hay línea de visión directa
        return hit.collider == null;
    }

    public bool IsPlayerInViewAngle()
    {
        if (PlayerTransform == null || turretData == null)
            return false;
            
        // Si puede rotar 360 grados, siempre está en ángulo de visión
        if (turretData.CanRotate360)
            return true;
            
        Vector2 directionToPlayer = PlayerTransform.position - transform.position;
        float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        
        // Normalizar el ángulo de la torreta y el ángulo hacia el jugador
        float turretAngle = transform.eulerAngles.z;
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(turretAngle, angleToPlayer));
        
        return angleDifference <= turretData.MaxRotationAngle * 0.5f;
    }

    public void ShowDetectionEffect()
    {
        if (detectionEffect != null)
        {
            detectionEffect.SetActive(true);
            
            // Desactivar después de un tiempo
            Invoke(nameof(HideDetectionEffect), 1f);
        }
    }

    private void HideDetectionEffect()
    {
        if (detectionEffect != null)
        {
            detectionEffect.SetActive(false);
        }
    }

    public void ShowLostTargetEffect()
    {
        if (lostTargetEffect != null)
        {
            lostTargetEffect.SetActive(true);
            
            // Desactivar después de un tiempo
            Invoke(nameof(HideLostTargetEffect), 1f);
        }
    }

    private void HideLostTargetEffect()
    {
        if (lostTargetEffect != null)
        {
            lostTargetEffect.SetActive(false);
        }
    }
}

