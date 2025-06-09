using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private float fullWidth = 300f;
    [SerializeField] private TextMeshProUGUI coinText;

    private Health playerHealth;
    private PlayerInventory playerInventory;

    private void Start()
    {
        StartCoroutine(WaitForPlayerAndInit());
    }

    private IEnumerator WaitForPlayerAndInit()
    {
        GameObject player = null;

        // Esperamos hasta que el jugador esté disponible
        while ((player = GameManager.Instance?.GetPlayer()) == null)
        {
            yield return null; // Espera un frame
        }

        // Conectamos el componente Health
        playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
        }
        else
        {
            Debug.LogError("El jugador no tiene componente Health.");
        }

        // Conectamos el componente PlayerInventory
        playerInventory = player.GetComponent<PlayerInventory>();
        if (playerInventory != null)
        {
            UpdateCoinText(playerInventory.GetCoins());
            playerInventory.onCoinsChanged.AddListener(UpdateCoinText);
        }
        else
        {
            Debug.LogWarning("El jugador no tiene componente PlayerInventory (se omitirá el conteo de monedas).");
        }
    }

    private void UpdateHealthBar(int current, int max)
    {
        if (healthBar != null && max > 0)
        {
            float normalizedHealth = Mathf.Clamp01((float)current / max);
            Vector2 size = healthBar.sizeDelta;
            size.x = fullWidth * normalizedHealth;
            healthBar.sizeDelta = size;
        }
    }

    private void UpdateCoinText(int coins)
    {
        if (coinText != null)
        {
            coinText.text = $"x{coins}";
        }
    }
}
