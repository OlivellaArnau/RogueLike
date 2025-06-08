using UnityEngine;

[CreateAssetMenu(fileName = "NewDropData", menuName = "Scriptable Objects/Drop Data")]
public class DropData : ScriptableObject
{
    [Tooltip("Prefab del objeto a soltar")]
    [SerializeField] private GameObject dropPrefab;

    [Tooltip("Nombre del objeto")]
    [SerializeField] private string dropName;

    [Tooltip("Descripción del objeto")]
    [TextArea(2, 5)]
    [SerializeField] private string description;

    [Tooltip("Probabilidad relativa de que aparezca este objeto (peso)")]
    [SerializeField] private float weight = 1f;

    [Tooltip("Cantidad mínima que puede soltar")]
    [SerializeField] private int minAmount = 1;

    [Tooltip("Cantidad máxima que puede soltar")]
    [SerializeField] private int maxAmount = 2;

    [Tooltip("Tipo de objeto")]
    [SerializeField] private DropType dropType;
    public enum DropType
    {
        Key,        // Llave para abrir puertas
        Health,     // Objeto que restaura salud
        Currency,   // Monedas o similar
    }
    public GameObject DropPrefab => dropPrefab;
    public string DropName => dropName;
    public string Description => description;
    public float Weight => weight;
    public int MinAmount => minAmount;
    public int MaxAmount => maxAmount;
    public DropType Type => dropType;
    public int GetRandomAmount()
    {
        return Random.Range(minAmount, maxAmount + 1);
    }
}


