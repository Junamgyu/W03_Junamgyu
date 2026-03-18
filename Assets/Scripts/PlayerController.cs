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
        Debug.Log(inputDir);
        player.playerMove.CanMove(inputDir);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            player.playerAttack.Shotgun();
        }
    }
}
