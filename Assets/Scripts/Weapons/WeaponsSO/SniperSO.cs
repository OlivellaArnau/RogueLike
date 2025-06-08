using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName = "SniperSO", menuName = "Scriptable Objects/SniperSO")]
public class SniperSO : AWeaponSO
{
    public float range;
    public override void Shoot()
    {
        throw new System.NotImplementedException();
    }
}
