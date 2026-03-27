using UnityEngine;
//using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMove : MonoBehaviour
{
    Rigidbody2D _rb;
    Player _player;
    Vector2 _dir;

    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckRadius = 0.1f;

    [Tooltip("공중 반동아닐 시의 좌우 이동 저항")][SerializeField] private float _recoilMoveInfluence = 0.3f; 
    [Tooltip("공중 반동일 시의 좌우 이동 저항")][SerializeField] private float _airRecoilMoveInfluence = 0.1f; 

    [Tooltip("방패 중 이동속도 배율")][SerializeField] private float _shieldMoveMultiplier = 0.4f;

    private float _landTimer = 0f;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _player = GetComponent<Player>();
        _player.OnRecoilStateChanged += HandleRecoilStateChanged;
    }

    void OnDestroy()
    {
        _player.OnRecoilStateChanged -= HandleRecoilStateChanged;
    }

    void HandleRecoilStateChanged(RecoilState state)
    {
        // 반동이 끝났는데 여전히 땅이면 재장전
        /*
        if (state == RecoilState.None && _player.IsGrounded)
            _player.playerAttack.ReloadAll();
        */
    }

    void FixedUpdate()
    {
        float speed = _player.moveSpeed;
        if(_player.IsShieldOn) speed *= _shieldMoveMultiplier;

        if (_player.CurrentRecoil == RecoilState.Recoiling && !_player.IsGrounded)
        {
            float newX = _rb.linearVelocity.x + speed * _dir.x * _airRecoilMoveInfluence * Time.fixedDeltaTime;
            _rb.linearVelocity = new Vector2(newX, _rb.linearVelocityY);
            return;
        }

        if (_player.CurrentRecoil == RecoilState.Recoiling && _player.IsGrounded)
        {
            float newX = _rb.linearVelocity.x + speed * _dir.x * _recoilMoveInfluence * Time.fixedDeltaTime;
            _rb.linearVelocity = new Vector2(newX, _rb.linearVelocityY);
            return;
        }

        _rb.linearVelocity = new Vector2(speed * _dir.x, _rb.linearVelocityY);

    }

    void Update()
    {
        bool isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);

        // 공중 → 착지 순간 감지
        if (isGrounded && !_player.IsGrounded)
        {
            //_player.playerAttack.ReloadAll(); // 무기 전체 재장전
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
