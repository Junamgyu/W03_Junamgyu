using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimer : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private Transform _gunPivot;

    public Vector2 AimDirection { get; private set; } = Vector2.right;

    private void Awake()
    {
        if (_cam == null)
            _cam = Camera.main;
    }

    public void HandleLook(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();

        if (ctx.control.device is Mouse)
        {
            Vector2 mouseWorld = _cam.ScreenToWorldPoint(input);
            Vector2 dir = mouseWorld - (Vector2)transform.position;

            if (dir.sqrMagnitude > 0.001f)
                AimDirection = dir.normalized;
        }
        else if (ctx.control.device is Gamepad)
        {
            if (input.sqrMagnitude > 0.001f)
                AimDirection = input.normalized;
        }

        // GunPivot 회전
        float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
        _gunPivot.rotation = Quaternion.Euler(0f, 0f, angle);


    }
}