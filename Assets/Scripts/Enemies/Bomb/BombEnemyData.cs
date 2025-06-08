using UnityEngine;

[CreateAssetMenu(fileName = "NewBombData", menuName = "Scriptable Objects/Bomb Data")]
public class BombEnemyData : EnemyDataSO
{
    [Header("Bomba - Específico")]
    [Tooltip("Radio de la explosión")]
    [SerializeField] private float explosionRadius = 3f;
    
    [Tooltip("Fuerza de la explosión")]
    [SerializeField] private float explosionForce = 10f;
    
    [Tooltip("Tiempo de preparación antes de explotar")]
    [SerializeField] private float primeTime = 0.5f;
    
    [Tooltip("Distancia a la que la bomba comienza a prepararse para explotar")]
    [SerializeField] private float primeDistance = 1.5f;
    
    [Tooltip("Multiplicador de daño de la explosión respecto al daño base")]
    [SerializeField] private float explosionDamageMultiplier = 1.5f;
    
    [Tooltip("Color de la bomba cuando está en estado normal")]
    [SerializeField] private Color normalColor = Color.white;
    
    [Tooltip("Color de la bomba cuando está preparándose para explotar")]
    [SerializeField] private Color primedColor = Color.red;
    
    [Tooltip("Velocidad de parpadeo cuando está preparándose para explotar")]
    [SerializeField] private float blinkSpeed = 5f;
    
    [Tooltip("¿Debe la bomba perseguir al jugador incluso si está fuera del rango de detección inicial?")]
    [SerializeField] private bool persistentChase = true;
    
    // Propiedades públicas para acceder a los campos privados
    public float ExplosionRadius => explosionRadius;
    public float ExplosionForce => explosionForce;
    public float PrimeTime => primeTime;
    public float PrimeDistance => primeDistance;
    public float ExplosionDamageMultiplier => explosionDamageMultiplier;
    public Color NormalColor => normalColor;
    public Color PrimedColor => primedColor;
    public float BlinkSpeed => blinkSpeed;
    public bool PersistentChase => persistentChase;
}

