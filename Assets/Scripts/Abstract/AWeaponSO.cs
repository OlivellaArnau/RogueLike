using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "Scriptable Objects/WeaponSO")]
public abstract class AWeaponSO : ScriptableObject
{
    [SerializeField] protected float damage;
    [SerializeField] protected float fireRate;
    [SerializeField] public int price;
    [SerializeField] protected GameObject weaponPrefab;
    [SerializeField] protected GameObject weapon;
    [SerializeField] protected bool IsAquired;
    [SerializeField] protected bool IsEquipped;

    public abstract void Shoot();
}
