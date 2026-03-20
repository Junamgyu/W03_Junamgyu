using UnityEngine;

public class Player : MonoBehaviour
{

    public float moveSpeed = 5f;

    public float gravityOffDuration = 0.5f;
    public float dampingDuration = 0.1f;
    public float dampingValue = 8f;

    public float OriginalGravity { get; private set; }
    public bool IsRecoiling { get; set; } // 반동 상태
    public bool IsGrounded { get; set; } // 땅 여부 
    public bool HasAirRecoil { get; set; } // 공중 반동 상태

    public bool IsGravityOverridden { get; set; } // 샷건 or 주무기 반동 중에는 Jump에서 처리되는 중력 처리 꺼주기 위함.

    public PlayerAttack playerAttack{ get; private set; }
    public PlayerMove playerMove{ get; private set; }
    public PlayerAimer playerAimer { get; private set; }
    public PlayerJump playerJump { get; private set; }
    public PlayerHealth playerHealth { get; private set; }
    public DeadeyeSkill deadeyeSkill { get; private set; }

    Rigidbody2D _rb;


    void Awake()
    {
        playerAttack= GetComponent<PlayerAttack>(); // 샷건, 주무기 반동 담당
        playerMove= GetComponent<PlayerMove>(); // 좌우 이동 + 땅 여부 판단 담당
        playerAimer = GetComponent<PlayerAimer>(); 
        playerJump = GetComponent<PlayerJump>(); // 점프 담당
        playerHealth = GetComponent<PlayerHealth>();
        deadeyeSkill = GetComponent<DeadeyeSkill>();
        _rb = GetComponent<Rigidbody2D>();
        OriginalGravity = _rb.gravityScale;
    }

}
