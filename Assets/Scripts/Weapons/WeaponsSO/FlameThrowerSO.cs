using UnityEngine;

[CreateAssetMenu(fileName = "FlameThrowerSO", menuName = "Scriptable Objects/FlameThrowerSO")]
public class FlameThrowerSO : AWeaponSO
{
    public ParticleSystem flameThrowerParticles;
    public void OnEnable()
    {
        flameThrowerParticles = weapon.GetComponent<ParticleSystem>();
    }
    public override void Shoot()
    {
        Debug.Log("Flame thrower shooting");
    }
    public void Awake()
    {
        flameThrowerParticles.Stop();
    }
}
