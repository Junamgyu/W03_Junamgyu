using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    private InputSystem_Actions _input;

    public event Action<InputAction.CallbackContext> OnMove;
    public event Action<InputAction.CallbackContext> OnJump;
    public event Action<InputAction.CallbackContext> OnPrimaryAttack;
    public event Action<InputAction.CallbackContext> OnSecondaryAttack;
    public event Action<InputAction.CallbackContext> OnSlowMotionSkill;
    public event Action<InputAction.CallbackContext> OnDeadeyeSkill;
    public event Action<InputAction.CallbackContext> OnMarkTarget;
    public event Action<InputAction.CallbackContext> OnCheatOne;

    public void Initialize()
    {
        if (IsInitialized) return;

        _input = new InputSystem_Actions();

        BindActions();
        _input.Player.Enable();

        IsInitialized = true;
    }

    private void BindActions()
    {
        var player = _input.Player;

        player.Move.started += HandleMove;
        player.Move.performed += HandleMove;
        player.Move.canceled += HandleMove;

        player.Jump.started += HandleJump;
        player.Jump.performed += HandleJump;
        player.Jump.canceled += HandleJump;

        player.Attack.started += HandlePrimaryAttack;
        player.Attack.performed += HandlePrimaryAttack;
        player.Attack.canceled += HandlePrimaryAttack;

        player.Shotgun.started += HandleSecondaryAttack;
        player.Shotgun.performed += HandleSecondaryAttack;
        player.Shotgun.canceled += HandleSecondaryAttack;

        player.SlowMotionSkill.started += HandleSlowMotionSkill;
        player.SlowMotionSkill.performed += HandleSlowMotionSkill;
        player.SlowMotionSkill.canceled += HandleSlowMotionSkill;

        player.DeadeyeSkill.started += HandleDeadeyeSkill;
        player.DeadeyeSkill.performed += HandleDeadeyeSkill;
        player.DeadeyeSkill.canceled += HandleDeadeyeSkill;

        player.MarkTarget.started += HandleMarkTarget;
        player.MarkTarget.performed += HandleMarkTarget;
        player.MarkTarget.canceled += HandleMarkTarget;

        player.CheatOne.started += HandleCheatOne;
    }

    private void HandleMove(InputAction.CallbackContext ctx) => OnMove?.Invoke(ctx);
    private void HandleJump(InputAction.CallbackContext ctx) => OnJump?.Invoke(ctx);
    private void HandlePrimaryAttack(InputAction.CallbackContext ctx) => OnPrimaryAttack?.Invoke(ctx);
    private void HandleSecondaryAttack(InputAction.CallbackContext ctx) => OnSecondaryAttack?.Invoke(ctx);
    private void HandleSlowMotionSkill(InputAction.CallbackContext ctx) => OnSlowMotionSkill?.Invoke(ctx);
    private void HandleDeadeyeSkill(InputAction.CallbackContext ctx) => OnDeadeyeSkill?.Invoke(ctx);
    private void HandleMarkTarget(InputAction.CallbackContext ctx) => OnMarkTarget?.Invoke(ctx);
    private void HandleCheatOne(InputAction.CallbackContext ctx) => OnCheatOne?.Invoke(ctx);

    private void OnDestroy()
    {
        if (_input == null)
            return;

        var player = _input.Player;

        player.Move.started -= HandleMove;
        player.Move.performed -= HandleMove;
        player.Move.canceled -= HandleMove;

        player.Jump.started -= HandleJump;
        player.Jump.performed -= HandleJump;
        player.Jump.canceled -= HandleJump;

        player.Attack.started -= HandlePrimaryAttack;
        player.Attack.performed -= HandlePrimaryAttack;
        player.Attack.canceled -= HandlePrimaryAttack;

        player.Shotgun.started -= HandleSecondaryAttack;
        player.Shotgun.performed -= HandleSecondaryAttack;
        player.Shotgun.canceled -= HandleSecondaryAttack;

        player.SlowMotionSkill.started -= HandleSlowMotionSkill;
        player.SlowMotionSkill.performed -= HandleSlowMotionSkill;
        player.SlowMotionSkill.canceled -= HandleSlowMotionSkill;

        player.DeadeyeSkill.started -= HandleDeadeyeSkill;
        player.DeadeyeSkill.performed -= HandleDeadeyeSkill;
        player.DeadeyeSkill.canceled -= HandleDeadeyeSkill;

        player.MarkTarget.started -= HandleMarkTarget;
        player.MarkTarget.performed -= HandleMarkTarget;
        player.MarkTarget.canceled -= HandleMarkTarget;

        player.CheatOne.started -= HandleCheatOne;

        _input.Player.Disable();
    }
}