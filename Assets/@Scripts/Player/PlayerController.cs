using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private Player _player;
    private InputManager _inputManager;
    private bool _isShieldHeld = false;

    void Start()
    {
        _player = GetComponent<Player>();

        if (!ManagerRegistry.TryGet<InputManager>(out _inputManager))
        {
            Debug.LogError("InputManager is not registered in ManagerRegistry.");
            return;
        }

        _inputManager.OnLook += HandleLook;
        _inputManager.OnLookMouse += HandleLookMouse;
        _inputManager.OnMove += HandleMove;
        _inputManager.OnJump += HandleJump;
        _inputManager.OnPrimaryAttack += HandlePrimaryAttack;
        _inputManager.OnSecondaryAttack += HandleSecondaryAttack;
        _inputManager.OnSlowMotionSkill += HandleSlowMotionSkill;
        //_inputManager.OnCheatOne += HandleCheatOne;
    }
    private void Update()
    {
        if(_isShieldHeld)
            Debug.Log("방패 누르고 있는 중");
    }

    private void OnDestroy()
    {
        if (_inputManager == null)
            return;

        _inputManager.OnLook -= HandleLook;
        _inputManager.OnLookMouse -= HandleLookMouse;
        _inputManager.OnMove -= HandleMove;
        _inputManager.OnJump -= HandleJump;
        _inputManager.OnPrimaryAttack -= HandlePrimaryAttack;
        _inputManager.OnSecondaryAttack -= HandleSecondaryAttack;
        _inputManager.OnSlowMotionSkill -= HandleSlowMotionSkill;
    }

    private void HandleLook(InputAction.CallbackContext ctx)
    {
        _player.playerAimer.HandleLook(ctx);
    }

    private void HandleLookMouse(InputAction.CallbackContext ctx)
    {
        _player.playerAimer.HandleLookMouse(ctx);
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

    private void HandlePrimaryAttack(InputAction.CallbackContext ctx)       //왼클릭으로 샷건
    {
        if (ctx.started)
            _player.playerAttack.FireShotgun();
    }

    private void HandleSecondaryAttack(InputAction.CallbackContext ctx)     //방패로 사용할 부분
    {
        if (ctx.performed)
        {
            _player.playerAttack.ShieldOn();
            _isShieldHeld = true;
            Debug.Log("Shield On");
        }
            

        if (ctx.canceled)
        {
            _isShieldHeld = false;
            _player.playerAttack.ShieldOff();
        }
            
    }

    private void HandleSlowMotionSkill(InputAction.CallbackContext ctx)
    {
        var skill = _player.GetComponent<DeadeyeSkill>();
        if (skill != null)
            skill.OnSlowMotion(ctx);
    }
    //private void HandleCheatOne(InputAction.CallbackContext ctx)
    //{
    //    if (ctx.started)
    //        _player.deadeyeSkill.AddGauge(20);
    //}
}