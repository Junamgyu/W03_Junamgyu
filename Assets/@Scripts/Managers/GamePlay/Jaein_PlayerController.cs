using UnityEngine;
using UnityEngine.InputSystem;

public class Jaein_PlayerController : MonoBehaviour
{
    private Player player;
    private InputManager _inputManager;

    void Start()
    {
        player = GetComponent<Player>();

        if (!ManagerRegistry.TryGet<InputManager>(out _inputManager))
        {
            Debug.LogError("InputManager is not registered in ManagerRegistry.");
            return;
        }

        _inputManager.OnMove += HandleMove;
        _inputManager.OnJump += HandleJump;
        _inputManager.OnPrimaryAttack += HandlePrimaryAttack;
        _inputManager.OnSecondaryAttack += HandleSecondaryAttack;
        _inputManager.OnSkill += HandleSkill;
        _inputManager.OnMarkTarget += HandleMarkTarget;
    }

    private void OnDestroy()
    {
        if (_inputManager == null)
            return;

        _inputManager.OnMove -= HandleMove;
        _inputManager.OnJump -= HandleJump;
        _inputManager.OnPrimaryAttack -= HandlePrimaryAttack;
        _inputManager.OnSecondaryAttack -= HandleSecondaryAttack;
        _inputManager.OnSkill -= HandleSkill;
        _inputManager.OnMarkTarget -= HandleMarkTarget;
    }

    private void HandleMove(InputAction.CallbackContext ctx)
    {
        Vector2 inputDir = ctx.ReadValue<Vector2>();
        player.playerMove.CanMove(inputDir);
    }

    private void HandleJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            player.playerJump.OnJump(ctx);
    }

    private void HandlePrimaryAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            player.playerAttack.FireCurrentWeapon();
    }

    private void HandleSecondaryAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            player.playerAttack.FireShotgun();
    }

    private void HandleSkill(InputAction.CallbackContext ctx)
    {
        var skill = player.GetComponent<DeadeyeSkill>();
        if (skill != null)
            skill.OnSkill(ctx);
    }

    private void HandleMarkTarget(InputAction.CallbackContext ctx)
    {
        var skill = player.GetComponent<DeadeyeSkill>();
        if (skill != null)
            skill.OnMarkTarget(ctx);
    }
}