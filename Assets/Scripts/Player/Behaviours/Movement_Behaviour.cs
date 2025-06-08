using UnityEngine;
using System.Collections;

public class Movement_Behaviour : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float dashDistance = 5f; // Distance to dash
    [SerializeField] private float dashDuration = 0.2f; // Time to complete the dash
    [SerializeField] private float dashCooldown = 1f;  // Cooldown duration
    [SerializeField] private float invincibilityDuration = 0.5f; // Optional invincibility
    [SerializeField] private bool isWalking = false;
    [SerializeField] private bool isRunning = false;

    private Rigidbody2D rb;

    [SerializeField]private Animator animator;

    private Vector2 movementInput;
    private bool canDash = true;
    private bool isDashing = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
    }
    public void Move(Vector2 input)
    {
        if (isDashing) return; // Prevent movement during dash

        movementInput = input.normalized;

        if (movementInput != Vector2.zero)
        {
            isWalking = true;
            isRunning = false; // Ensure walking overrides running if movement starts
            rb.MovePosition(rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            isWalking = false; // Stop walking when input is zero
        }

        UpdateAnimatorParameters();
    }

    public void Run(Vector2 input)
    {
        if (isDashing) return; // Prevent movement during dash

        movementInput = input;

        if (movementInput != Vector2.zero)
        {
            isWalking = true; // Ensure running overrides walking
            isRunning = true;
            rb.MovePosition(rb.position + movementInput * runSpeed * Time.fixedDeltaTime);
        }
        else
        {
            isRunning = false; // Stop running when input is zero
        }

        UpdateAnimatorParameters();
    }
    public void Dash()
    {
        if (canDash && movementInput != Vector2.zero)
        {
            Debug.Log("DashInitialaized");
            Vector2 dashDirection = movementInput.normalized;
            Vector2 dashTarget = rb.position + dashDirection * dashDistance;

            // Start dash logic
            StartCoroutine(DashCoroutine(dashTarget));
        }
        else
        {
            Debug.Log("Dash not allowed. CanDash: " + canDash + ", MovementInput: " + movementInput);
        }
    }
    private IEnumerator DashCoroutine(Vector2 targetPosition)
    {
        canDash = false;
        isDashing = true;
        isWalking = false;
        isRunning = false;

        Vector2 startPosition = rb.position;
        float elapsedTime = 0f;

        // Smoothly move from start to target over dashDuration
        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dashDuration; // Normalized time (0 to 1)
            rb.MovePosition(Vector2.Lerp(startPosition, targetPosition, t));
            yield return null; // Wait for the next frame
        }

        rb.MovePosition(targetPosition); // Ensure final position is precise
        isDashing = false;
        isWalking = true;
        isRunning = false;

        // Optional: Invincibility logic
        if (invincibilityDuration > 0)
        {
            StartCoroutine(InvincibilityCoroutine());
        }

        // Dash cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private IEnumerator InvincibilityCoroutine()
    {
        yield return new WaitForSeconds(invincibilityDuration);
        // Re-enable collision or disable invincibility effects
    }
    private void UpdateAnimatorParameters()
    {
        if (animator != null)
        {
            // Update movement states
            animator.SetBool("IsWalking", isWalking);
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsDashing", isDashing);

            // Handle idle state (if no movement input)
            if (movementInput == Vector2.zero && !isDashing)
            {
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", false);
            }
        }
        else
        {
            Debug.LogError("Animator is null!");
        }
    }
}
