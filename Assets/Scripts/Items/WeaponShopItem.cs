using UnityEngine;
using TMPro;

public class WeaponShopItem : MonoBehaviour
{
    [Header("Configuración del arma en venta")]
    public WeaponBase weaponToSell;

    [Header("UI")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private TextMeshProUGUI uiText;

    private bool playerInRange = false;
    private PlayerInventory playerInventory;

    private void Start()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInventory = other.GetComponent<PlayerInventory>();

            if (uiPanel != null && weaponToSell != null)
            {
                uiText.text = $"{weaponToSell.WeaponName}\nPrecio: {weaponToSell.ShopPrice}\n[F] Comprar";
                uiPanel.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            uiPanel?.SetActive(false);
        }
    }

    public void TryPurchase()
    {
        if (!playerInRange || weaponToSell == null || playerInventory == null)
            return;

        if (playerInventory.SpendCoins(weaponToSell.ShopPrice))
        {
            int index = WeaponInventory.Instance.GetWeaponIndex(weaponToSell);
            if (index != -1)
            {
                WeaponInventory.Instance.UnlockWeapon(index);
                WeaponInventory.Instance.EquipRangedWeapon(index);
                Debug.Log($"Arma {weaponToSell.WeaponName} comprada");
                uiPanel.SetActive(false);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("Arma no encontrada en WeaponInventory.");
            }

            Debug.Log($"Arma {weaponToSell.WeaponName} comprada");
            uiPanel.SetActive(false);
            Destroy(gameObject); // Eliminar tienda tras compra
        }
        else
        {
            Debug.Log("No tienes suficiente dinero o ya la tienes");
        }
    }
}
