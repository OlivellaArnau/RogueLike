using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCoinDrop : MonoBehaviour
{
    [Header("Configuración de Drop")]
    [Tooltip("Prefab de la moneda que se generará")]
    [SerializeField] private GameObject coinPrefab;

    [Tooltip("Cantidad mínima de monedas a generar")]
    [SerializeField] private int minCoins = 1;

    [Tooltip("Cantidad máxima de monedas a generar")]
    [SerializeField] private int maxCoins = 3;

    [Tooltip("Probabilidad de generar monedas (0-1)")]
    [Range(0, 1)]
    [SerializeField] private float dropChance = 0.7f;

    [Tooltip("Fuerza con la que se lanzan las monedas")]
    [SerializeField] private float dropForce = 3f;

    [Tooltip("Altura máxima de las monedas al ser lanzadas")]
    [SerializeField] private float dropHeight = 1f;

    [Header("Configuración Avanzada")]
    [Tooltip("¿Generar siempre monedas? (ignora la probabilidad)")]
    [SerializeField] private bool alwaysDrop = false;

    [Tooltip("Valor de cada moneda")]
    [SerializeField] private int coinValue = 1;

    // Referencia al controlador del enemigo
    private EnemyController enemyController;

    private void Awake()
    {
        // Obtener el controlador del enemigo
        enemyController = GetComponent<EnemyController>();

        if (enemyController != null)
        {
            // Suscribirse al evento de muerte del enemigo
            enemyController.gameObject.GetComponent<Health>().OnDeath.AddListener(OnEnemyDeath);
        }
        else
        {
            Debug.LogWarning("No se encontró un EnemyController en este objeto");
        }
    }
    private void OnEnemyDeath()
    {
        if (ShouldDropCoins())
        {
            DropCoins();
        }
    }
    private bool ShouldDropCoins()
    {
        // Si siempre se deben generar, retornar true
        if (alwaysDrop)
            return true;

        // Generar según la probabilidad
        return Random.value <= dropChance;
    }
    private void DropCoins()
    {
        if (coinPrefab != null)
        {
            // Determinar cuántas monedas generar
            int coinCount = Random.Range(minCoins, maxCoins + 1);

            for (int i = 0; i < coinCount; i++)
            {
                // Calcular la posición de generación
                Vector3 dropPosition = transform.position;

                // Instanciar la moneda
                GameObject coinObject = Instantiate(coinPrefab, dropPosition, Quaternion.identity);

                // Configurar el valor de la moneda
                CoinDrop coinDrop = coinObject.GetComponent<CoinDrop>();
                if (coinDrop != null)
                {
                    coinDrop.SetCoinValue(coinValue);
                }

                // Aplicar una fuerza para que la moneda "salte"
                Rigidbody2D rb = coinObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Calcular una dirección aleatoria
                    Vector2 direction = Random.insideUnitCircle.normalized;

                    // Aplicar la fuerza
                    rb.AddForce(direction * dropForce, ForceMode2D.Impulse);

                    // Aplicar una fuerza hacia arriba para simular un salto
                    rb.AddForce(Vector2.up * dropHeight, ForceMode2D.Impulse);
                }
            }

            Debug.Log($"Enemigo ha soltado {coinCount} monedas");
        }
        else
        {
            Debug.LogWarning("No se ha asignado un prefab de moneda");
        }
    }
}


