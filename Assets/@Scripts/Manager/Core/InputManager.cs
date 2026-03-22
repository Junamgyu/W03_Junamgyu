using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    private InputSystem_Actions _input;

    // Mouse - Gamepad Look
    public event Action<InputAction.CallbackContext> OnLook;

    // Player Input
    public event Action<InputAction.CallbackContext> OnMove;
    public event Action<InputAction.CallbackContext> OnJump;
    public event Action<InputAction.CallbackContext> OnPrimaryAttack;
    public event Action<InputAction.CallbackContext> OnSecondaryAttack;
    public event Action<InputAction.CallbackContext> OnSlowMotionSkill;
    public event Action<InputAction.CallbackContext> OnDeadeyeSkill;
    public event Action<InputAction.CallbackContext> OnCheatOne;

    // System Input
    public event Action<InputAction.CallbackContext> OnPause;

    public void Initialize()
    {
        if (IsInitialized) return;

        _input = new InputSystem_Actions();

        BindActions();
        _input.Player.Enable();
        _input.UI.Enable();

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

        // UI Actions
        ui.Pause.started += HandlePause;

        // TODO: Boss Intro
        // 카메라 매니저가 보스 인트로 컷씬 인보크할 때, 플레이어 입력을 잠시 비활성화하는 기능 추가 필요
        // cameraManager.OnBossIntro += DisablePlayerInput();
        // cameraManager.OnBossOutro += EnablePlayerInput();
    }

    private void HandleLook(InputAction.CallbackContext ctx) => OnLook?.Invoke(ctx);
    private void HandleMove(InputAction.CallbackContext ctx) => OnMove?.Invoke(ctx);
    private void HandleJump(InputAction.CallbackContext ctx) => OnJump?.Invoke(ctx);
    private void HandlePrimaryAttack(InputAction.CallbackContext ctx) => OnPrimaryAttack?.Invoke(ctx);
    private void HandleSecondaryAttack(InputAction.CallbackContext ctx) => OnSecondaryAttack?.Invoke(ctx);
    private void HandleSlowMotionSkill(InputAction.CallbackContext ctx) => OnSlowMotionSkill?.Invoke(ctx);
    private void HandleDeadeyeSkill(InputAction.CallbackContext ctx) => OnDeadeyeSkill?.Invoke(ctx);
    private void HandleCheatOne(InputAction.CallbackContext ctx) => OnCheatOne?.Invoke(ctx);

    // UI Handlers
    private void HandlePause(InputAction.CallbackContext ctx) => OnPause?.Invoke(ctx);

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

        ui.Pause.started -= HandlePause;

        // TODO: 보스 인트로 컷씬이 끝나면 플레이어 입력을 다시 활성화하는 기능 추가
        // cameraManager.OnBossIntro -= DisablePlayerInput();
        // cameraManager.OnBossOutro -= EnablePlayerInput();

        _input.Player.Disable();
        _input.UI.Disable();
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
}