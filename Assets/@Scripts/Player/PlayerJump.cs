using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [Header("Jump Stats")]
    [Tooltip("원하는 점프 높이")][SerializeField] private float _jumpHeight = 4f;
    [Tooltip("정점까지 도달하는데 걸리는 시간")][SerializeField] private float _timeToJumpApex = 0.4f;
    [Tooltip("점프 버튼 누르고 있을 시 중력 적용 배율")][SerializeField] private float _upwardMultiplier = 1f;
    [Tooltip("떨어질 때의 낙하 가속 배율")][SerializeField] private float _downwardMultiplier = 3f;
    [Tooltip("점프 버튼 땔 시 중력 적용 배율")][SerializeField] private float _jumpCutOff = 2f;       // 버튼 떼면 이 배율로 전환
    [SerializeField] private float _speedLimit = 20f;

    [SerializeField] private float _coyoteTime = 0.15f;

    Rigidbody2D _rb;
    Player _player;

    // 점프
    bool _desiredJump; // 점프 실행해줘
    bool _pressingJump; // 점프 버튼 누르는 중이야
    bool _currentlyJumping; // 점프 중
    
    // 중력
    float _gravMultiplier = 1f;

    // 코요테
    float _coyoteTimeCounter;

    // TODO: 점프 버퍼 변수 자리
    // float _jumpBuffer = 0.15f;
    // float _jumpBufferCounter;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _player = GetComponent<Player>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // 공중 반동 상태면 점프 불가
        if (_player.HasAirRecoil) return;

        if (context.started)
        {
            // 지상이거나 코요테 타임 안이면 점프 가능
            bool canJump = _player.IsGrounded || (!_currentlyJumping && _coyoteTimeCounter < _coyoteTime);
            if (!canJump) return;
            _desiredJump = true;
            _pressingJump = true;
            _coyoteTimeCounter = _coyoteTime; // 코요테 타임 즉시 소모 (코요테 타임 중에 점프하면 카운터를 한계값으로 올려서 2번 쓰는 것 방지)
        }
        if (context.canceled)
            _pressingJump = false;
    }

    void FixedUpdate()
    {
        if (_player.IsGravityOverridden) return; // Attack의 GravityRoutine이 관할 중이면 스킵

        // 코요테 타임 카운터
        if (!_player.IsGrounded && !_currentlyJumping)
            _coyoteTimeCounter += Time.fixedDeltaTime;
        else if (_player.IsGrounded)  // 지상일 때만 리셋
            _coyoteTimeCounter = 0f;

        ApplyGravity();

        if (_desiredJump)
        {
            DoJump();
            return; // 이 프레임은 중력 재계산 스킵(중요!)
        }
    }

    void ApplyGravity()
    {

        // 중력 배율 결정
        // 올라가는 중
        if (_rb.linearVelocity.y > 0.01f)
        {
            if (_player.IsGrounded)
                _gravMultiplier = 1f; // 무빙 플랫폼 위 - 기본값 유지
            else if (_pressingJump && _currentlyJumping)
                _gravMultiplier = _upwardMultiplier; // 버튼 누른 채 상승
            else
                _gravMultiplier = _jumpCutOff;  // 버튼 떼면 빠르게 꺾임
        }

        // 내려가는 중
        else if (_rb.linearVelocity.y < -0.01f)
        {
            _gravMultiplier = _player.IsGrounded ? 1f : _downwardMultiplier;
        }
        // 거의 정지
        else
        {
            if (_player.IsGrounded) _currentlyJumping = false;
            _gravMultiplier = 1f;
        }

        // 중력 스케일 적용
        // PlayerAttack의 GravityRoutine과 충돌 방지:
        // IsRecoiling 중엔 gravityScale을 건드리지 않음
        if (!_player.IsRecoiling)
        {
            // 원하는 점프 높이와 정점 도달 시간으로부터 필요한 중력을 역산하는 로직
            Vector2 newGravity = new Vector2(0, (-2f * _jumpHeight) / (_timeToJumpApex * _timeToJumpApex));
            _rb.gravityScale = (newGravity.y / Physics2D.gravity.y) * _gravMultiplier;
        }

        // Y속도 상한 (터미널 속도)
        _rb.linearVelocity = new Vector2(
            _rb.linearVelocity.x,
            Mathf.Clamp(_rb.linearVelocity.y, -_speedLimit, 100f)
        );
    }

    void DoJump()
    {
        _desiredJump = false;
        _gravMultiplier = 1f;
        Vector2 newGravity = new Vector2(0, (-2f * _jumpHeight) / (_timeToJumpApex * _timeToJumpApex));
        _rb.gravityScale = (newGravity.y / Physics2D.gravity.y) * _gravMultiplier;

        float jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * _rb.gravityScale * _jumpHeight);

        // 코요테 타임 점프면 Y속도 초기화 후 순수 점프 속도만 적용
        if (!_player.IsGrounded)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
        }
        else if (_rb.linearVelocity.y > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - _rb.linearVelocity.y, 0f);
        }

        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y + jumpSpeed);
        _currentlyJumping = true;

    }
}
