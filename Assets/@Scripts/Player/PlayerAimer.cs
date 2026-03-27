using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimer : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private Transform _shieldPivot; // 주무기(권총) 피봇
    [Header("Shiled Rotation")]
    [SerializeField] private float _maxShieldAngle = 25f;
    [Header("Shield Settings")]
    [SerializeField] private float _shieldDefaultAngle = 0f;        //방패 Off 기본 각도
    [SerializeField] private float _shieldSnapSpeed = 20f;          //방패 스냅 속도

    [Header("Aim Assist")]
    [SerializeField] private float _aimAssistRadiusMouse = 1.5f;
    [SerializeField] private float _aimAssistRadiusGamepad = 4f;
    [SerializeField] private float _aimAssistAngleGamepad = 30f;
    [SerializeField] private float _aimAssistStrengthMouse = 0.15f; // 당기는 강도 (낮을수록 자연스러움)
    [SerializeField] private float _aimAssistStrengthGamepad = 0.3f;
    [SerializeField] private LayerMask _enemyLayer;

    public Vector2 AimDirection { get; private set; } = Vector2.right;

    public bool IsUsingGamepad { get; private set; }

    private bool _isFacingRight = true;     //플레이어 회전을 위한 변수
    private bool _isShieldOn = false;
    private float _lockedShieldAngle = 0f;
    private Vector2 _mouseWorldPos;
    private Player _player;
    private Vector3 _shieldOriginaScale;

    private void Awake()
    {
        if (_cam == null)
            _cam = Camera.main;

            _player = GetComponent<Player>();
            _shieldOriginaScale = _shieldPivot.localScale;
    }

    //! 플레이어가 방향을 바뀔 때 호출
    public void SetFacingDirection(bool isFacingRight)
    {
        _isFacingRight = isFacingRight;
    }

    public void OnShieldOn()
    {
        _isShieldOn = true;

        //활성화 순간 마우스 방향으로 각도 고정
        Vector2 dir = _mouseWorldPos - (Vector2)transform.position;
        _lockedShieldAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    public void OnShieldOff()
    {
        _isShieldOn = false;

        if(_isFacingRight)
        {
            _shieldPivot.localScale = new Vector3(_shieldOriginaScale.x, _shieldOriginaScale.y, _shieldOriginaScale.z);
        }
        else
            _shieldPivot.localScale = new Vector3(_shieldOriginaScale.x, _shieldOriginaScale.y, _shieldOriginaScale.z);
    }

    private void Update()
    {
        UpdateShieldRotation();
        
    }

    private void UpdateShieldRotation()
    {
        float targetAngle;

        if(_isShieldOn)
        {
            targetAngle = _lockedShieldAngle;
        }
        else
        {
            targetAngle = _isFacingRight ? _shieldDefaultAngle : 180f + _shieldDefaultAngle;
        }

        float current = _shieldPivot.eulerAngles.z;
        float smoothed = Mathf.LerpAngle(current, targetAngle, Time.deltaTime * _shieldSnapSpeed);
        _shieldPivot.rotation = Quaternion.Euler(0f, 0f, smoothed);
    }

    public void HandleLook(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) return;
        Vector2 input = ctx.ReadValue<Vector2>();
        if (input.sqrMagnitude < 0.01f) return;

        IsUsingGamepad = true;
        AimDirection = input.normalized;

        Vector2 detectCenter = (Vector2)transform.position;
        AimDirection = GetAimAssistDirection(AimDirection, detectCenter, _aimAssistRadiusGamepad, _aimAssistAngleGamepad);
    }

    public void HandleLookMouse(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) return;
        IsUsingGamepad = false;

        Vector2 input = ctx.ReadValue<Vector2>();
        _mouseWorldPos = _cam.ScreenToWorldPoint(input);

        Vector2 dir = _mouseWorldPos - (Vector2)transform.position;
        if (dir.sqrMagnitude > 0.001f)
            AimDirection = dir.normalized; 

        if(_mouseWorldPos.x > transform.position.x)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            _isFacingRight = true;
            _player.playerAttack.SetPivotFacing(true);

            //자식 피봇 스케일 원상 복구
            //방패 OFF 일 때만 스케일 변경
            if(!_isShieldOn)
            _shieldPivot.localScale = new Vector3(_shieldOriginaScale.x, _shieldOriginaScale.y, _shieldOriginaScale.z);
            
        }
        else
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
            _isFacingRight = false;
            _player.playerAttack.SetPivotFacing(false);
            //부모 -1 상쇄
            //방패 OFF일 때만 스케일 변경
            if(!_isShieldOn)
            _shieldPivot.localScale = new Vector3(-_shieldOriginaScale.x, _shieldOriginaScale.y, _shieldOriginaScale.z);
        }

        Vector2 detectCenter = _mouseWorldPos;
        AimDirection = GetAimAssistDirection(AimDirection, detectCenter, _aimAssistRadiusMouse, 360f);
    }


    Vector2 GetAimAssistDirection(Vector2 aimDir, Vector2 detectCenter, float radius, float maxAngle)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(detectCenter, radius, _enemyLayer);
        if (hits.Length == 0) return aimDir;

        Collider2D closest = null;
        float closestAngle = float.MaxValue;
        foreach (Collider2D hit in hits)
        {
            Vector2 toEnemy = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            float angle = Vector2.Angle(aimDir, toEnemy);

            if (angle > maxAngle) continue; // 각도 범위 밖이면 무시

            if (angle < closestAngle)
            {
                closestAngle = angle;
                closest = hit;
            }
        }

        if (closest == null) return aimDir;

        Vector2 toClosest = ((Vector2)closest.transform.position - (Vector2)transform.position).normalized;
        float strength = IsUsingGamepad ? _aimAssistStrengthGamepad : _aimAssistStrengthMouse;
        return Vector2.Lerp(aimDir, toClosest, strength).normalized;
    }
}