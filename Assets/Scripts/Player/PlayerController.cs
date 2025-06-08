using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Movement_Behaviour))]
public class PlayerController : MonoBehaviour, PlayerBase.IPlayer_BaseActions
{
    private Movement_Behaviour movementBehaviour;
    private Look_Behaviour lookBehaviour;
    private Combat_Behaviour combatBehaviour;
    private Camera mainCamera;
    private PlayerBase playerInputActions;
    private Vector2 pointerPosition;
    private Vector2 movementValue;
    private bool isRunning;
    [SerializeField]
    private int MaxHealth = 100;
    // Sala actual
    private Room currentRoom;
    public Health health;
    // Eventos
    public event Action OnPlayerDeath;
    public event Action<int> OnPlayerDamageTaken;
    public event Action<int> OnPlayerHealed;
    private void Awake()
    {

        movementBehaviour = GetComponent<Movement_Behaviour>();
        lookBehaviour = GetComponent<Look_Behaviour>();
        combatBehaviour = GetComponent<Combat_Behaviour>();
        health = GetComponent<Health>();
        if (health != null)
        {
            health.SetMaxHealth(MaxHealth);
        }
        mainCamera = Camera.main;
        playerInputActions = new PlayerBase();
        playerInputActions.Player_Base.SetCallbacks(this);
    }
    private void FixedUpdate()
    {
        if (isRunning)
        {
            movementBehaviour.Run(movementValue);
        }
        else
        {
            movementBehaviour.Move(movementValue);
        }
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        movementValue = context.ReadValue<Vector2>();
    }
    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isRunning = true;
        }
        else if (context.canceled)
        {
            isRunning = false;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Dash triggered."); // Replace with actual implementation
            movementBehaviour.Dash();
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Attack triggered."); // Replace with actual implementation
            combatBehaviour.MeleeAttack();

        }
    }

    public void OnBuyWeapon(InputAction.CallbackContext context)
    {
        Debug.Log("Weapon_Bought context"); // Replace with actual implementation
    }
    public void OnChangeWeapon(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            Debug.Log("Weapon_Changed context");
        }
    }
    public void OnShoot(InputAction.CallbackContext context)
    { 
        if(context.performed)
        {
            Debug.Log("Shoot Performed");
        }
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 mousePosition = context.ReadValue<Vector2>();
            Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));
            lookBehaviour.LookAt(worldMousePosition);
            lookBehaviour.RotateAsset();
        }
    }

    public void SetCurrentRoom(Room room)
    {
        currentRoom = room;
    }
    public Room GetCurrentRoom()
    {
        return currentRoom;
    }
    private void OnEnable()
    {
        playerInputActions.Enable();
    }
    private void OnDisable()
    {
        playerInputActions.Disable();
    }
    public void TakeDamage(int damage)
    {
        if (health != null)
        {
            health.TakeDamage(damage);
            OnPlayerDamageTaken?.Invoke(damage);
            if (health.CurrentHealth <= 0)
            {
                OnPlayerDeath?.Invoke();
            }
        }
    }
    public void Heal(int amount)
    {
        if (health != null)
        {
            health.Heal(amount);
            OnPlayerHealed?.Invoke(amount);
        }
    }
    public void Die()
    {
        if (health != null && health.CurrentHealth <= 0)
        {
            OnPlayerDeath?.Invoke();
            // Aquí se podría implementar la lógica de muerte del jugador, como reiniciar la escena o mostrar un menú de Game Over
            Debug.Log("El jugador ha muerto.");
        }
    }
    public bool BuyItem(int cost, string itemname)
    {
        // Aquí se implementaría la lógica de compra de items
        // Por ejemplo, verificar si el jugador tiene suficientes monedas y restar el costo
        // Retornar true si la compra fue exitosa, false en caso contrario
        Debug.Log($"Intentando comprar el item: {itemname} por {cost} monedas.");
        return true; // Simulación de compra exitosa
    }
}
