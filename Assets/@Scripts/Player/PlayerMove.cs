using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMove : MonoBehaviour
{
    Rigidbody2D _rb;
    Player _player;
    Vector2 _dir;

    // 이게 여기에 있는게 맞을까???
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckRadius = 0.1f;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _player = GetComponent<Player>();
    }

    void FixedUpdate()
    {
        // TODO 반동으로 인해 뜨게 되었을 경우 IsRecoiling이 짧기도 하고 HasAirRecoil가 true가 되어야 하는데 안됨
        //Debug.Log($"IsRecoiling => {_player.IsRecoiling},  HasAirRecoil => {_player.HasAirRecoil}");

        if (_player.IsRecoiling) return;

        _rb.linearVelocity = new Vector2(_player.moveSpeed * _dir.x, _rb.linearVelocityY);

    }

    void Update()
    {
        bool isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);

        // 공중 → 착지 순간 감지
        if (isGrounded && !_player.IsGrounded)
        {
            _player.jaein_playerAttack.ReloadAll(); // 무기 전체 재장전
            _player.HasAirRecoil = false; // 공중 반동 상태 초기화
        }
            
        _player.IsGrounded = isGrounded;
    }

    public void CanMove(Vector2 input)
    {
        _dir = input;
    }
}
