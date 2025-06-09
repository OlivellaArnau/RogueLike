using UnityEngine;
using System.Collections;

public class Combat_Behaviour : MonoBehaviour
{
    [Header("Sistema de Armas")]
    [SerializeField] private WeaponBase currentWeapon;
    [SerializeField] private Transform pointer; // Dirección de disparo
    [SerializeField] private LayerMask enemyLayerMask;

    [Header("Estado")]
    private float lastWeaponUseTime = 0f;
    private bool isUsingWeapon = false;

    // Referencias
    private Animator animator;
    private Look_Behaviour lookBehaviour;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        lookBehaviour = GetComponent<Look_Behaviour>();

        if (pointer == null)
        {
            Transform foundPointer = transform.Find("pointer");
            if (foundPointer != null) pointer = foundPointer;
            if (pointer == null)
                Debug.LogError("Pointer no asignado en Combat_Behaviour");
        }
    }
    private void Update()
    {
        HandleInput();
        UpdateAnimator();
    }
    private void HandleInput()
    {
        if (currentWeapon == null)
        {
            Debug.LogWarning("No hay arma equipada");
            return;
        }
    }
    private void UseCurrentWeapon()
    {
        Debug.Log($"[WeaponSO] {currentWeapon.WeaponName} usado");
        if (Input.GetMouseButton(0))

        if (!currentWeapon.CanUse(lastWeaponUseTime))
            Debug.Log("Arma en cooldown");

        isUsingWeapon = true;
        Vector3 target = GetTargetPosition();

        bool used = currentWeapon.UseWeapon(transform, target);
        if (used)
        {
            lastWeaponUseTime = Time.time;
        }

        isUsingWeapon = false;
    }

    private Vector3 GetTargetPosition()
    {
        if (currentWeapon is MeleeWeapon) return transform.position;

        if (pointer != null)
            return transform.position + pointer.right * currentWeapon.Range;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;
        return mouseWorld;
    }

    private void UpdateAnimator()
    {
        if (animator != null)
            animator.SetBool("IsAttacking", isUsingWeapon);
    }

    public void EquipWeapon(WeaponBase weapon)
    {
        if (currentWeapon != null)
            currentWeapon.OnUnequip(transform);

        currentWeapon = weapon;

        if (currentWeapon != null)
            currentWeapon.OnEquip(transform);

        Debug.Log($"[Combat] Arma equipada: {currentWeapon?.WeaponName}");
    }

    public WeaponBase GetCurrentWeapon() => currentWeapon;

    public Collider2D[] GetEnemiesInArea(Vector3 center, float radius)
    {
        return Physics2D.OverlapCircleAll(center, radius, enemyLayerMask);
    }

    public RaycastHit2D GetEnemyInDirection(Vector3 origin, Vector3 direction, float distance)
    {
        return Physics2D.Raycast(origin, direction, distance, enemyLayerMask);
    }

    public Vector3 GetPointerDirection() => pointer != null ? pointer.right : Vector3.right;

    public Vector3 GetPointerPosition() => pointer != null ? pointer.position : transform.position;

    public LayerMask GetEnemyLayerMask() => enemyLayerMask;

    public void Shoot()
    {
        Debug.Log("[Combat] Shoot() llamado");

        if (currentWeapon != null)
        {
            Debug.Log("[Combat] currentWeapon NO es null");
            UseCurrentWeapon();
        }
        else
        {
            Debug.Log("[Combat] currentWeapon es null");
        }
        // Asegurarse de que currentWeapon no sea null antes de usarlo
    }
}
