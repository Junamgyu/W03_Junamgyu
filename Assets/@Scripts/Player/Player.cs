using UnityEngine;

public enum ActionState { Idle, Jumping, Recoiling, Deadeye, Slow }
public enum GroundState { Grounded, Airborne } // 오르는 상태, 떨어지는 상태 같은 것으로 구체화?

public class Player : MonoBehaviour
{

    public float moveSpeed = 5f;
    public float gravityOffDuration = 0.5f;
    public float dampingDuration = 0.1f;
    public float dampingValue = 8f;
    public float OriginalGravity { get; private set; }

    public ActionState CurrentAction { get; private set; } = ActionState.Idle;
    public GroundState CurrentGround { get; private set; } = GroundState.Airborne; // TODO: 캐릭터가 정확히 땅에서 시작하면 수정 필요.
    public bool CanJump { get; set; } = true; // 샷건 반동을 받고 조금이라도 떠 있으면 막힘, 2단 점프 없음.

    public void SetActionState(ActionState state) => CurrentAction = state;
    public void SetGroundState(GroundState state) => CurrentGround = state;

    // 편의 프로퍼티
    public bool IsGrounded => CurrentGround == GroundState.Grounded;
    public bool IsRecoiling => CurrentAction == ActionState.Recoiling;

    public PlayerAttack playerAttack{ get; private set; }
    public PlayerMove playerMove{ get; private set; }
    public PlayerAimer playerAimer { get; private set; }
    public PlayerJump playerJump { get; private set; }
    public PlayerHealth playerHealth { get; private set; }
    public DeadeyeSkill deadeyeSkill { get; private set; }

    Rigidbody2D _rb;


    void Awake()
    {
        playerAttack= GetComponent<PlayerAttack>(); // 샷건, 주무기 반동 담당 (반동 시 중력 제어 추가 됨)
        playerMove= GetComponent<PlayerMove>(); // 좌우 이동 + 땅 여부 판단 담당
        playerAimer = GetComponent<PlayerAimer>(); 
        playerJump = GetComponent<PlayerJump>(); // 점프 + 올라갈 때, 내려갈 때의 중력 담당
        playerHealth = GetComponent<PlayerHealth>();
        deadeyeSkill = GetComponent<DeadeyeSkill>();
        _rb = GetComponent<Rigidbody2D>();
        OriginalGravity = _rb.gravityScale;
    }

}
