using UnityEngine;
using UnityEngine.InputSystem;
using System;   

public class PlayerInputBridge : MonoBehaviour
{
    // Properties that other scripts can read, but not modify
    // Movement scripts
    public Vector2 MoveInput {get; private set;}
    public bool JumpTriggered {get; set;}
    public bool IsSprinting {get; private set;}
    // Action scripts
    public event Action OnAttackTriggered;
    public event Action OnDodgeTriggered;
    public event Action<int> OnSwapCharacterTriggered;  // index of characters on team [0-3]

    public void Start()
    {
        // Locks the mouse pointer into the center of the game view window
        Cursor.lockState = CursorLockMode.Locked;
        
        // Hides the cursor so it doesn't float over your gameplay mesh
        Cursor.visible = false;
    }

    // Linked directly to your Player Input component event for Move
    // Method automatically called when WASD keys are pressed/released
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    // Linked directly to your Player Input component event for Jump
    // Method automatically called when JUMP key is pressed
    public void OnJump(InputAction.CallbackContext context)
    {
        // Sets to true ONLY on the exact frame of the key pressing down
        if (context.started)
        {
            JumpTriggered = context.started;
        }
    }

    // Method automatically called when SPRINT key is pressed
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed) IsSprinting = true;  // key held down
        if (context.canceled) IsSprinting = false; // key released
    }

    // Method automatically called when ATTACK key is pressed
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // Fires the event out to the AttackHandler script
            OnAttackTriggered?.Invoke();
            Debug.Log("Attack!");
        }
    }

    // Method automatically called when DODGE key is pressed
    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // Fires the event out to the Dodge/Movement systems
            OnDodgeTriggered?.Invoke();
            Debug.Log("Dodge!");
        }
    }

    // Character Swapping
    public void OnSwap1(InputAction.CallbackContext context)
    {
        if (context.started) OnSwapCharacterTriggered?.Invoke(0);
    }

    public void OnSwap2(InputAction.CallbackContext context)
    {
        if (context.started) OnSwapCharacterTriggered?.Invoke(1);
    }

    public void OnSwap3(InputAction.CallbackContext context)
    {
        if (context.started) OnSwapCharacterTriggered?.Invoke(2);
    }

    public void OnSwap4(InputAction.CallbackContext context)
    {
        if (context.started) OnSwapCharacterTriggered?.Invoke(3);
    }
}