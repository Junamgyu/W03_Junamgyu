using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    Rigidbody2D _rb;
    Player player;
    Vector2 _dir;

    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckRadius = 0.1f;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
    }

    void FixedUpdate()
    {
        if (player.IsRecoiling) return;
        _rb.AddForce(new Vector2(_dir.x, 0) * player.moveSpeed, ForceMode2D.Impulse);

    }

    void Update()
    {
        bool isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);

        // 공중 → 착지 순간 감지
        if (isGrounded && !player.IsGrounded)
            player.playerAttack.ReloadAll();

        player.IsGrounded = isGrounded;
    }

    public void CanMove(Vector2 input)
    {
        _dir = input;
    }
}
