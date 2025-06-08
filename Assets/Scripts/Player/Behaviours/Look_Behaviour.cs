using UnityEngine;
using UnityEngine.UIElements;

public class Look_Behaviour : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform rotator;
    private Vector2 lookDirection;
    public void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void LookAt(Vector2 targetPosition)
    {
        //Debug.Log("LookAt");
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        lookDirection = direction;
        UpdateAnimatorParameters();
    }
    private void UpdateAnimatorParameters()
    {
        if (animator != null)
        {
            //Debug.Log("AnimarorUpdated");
            // Set look direction for animations
            animator.SetFloat("Horizontal", lookDirection.x);
            animator.SetFloat("Vertical", lookDirection.y);
        }
        else
        {
            Debug.LogError("Animator is null!");
        }
    }
    public void RotateAsset()
    {
        if (rotator != null)
        {
            // Calculate angle in degrees from the lookDirection vector
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;

            // Apply the rotation to the Rotator object
            rotator.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            Debug.LogError("Rotator is null!");
        }
    }
}
