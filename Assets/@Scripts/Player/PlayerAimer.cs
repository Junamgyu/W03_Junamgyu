using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimer : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private Transform _gunPivot; // 주무기(권총) 피봇

    [Header("Aim Assist")]
    [SerializeField] private float _aimAssistRadius = 1.5f;   // 감지 범위
    [SerializeField] private float _aimAssistStrengthMouse = 0.15f; // 당기는 강도 (낮을수록 자연스러움)
    [SerializeField] private float _aimAssistStrengthGamepad = 0.3f;
    [SerializeField] private LayerMask _enemyLayer;

    public Vector2 AimDirection { get; private set; } = Vector2.right;

    private void Awake()
    {
        if (_cam == null)
            _cam = Camera.main;
    }

    public void HandleLook(InputAction.CallbackContext ctx)
    {

        if (GameManager.Instance.IsPaused) return;


        Vector2 input = ctx.ReadValue<Vector2>();
        bool isGamepad = ctx.control.device is Gamepad;

        if (ctx.control.device is Mouse)
        {
            Vector2 mouseWorld = _cam.ScreenToWorldPoint(input);
            Vector2 dir = mouseWorld - (Vector2)transform.position;

            if (dir.sqrMagnitude > 0.001f)
                AimDirection = dir.normalized;
        }
        else if (isGamepad)
        {
            if (input.sqrMagnitude > 0.001f)
                Debug.Log("A");
                AimDirection = input.normalized;
        }

        // 에임 어시스트 적용
        AimDirection = GetAimAssistDirection(AimDirection, isGamepad);

        // GunPivot 회전
        float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
        _gunPivot.rotation = Quaternion.Euler(0f, 0f, angle);

    }

    Vector2 GetAimAssistDirection(Vector2 aimDir, bool isGamepad)
    {
        // 감지 중심점 결정
        Vector2 detectCenter = isGamepad
            ? (Vector2)transform.position + aimDir * 3f
            : (Vector2)_cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        Collider2D[] hits = Physics2D.OverlapCircleAll(detectCenter, _aimAssistRadius, _enemyLayer);
        if (hits.Length == 0) return aimDir;

        // 에임 방향에서 가장 가까운 적 찾기
        Collider2D closest = null;
        float closestAngle = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            Vector2 toEnemy = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            float angle = Vector2.Angle(aimDir, toEnemy);
            if (angle < closestAngle)
            {
                closestAngle = angle;
                closest = hit;
            }
        }

        if (closest == null) return aimDir;

        // 적 방향으로 당기기
        Vector2 toClosest = ((Vector2)closest.transform.position - (Vector2)transform.position).normalized;
        float strength = isGamepad ? _aimAssistStrengthGamepad : _aimAssistStrengthMouse;
        return Vector2.Lerp(aimDir, toClosest, strength).normalized;
    }
}