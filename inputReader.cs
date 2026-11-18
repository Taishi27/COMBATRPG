using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class inputReader : MonoBehaviour, Inputs.IPlayerMovementActions
{
    public Vector2 movementValue {  get; private set; }
    public Inputs inputs;
    public InputActionAsset LiveAsset => inputs.asset;
    public event Action jumpEvent;
    public event Action dodgeEvent;
    public event Action TargetEvent;
    public event Action CancelTargetEvent;
    public event Action LightAttackEvent;
    public event Action HeavyAttackEvent;
    public event Action BlockEvent;
    public event Action BlockCancelledEvent;
    public event Action openPauseMenu;
    public event Action closePauseMenu;
    public event Action RestartGameEvent;
    public event Action SprintEvent;
    public event Action SprintCancelEvent;
    public event Action AbilityEvent;
    public event Action Ability2Event;
    public event Action Ability3Event;
    public event Action Ability4Event;
    public event Action AbilityCancelledEvent;
    public event Action Ability2CancelledEvent;
    public event Action Ability3CancelledEvent;
    public event Action Ability4CancelledEvent;
    public event Action LightMagicCancelledEvent;
    public event Action UsePotionSlot1Event;
    public event Action UsePotionSlot2Event;
    public event Action UsePotionSlot3Event;
    public event Action UsePotionSlot4Event;
    public event Action ToggleMapEvent;

    public event Action HealingAbilityEvent;
    public event Action LightMagicAbilityEvent;

    //public bool IsAttacking;
    public playerStateMachine stateMachine;
   

    public bool HeavyAttackReleasedThisFrame { get; private set; }
    public bool IsLightAttackPressedThisFrame;
    public bool IsHeavyAttackPressedThisFrame;
    public bool IsBlocking { get; private set; } = false;

    public bool IsDodging = false;

    public bool IsDashing { get; private set; } = false;

    public bool IsSprinting { get; private set; } = false;

    public bool IsAbilityHeld => Keyboard.current.hKey.isPressed;
    public bool IsAbilityReleased => Keyboard.current.hKey.wasReleasedThisFrame;

    private Dictionary<AbilityInputKey, float> abilityHoldStartTimes = new();
    public float quickCastThreshold;
    public float sprintGenerosityBuffer = 1f;
    public bool JumpPressed => jumpBufferCounter > 0f;
    private bool isInPauseMenu = false;
    private float jumpBufferCounter;
    private float jumpBufferTime = 0.3f;

    [Header("Buffer windows")]
    public float lightAttackBuffer = 0.25f;

    public float heavyAttackBuffer = 0.25f;

    private float _lastLightAttackDown = -999f;

    private float _lastHeavyAttackDown = -999f;
    public bool IsLightAttackHeld { get; private set; }
    public bool IsHeavyAttackHeld { get; private set; }

    public bool IsUIInputLocked { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() 
    {
        inputs = new Inputs();
        inputs.PlayerMovement.SetCallbacks(this);
        inputs.PlayerMovement.Enable();
    }
    private void Awake()
    {
       stateMachine = GetComponent<playerStateMachine>();
       abilityHoldStartTimes = new Dictionary<AbilityInputKey, float>();
       foreach (AbilityInputKey key in Enum.GetValues(typeof(AbilityInputKey)))
       {
           abilityHoldStartTimes[key] = 0f;
       }
      
    }

    public void SetUIInputLocked(bool isLocked)
    {
        IsUIInputLocked = isLocked;

        if (isLocked)
        {
            movementValue = Vector2.zero;
            IsBlocking = false;
            IsDodging = false;
            IsDashing = false;
            IsSprinting = false;
            IsLightAttackHeld = false;
            IsHeavyAttackHeld = false;
            IsLightAttackPressedThisFrame = false;
            IsHeavyAttackPressedThisFrame = false;
            HeavyAttackReleasedThisFrame = false;
            jumpBufferCounter = 0f;
        }
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }
        if (!context.performed) { return; }
        dodgeEvent?.Invoke();
        IsDodging = true;


    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }
        if(!context.performed) { return; }
        jumpEvent?.Invoke();
        Debug.Log("juping input pressed");
        jumpBufferCounter = jumpBufferTime;
    }

    private void OnDestroy()
    {
        inputs.PlayerMovement.Disable();

    }
    // Update is called once per frame
    void Update()
    {
        if (IsUIInputLocked)
        {
            jumpBufferCounter -= Time.deltaTime;
            return;
        }

        if (IsLightAttackPressedThisFrame)
        {
            stateMachine.QueueAttack(AttackInputType.Light);
        }
          

        if (IsHeavyAttackPressedThisFrame)
        {
            stateMachine.QueueAttack(AttackInputType.Heavy);
            Debug.Log("heavy attack queued");
        }
        jumpBufferCounter -= Time.deltaTime;
    }
    private void LateUpdate()
    {
        // Reset one-frame triggers
        IsHeavyAttackPressedThisFrame = false;
        HeavyAttackReleasedThisFrame = false;
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked)
        {
            movementValue = Vector2.zero;
            return;
        }

        //Debug.Log("movin");
        movementValue = context.ReadValue<Vector2>();
        //Debug.Log(movementValue);
    }

    public void OnTarget(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }
        if(!context.performed) { return; }
        TargetEvent?.Invoke();

    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }
        if (!context.performed) { return; }
        CancelTargetEvent?.Invoke();
    }

    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }

        //if(context.interaction is HoldInteraction)
        //{
        //    isHeavyAttacking = true;
        //}
        //else if(context.interaction is TapInteraction)
        //{
        //    IsAttacking = true;
        //}

        if (context.performed)
        {
            _lastLightAttackDown = Time.time;  // stamp it
            IsLightAttackHeld = true;
            IsLightAttackPressedThisFrame = true;
            LightAttackEvent?.Invoke();
        }
        else if (context.canceled)
        {
           
            IsLightAttackPressedThisFrame = false;
            IsLightAttackHeld = false;

        }

        //LightAttackEvent?.Invoke();
    }
    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }

        if (context.started)
        {
            _lastHeavyAttackDown = Time.time;
            IsHeavyAttackPressedThisFrame = true;
            IsHeavyAttackHeld = true;
            HeavyAttackEvent?.Invoke();
        }
        else if (context.canceled)
        {
            HeavyAttackReleasedThisFrame = true;
            IsHeavyAttackHeld = false;
        }
    }

    /// Returns true once if the buffered press is within `window` seconds.
    public bool ConsumeLightAttackBuffered(float window = -1f)
    {
        if (window < 0f) window = lightAttackBuffer;
        if (Time.time - _lastLightAttackDown <= window)
        {
            // invalidate so it can�t be consumed twice
            _lastLightAttackDown = -999f;
            return true;
        }
        return false;
    }

    public bool ConsumeHeavyAttackBuffered(float window = -1f)
    {
        if (window < 0f) window = heavyAttackBuffer;
        if (Time.time - _lastHeavyAttackDown <= window)
        {
            // invalidate so it can�t be consumed twice
            _lastHeavyAttackDown = -999f;
            return true;
        }
        return false;
    }
    public void OnBlock(InputAction.CallbackContext context)
    {
        //if (IsUIInputLocked) { return; }

        if (context.performed)
        {
            BlockEvent?.Invoke();
            IsBlocking = true;
        }
        else if (context.canceled)
        {
            BlockCancelledEvent?.Invoke();
            IsBlocking = false;
        }
    }


    public void OnDash(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }

        if (context.performed)
        {
            IsDashing = true;
        }
        else if (context.canceled)
        {
            IsDashing = false;
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }

        if (context.performed)
        {
            SprintEvent?.Invoke();
            IsSprinting = true;
        }
        else if (context.canceled)
        {
            SprintCancelEvent?.Invoke();
            IsSprinting = false;

        }


    }

    public void OnExecuteAbility1(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }

        if (context.performed)
        {
            AbilityEvent?.Invoke();
        }
        if(context.canceled)
        {
            AbilityCancelledEvent?.Invoke();
        }
    }

    public void OnExecuteAbility2(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }

        if (context.started)
        {
            Ability2Event?.Invoke();
        }
        if (context.canceled)
        {
            Ability2CancelledEvent?.Invoke();
        }

    }

    public void OnExecuteAbility3(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }

        if (context.performed)
        {
            Ability3Event?.Invoke();
        }
        if (context.canceled)
        {
            Ability3CancelledEvent?.Invoke();
        }
    }

    public void OnExecuteAbility4(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }

        if (context.performed)
        {
            Ability4Event?.Invoke();
        }
        if (context.canceled)
        {
            Ability4CancelledEvent?.Invoke();
        }
    }

    public void OnExecuteHealingMagic(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }
        if(!context.performed) { return; }
        HealingAbilityEvent?.Invoke();

    }

    public void OnExecuteLightMagic(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }

        if (context.performed)
        {
            LightMagicAbilityEvent?.Invoke();
        }
        if (context.canceled)
        {
            LightMagicCancelledEvent?.Invoke();
        }


    }
    
    public void OnUsePotionSlot1(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) return;
        if (!context.performed) return;
        UsePotionSlot1Event?.Invoke();
    }

    public void OnUsePotionSlot2(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) return;
        if (!context.performed) return;
        UsePotionSlot2Event?.Invoke();
    }

    public void OnUsePotionSlot3(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) return;
        if (!context.performed) return;
        UsePotionSlot3Event?.Invoke();
    }

    public void OnUsePotionSlot4(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) return;
        if (!context.performed) return;
        UsePotionSlot4Event?.Invoke();
    }
    
    public void OnToggleMapEvent(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }
        if (!context.performed) { return; }

        Debug.Log("ToggleMap input fired");
        ToggleMapEvent?.Invoke();
    }


    /// Resolve a live InputAction from an InputActionReference (by GUID)
    public InputAction Resolve(InputActionReference reference)
    {
        if (reference == null || reference.action == null) return null;
        return LiveAsset.FindAction(reference.action.id);
    }

    public void EnableActions(IEnumerable<InputActionReference> refsList)
    {
        if (refsList == null) return;
        foreach (var r in refsList)
            Resolve(r)?.Enable();
    }

    public void DisableActions(IEnumerable<InputActionReference> refsList)
    {
        if (refsList == null) return;
        foreach (var r in refsList)
            Resolve(r)?.Disable();
    }
    public void ConsumeJumpInput() => jumpBufferCounter = 0f;

    public void OnRestartGame(InputAction.CallbackContext context)
    {
        if (IsUIInputLocked) { return; }

        //isInPauseMenu = !isInPauseMenu;

        //if (isInPauseMenu)
        //{
        //    openPauseMenu?.Invoke();
        //}
        //else
        //{
        //    closePauseMenu?.Invoke();
        //}
        if (context.performed)
        {
            RestartGameEvent?.Invoke();
        }
    }
}