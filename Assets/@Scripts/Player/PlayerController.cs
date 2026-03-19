using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerController : MonoBehaviour
{
    Player _player;

    void Start()
    {
        _player = GetComponent<Player>();

    }

    void Update()
    {

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 inputDir = context.ReadValue<Vector2>();
        _player.playerMove.CanMove(inputDir);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        _player.playerJump.OnJump(context);
    }


    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
            _player.playerAttack.FireCurrentWeapon();
    }

    public void OnShotgun(InputAction.CallbackContext context)
    {
        if (context.started)
            _player.playerAttack.FireShotgun();
    }

    public void OnSkill(InputAction.CallbackContext context)
    {
        _player.GetComponent<DeadeyeSkill>().OnSkill(context);
    }

    public void OnMarkTarget(InputAction.CallbackContext context)
    {
        _player.GetComponent<DeadeyeSkill>().OnMarkTarget(context);
    }
}
