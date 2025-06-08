using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("General")]
    [Tooltip("Nombre del enemigo")]
    [SerializeField] private string enemyName;

    [Tooltip("Salud máxima del enemigo")]
    [SerializeField] private int maxHealth = 100;

    [Header("Detección")]
    [Tooltip("Rango de detección del jugador")]
    [SerializeField] private float detectionRange = 10f;

    [Header("Combate")]
    [Tooltip("Daño base que causa el enemigo")]
    [SerializeField] private int damage = 10;

    [Tooltip("Tiempo de enfriamiento entre ataques")]
    [SerializeField] private float attackCooldown = 2f;

    [Header("Movimiento")]
    [Tooltip("Velocidad de movimiento")]
    [SerializeField] private float moveSpeed = 3f;

    [Tooltip("Velocidad de rotación en grados por segundo")]
    [SerializeField] private float rotationSpeed = 180f;

    [Header("Drops")]
    [Tooltip("Probabilidad de soltar un objeto al morir (0-1)")]
    [Range(0, 1)]
    [SerializeField] private float dropChance = 0.5f;

    [Tooltip("Posibles objetos que puede soltar")]
    [SerializeField] private DropData[] possibleDrops;

    // Propiedades públicas para acceder a los campos privados
    public string EnemyName => enemyName;
    public int MaxHealth => maxHealth;
    public float DetectionRange => detectionRange;
    public int Damage => damage;
    public float AttackCooldown => attackCooldown;
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public float DropChance => dropChance;
    public DropData[] PossibleDrops => possibleDrops;
}

