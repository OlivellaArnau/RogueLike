using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    
    [SerializeField] private float lifetime = 5f;
    
    [SerializeField] private GameObject impactEffect;
    
    [SerializeField] private LayerMask collisionLayers;
    
    [SerializeField] private bool destroyOnImpact = true;
    
    [SerializeField] private bool applyForceOnImpact = false;
    
    [SerializeField] private float impactForce = 5f;
    
    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & collisionLayers) != 0)
        {
            HandleImpact(collision);
        }
    }
    private void HandleImpact(Collider2D collision)
    {
        // Aplicar daño si el objeto tiene componente de salud
        Health health = collision.GetComponent<Health>();
        Debug.Log($"Impacto con {collision.gameObject.name}. Daño: {damage}, Salud: {health}");
        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log($"Daño aplicado a {collision.gameObject.name}. Salud restante: {health}");
        }

        // Aplicar fuerza si está configurado y el objeto tiene Rigidbody
        if (applyForceOnImpact)
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (collision.transform.position - transform.position).normalized;
                rb.AddForce(direction * impactForce, ForceMode2D.Impulse);
            }
        }

        // Crear efecto de impacto si existe
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }
        
        // Destruir el proyectil si está configurado
        if (destroyOnImpact)
        {
            Destroy(gameObject);
        }
    }
    
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    public int GetDamage()
    {
        return damage;
    }
}

