using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMove : MonoBehaviour
{
    Rigidbody2D _rb;
    Player _player;
    Vector2 _dir;

    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckRadius = 0.1f;

    private float _landTimer = 0f;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _player = GetComponent<Player>();
    }

    void FixedUpdate()
    {
        if (_player.IsRecoiling) return; // 반동 받는 동안 못 움직임.
        _rb.linearVelocity = new Vector2(_player.moveSpeed * _dir.x, _rb.linearVelocityY);

    }

    void Update()
    {
        bool isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);

        // 공중 → 착지 순간 감지
        if (isGrounded && !_player.IsGrounded)
        {
            _player.playerAttack.ReloadAll(); // 무기 전체 재장전
            _player.CanJump = true;
            _player.SetLocomotionState(LocomotionState.Land);
            _landTimer = 0f;
        }

        // Land 타이머
        if (_player.CurrentLocomotion == LocomotionState.Land)
        {
            _landTimer += Time.deltaTime;
            if (_landTimer >= _player.landDuration)
            {
                _player.SetLocomotionState(LocomotionState.Idle);
            }
            return; // Land 중엔 아래 로직 스킵
        }

        // LocomotionState 세팅
        if (isGrounded)
        {
            _player.SetLocomotionState(LocomotionState.Idle);
        }
        else
        {
            if (_rb.linearVelocity.y > 0.01f)
                _player.SetLocomotionState(LocomotionState.Jumping);
            else
                _player.SetLocomotionState(LocomotionState.Falling);
        }

    }

    public void CanMove(Vector2 input)
    {
        _dir = input;
    }
}
