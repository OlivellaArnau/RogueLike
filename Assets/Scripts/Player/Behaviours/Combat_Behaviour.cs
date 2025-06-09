using System.Collections;
using UnityEngine;

public class Combat_Behaviour : MonoBehaviour
{
    [SerializeField] private GameObject pointer; 
    [SerializeField] private float meleeAttackDuration = 0.2f; 
    [SerializeField] private float meleeCooldown = 1f;
    private Animator animator;
    private bool isAttacking;
    private bool canAttack = true; 

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (pointer != null)
        {
            pointer.SetActive(false); 
        }
    }

    public void MeleeAttack()
    {
        if (!isAttacking && canAttack)
        {
            StartCoroutine(MeleeAttackCoroutine());
        }
        else if (!canAttack)
        {
            Debug.Log("Attack is on cooldown!");
        }
    }

    private IEnumerator MeleeAttackCoroutine()
    {
        canAttack = false; 
        isAttacking = true;

        if (pointer != null)
        {
            pointer.SetActive(true);
            Debug.Log("Pointer activated for melee attack.");
        }

        UpdateAnimatorParameters();

        yield return new WaitForSeconds(meleeAttackDuration);

        if (pointer != null)
        {
            pointer.SetActive(false);
        }

        isAttacking = false;
        UpdateAnimatorParameters();

        yield return new WaitForSeconds(meleeCooldown);

        canAttack = true;
    }
    
    public void SetWeapon()
    {
         
    }
    private void UpdateAnimatorParameters()
    {
        if (animator != null)
        {
            animator.SetBool("IsAttacking", isAttacking);
        }
        else
        {
            Debug.LogError("Animator is null!");
        }
    }
}
