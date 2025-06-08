using UnityEngine;
using System.Collections;

public class BombController : EnemyController
{
    [Header("Bomba - Componentes")]
    [Tooltip("Referencia al SpriteRenderer para cambiar el color")]
    [SerializeField] private SpriteRenderer bombRenderer;
    
    [Tooltip("Efecto de partículas para cuando está preparándose para explotar")]
    [SerializeField] private ParticleSystem primeParticles;
    
    [Tooltip("Efecto de sonido para cuando está preparándose para explotar")]
    [SerializeField] private AudioSource primeSound;
    
    [Tooltip("Efecto de sonido para la explosión")]
    [SerializeField] private AudioSource explosionSound;
    
    // Referencia a los datos específicos de la bomba
    private BombEnemyData bombData;
    
    // Variables para el parpadeo
    private bool isBlinking = false;
    private Coroutine blinkCoroutine;
    
    protected new void Awake()
    {
        base.Awake();
        
        // Obtener referencia a los datos específicos de la bomba
        bombData = EnemyData as BombEnemyData;
        
        if (bombData == null)
        {
            Debug.LogError("BombController requiere un BombEnemyData asignado en EnemyData");
        }
        
        // Inicializar color
        if (bombRenderer != null && bombData != null)
        {
            bombRenderer.color = bombData.NormalColor;
        }
        
        // Desactivar partículas inicialmente
        if (primeParticles != null)
        {
            primeParticles.Stop();
        }
    }
    
    public bool IsPlayerInPrimeRange()
    {
        if (PlayerTransform == null || bombData == null)
            return false;
            
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerTransform.position);
        return distanceToPlayer <= bombData.PrimeDistance;
    }
    public void StartPriming()
    {
        // Iniciar parpadeo
        if (!isBlinking && bombRenderer != null && bombData != null)
        {
            isBlinking = true;
            
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
            }
            
            blinkCoroutine = StartCoroutine(BlinkCoroutine());
        }
        
        // Activar partículas
        if (primeParticles != null && !primeParticles.isPlaying)
        {
            primeParticles.Play();
        }
        
        // Reproducir sonido
        if (primeSound != null && !primeSound.isPlaying)
        {
            primeSound.Play();
        }
    }
    public void StopPriming()
    {
        // Detener parpadeo
        if (isBlinking)
        {
            isBlinking = false;
            
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
            
            // Restaurar color normal
            if (bombRenderer != null && bombData != null)
            {
                bombRenderer.color = bombData.NormalColor;
            }
        }
        
        // Detener partículas
        if (primeParticles != null && primeParticles.isPlaying)
        {
            primeParticles.Stop();
        }
        
        // Detener sonido
        if (primeSound != null && primeSound.isPlaying)
        {
            primeSound.Stop();
        }
    }
    private IEnumerator BlinkCoroutine()
    {
        while (isBlinking && bombRenderer != null && bombData != null)
        {
            // Alternar entre color normal y color de preparación
            float t = Mathf.PingPong(Time.time * bombData.BlinkSpeed, 1f);
            bombRenderer.color = Color.Lerp(bombData.NormalColor, bombData.PrimedColor, t);
            
            yield return null;
        }
    }
    public void PlayExplosionSound()
    {
        if (explosionSound != null)
        {
            // Desacoplar el sonido del objeto para que siga sonando después de que el objeto sea destruido
            explosionSound.transform.SetParent(null);
            explosionSound.Play();
            
            // Destruir el objeto de sonido después de que termine de reproducirse
            Destroy(explosionSound.gameObject, explosionSound.clip.length);
        }
    }
}

