using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private Player _player;
    private InputManager _inputManager;

    void Start()
    {
        _player = GetComponent<Player>();

        if (!ManagerRegistry.TryGet<InputManager>(out _inputManager))
        {
            Debug.LogError("InputManager is not registered in ManagerRegistry.");
            return;
        }

        _inputManager.OnLook += HandleLook;
        _inputManager.OnMove += HandleMove;
        _inputManager.OnJump += HandleJump;
        _inputManager.OnPrimaryAttack += HandlePrimaryAttack;
        _inputManager.OnSecondaryAttack += HandleSecondaryAttack;
        _inputManager.OnSlowMotionSkill += HandleSlowMotionSkill;
        _inputManager.OnDeadeyeSkill += HandleDeadeyeSkill;
        _inputManager.OnMarkTarget += HandleMarkTarget;
        _inputManager.OnCheatOne += HandleCheatOne;
    }

    private void OnDestroy()
    {
        if (_inputManager == null)
            return;

        _inputManager.OnLook -= HandleLook;
        _inputManager.OnMove -= HandleMove;
        _inputManager.OnJump -= HandleJump;
        _inputManager.OnPrimaryAttack -= HandlePrimaryAttack;
        _inputManager.OnSecondaryAttack -= HandleSecondaryAttack;
        _inputManager.OnDeadeyeSkill -= HandleDeadeyeSkill;
        _inputManager.OnSlowMotionSkill -= HandleSlowMotionSkill;
        _inputManager.OnMarkTarget -= HandleMarkTarget;
    }

    private void HandleLook(InputAction.CallbackContext ctx)
    {
        _player.playerAimer.HandleLook(ctx);
        //_player.deadeyeSkill.HandleLook(ctx);
    }

    private void HandleMove(InputAction.CallbackContext ctx)
    {
        Vector2 inputDir = ctx.ReadValue<Vector2>();
        _player.playerMove.CanMove(inputDir);
    }

    private void HandleJump(InputAction.CallbackContext ctx)
    {
        _player.playerJump.OnJump(ctx);
    }

    private void HandlePrimaryAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            _player.playerAttack.FireCurrentWeapon();
    }

    private void HandleSecondaryAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            _player.playerAttack.FireShotgun();
    }

    private void HandleSlowMotionSkill(InputAction.CallbackContext ctx)
    {
        var skill = _player.GetComponent<DeadeyeSkill>();
        if (skill != null)
            skill.OnSlowMotion(ctx);
    }

    private void HandleDeadeyeSkill(InputAction.CallbackContext ctx)
    {
        var skill = _player.GetComponent<DeadeyeSkill>();
        if (skill != null)
            skill.OnDeadeye(ctx);
    }

    private void HandleMarkTarget(InputAction.CallbackContext ctx)
    {
        var skill = _player.GetComponent<DeadeyeSkill>();
        if (skill != null)
            skill.OnMarkTarget(ctx);
    }

    private void HandleCheatOne(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            _player.deadeyeSkill.AddGauge();
    }
}