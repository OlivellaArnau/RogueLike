using UnityEngine;

[CreateAssetMenu(fileName = "NewTurretData", menuName = "Scriptable Objects/Turret Data")]
public class TurretEnemyData : EnemyDataSO
{
    [Header("Torreta - Específico")]
    [Tooltip("Ángulo máximo de rotación (en grados)")]
    [SerializeField] private float maxRotationAngle = 180f;

    [Tooltip("¿Puede la torreta rotar 360 grados?")]
    [SerializeField] private bool canRotate360 = true;

    [Tooltip("Rango máximo de disparo")]
    [SerializeField] private float maxShootRange = 15f;

    [Tooltip("Precisión del disparo (0-1, donde 1 es perfecta)")]
    [Range(0, 1)]
    [SerializeField] private float accuracy = 0.9f;

    [Tooltip("Tiempo que tarda en detectar al jugador una vez está en rango")]
    [SerializeField] private float detectionDelay = 0.5f;

    // Propiedades públicas para acceder a los campos privados
    public float MaxRotationAngle => maxRotationAngle;
    public bool CanRotate360 => canRotate360;
    public float MaxShootRange => maxShootRange;
    public float Accuracy => accuracy;
    public float DetectionDelay => detectionDelay;
}

