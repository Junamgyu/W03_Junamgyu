using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    private InputSystem_Actions _input;
    private CameraManager _cameraManager;

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

        _cameraManager = GetComponent<CameraManager>();

        _input = new InputSystem_Actions();
        BindActions();

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

        player.CheatOne.started += HandleCheatOne;
        player.Pause.started += HandlePause;

        CameraManager.OnBossIntro += DisablePlayerInput;
        CameraManager.OnBossOutro += EnablePlayerInput;

        ui.Pause.started += HandlePause;
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

        OnLook?.Invoke(ctx);
    }

    public void SetGameplayLookDeviceToGamepad()
    {
        IsUsingGamepadForLook = true;
    }

    private void HandleMove(InputAction.CallbackContext ctx)
    {
        OnMove?.Invoke(ctx);
    }

    private void HandleJump(InputAction.CallbackContext ctx)
    {
        OnJump?.Invoke(ctx);
    }

    private void HandlePrimaryAttack(InputAction.CallbackContext ctx)
    {
        OnPrimaryAttack?.Invoke(ctx);
    }

    private void HandleSecondaryAttack(InputAction.CallbackContext ctx)
    {
        OnSecondaryAttack?.Invoke(ctx);
    }

    private void HandleSlowMotionSkill(InputAction.CallbackContext ctx)
    {
        OnSlowMotionSkill?.Invoke(ctx);
    }

    private void HandleDeadeyeSkill(InputAction.CallbackContext ctx)
    {
        OnDeadeyeSkill?.Invoke(ctx);
    }

    private void HandleCheatOne(InputAction.CallbackContext ctx)
    {
        OnCheatOne?.Invoke(ctx);
    }

    private void HandlePause(InputAction.CallbackContext ctx)
    {
        OnPause?.Invoke(ctx);
    }

    private void HandleNavigate(InputAction.CallbackContext ctx)
    {
        OnNavigate?.Invoke(ctx);
    }

    private void HandleSubmit(InputAction.CallbackContext ctx)
    {
        OnSubmit?.Invoke(ctx);
    }

    private void HandleCancel(InputAction.CallbackContext ctx)
    {
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

        CameraManager.OnBossIntro -= DisablePlayerInput;
        CameraManager.OnBossOutro -= EnablePlayerInput;

        _input.Player.Disable();
        _input.UI.Disable();
    }
}