using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Player player;

    void Start()
    {
        player = GetComponent<Player>();

    }

    void Update()
    {

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 inputDir = context.ReadValue<Vector2>();
        player.playerMove.CanMove(inputDir);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
            player.playerAttack.FireCurrentWeapon();
    }

    public void OnShotgun(InputAction.CallbackContext context)
    {
        if (context.started)
            player.playerAttack.FireShotgun();
    }

    public void OnSkill(InputAction.CallbackContext context)
    {
        player.GetComponent<DeadeyeSkill>().OnSkill(context);
    }

    public void OnMarkTarget(InputAction.CallbackContext context)
    {
        player.GetComponent<DeadeyeSkill>().OnMarkTarget(context);
    }
}
