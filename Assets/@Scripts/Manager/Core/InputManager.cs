using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    private InputSystem_Actions _input;

    public event Action<InputAction.CallbackContext> OnLook;
    public event Action<InputAction.CallbackContext> OnMove;
    public event Action<InputAction.CallbackContext> OnJump;
    public event Action<InputAction.CallbackContext> OnPrimaryAttack;
    public event Action<InputAction.CallbackContext> OnSecondaryAttack;
    public event Action<InputAction.CallbackContext> OnSlowMotionSkill;
    public event Action<InputAction.CallbackContext> OnDeadeyeSkill;
    public event Action<InputAction.CallbackContext> OnCheatOne;

    public event Action<InputAction.CallbackContext> OnNavigate;
    public event Action<InputAction.CallbackContext> OnPause;
    public event Action<InputAction.CallbackContext> OnSubmit;
    public event Action<InputAction.CallbackContext> OnCancel;

    public bool IsUsingGamepad { get; private set; }
    public bool IsUsingGamepadForLook { get; private set; }

    public void Initialize()
    {
        if (IsInitialized)
            return;

        _input = new InputSystem_Actions();
        BindActions();

        if (Gamepad.current != null)
        {
            _input.bindingMask = InputBinding.MaskByGroup("Gamepad");
        }
        else
        {
            _input.bindingMask = InputBinding.MaskByGroup("Keyboard&Mouse");
        }

        IsInitialized = true;
    }

    private void BindActions()
    {
        var player = _input.Player;
        var ui = _input.UI;

        player.Look.performed += HandleLook;
        player.Look.canceled += HandleLook;

        player.Move.started += HandleMove;
        player.Move.performed += HandleMove;
        player.Move.canceled += HandleMove;

        player.Jump.started += HandleJump;
        player.Jump.performed += HandleJump;
        player.Jump.canceled += HandleJump;

        player.Attack.started += HandlePrimaryAttack;
        //player.Attack.performed += HandlePrimaryAttack;
        player.Attack.canceled += HandlePrimaryAttack;

        player.Shotgun.started += HandleSecondaryAttack;
        //player.Shotgun.performed += HandleSecondaryAttack;
        player.Shotgun.canceled += HandleSecondaryAttack;

        player.SlowMotionSkill.started += HandleSlowMotionSkill;
        player.SlowMotionSkill.performed += HandleSlowMotionSkill;
        player.SlowMotionSkill.canceled += HandleSlowMotionSkill;

        player.DeadeyeSkill.started += HandleDeadeyeSkill;
        player.DeadeyeSkill.performed += HandleDeadeyeSkill;
        player.DeadeyeSkill.canceled += HandleDeadeyeSkill;

        player.CheatOne.started += HandleCheatOne;
        player.Pause.started += HandlePause;

        ui.Pause.started += HandlePause;
        //ui.Navigate.started += HandleNavigate;
        //ui.Navigate.performed += HandleNavigate;
        //ui.Navigate.canceled += HandleNavigate;

        //ui.Submit.started += HandleSubmit;
        //ui.Submit.performed += HandleSubmit;

        //ui.Cancel.started += HandleCancel;
        //ui.Cancel.performed += HandleCancel;
    }

    private void UpdateLastUsedDevice(InputAction.CallbackContext ctx)
    {
        if (ctx.control == null || ctx.control.device == null)
            return;

        IsUsingGamepad = ctx.control.device is Gamepad;
    }

    private void HandleLook(InputAction.CallbackContext ctx)
    {
        if (ctx.control == null || ctx.control.device == null)
            return;

        var device = ctx.control.device;

        bool isGamepadLook = device is Gamepad;
        bool isMouseLook = device is Mouse;

        if (!isGamepadLook && !isMouseLook)
            return;

        IsUsingGamepadForLook = isGamepadLook;

        Debug.Log(
            $"[HandleLook] map={ctx.action.actionMap.name}, " +
            $"device={device.GetType().Name}, " +
            $"isGamepadForLook={IsUsingGamepadForLook}, " +
            $"value={ctx.ReadValue<Vector2>()}"
        );

        OnLook?.Invoke(ctx);
    }

    public void SetGameplayLookDeviceToGamepad()
    {
        IsUsingGamepadForLook = true;
    }

    private void HandleMove(InputAction.CallbackContext ctx)
    {
        //UpdateLastUsedDevice(ctx);
        OnMove?.Invoke(ctx);
    }

    private void HandleJump(InputAction.CallbackContext ctx)
    {
        //UpdateLastUsedDevice(ctx);
        OnJump?.Invoke(ctx);
    }

    private void HandlePrimaryAttack(InputAction.CallbackContext ctx)
    {
        //UpdateLastUsedDevice(ctx);
        OnPrimaryAttack?.Invoke(ctx);
    }

    private void HandleSecondaryAttack(InputAction.CallbackContext ctx)
    {
        //UpdateLastUsedDevice(ctx);
        OnSecondaryAttack?.Invoke(ctx);
    }

    private void HandleSlowMotionSkill(InputAction.CallbackContext ctx)
    {
        //UpdateLastUsedDevice(ctx);
        OnSlowMotionSkill?.Invoke(ctx);
    }

    private void HandleDeadeyeSkill(InputAction.CallbackContext ctx)
    {
        //UpdateLastUsedDevice(ctx);
        OnDeadeyeSkill?.Invoke(ctx);
    }

    private void HandleCheatOne(InputAction.CallbackContext ctx)
    {
        //UpdateLastUsedDevice(ctx);
        OnCheatOne?.Invoke(ctx);
    }

    private void HandlePause(InputAction.CallbackContext ctx)
    {
        //UpdateLastUsedDevice(ctx);
        OnPause?.Invoke(ctx);
    }

    private void HandleNavigate(InputAction.CallbackContext ctx)
    {
        //UpdateLastUsedDevice(ctx);
        OnNavigate?.Invoke(ctx);
    }

    private void HandleSubmit(InputAction.CallbackContext ctx)
    {
        //UpdateLastUsedDevice(ctx);
        OnSubmit?.Invoke(ctx);
    }

    private void HandleCancel(InputAction.CallbackContext ctx)
    {
        //UpdateLastUsedDevice(ctx);
        OnCancel?.Invoke(ctx);
    }

    public void EnablePlayerInput()
    {
        if (_input == null)
            return;

        _input.Player.Enable();
    }

    public void DisablePlayerInput()
    {
        if (_input == null)
            return;

        _input.Player.Disable();
    }

    public void EnableUIInput()
    {
        if (_input == null)
            return;

        _input.UI.Enable();
    }

    public void DisableUIInput()
    {
        if (_input == null)
            return;

        _input.UI.Disable();
    }

    private void OnDestroy()
    {
        if (_input == null)
            return;

        var player = _input.Player;
        var ui = _input.UI;

        player.Look.performed -= HandleLook;
        player.Look.canceled -= HandleLook;

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

        player.CheatOne.started -= HandleCheatOne;
        player.Pause.started -= HandlePause;

        ui.Pause.started -= HandlePause;
        ui.Navigate.started -= HandleNavigate;
        ui.Navigate.performed -= HandleNavigate;
        ui.Navigate.canceled -= HandleNavigate;

        ui.Submit.started -= HandleSubmit;
        ui.Submit.performed -= HandleSubmit;

        ui.Cancel.started -= HandleCancel;
        ui.Cancel.performed -= HandleCancel;

        _input.Player.Disable();
        _input.UI.Disable();
    }
}