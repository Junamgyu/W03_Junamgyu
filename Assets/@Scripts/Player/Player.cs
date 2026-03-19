using UnityEngine;

public class Player : MonoBehaviour
{

    public float moveSpeed = 5f;

    public float gravityOffDuration = 0.5f;
    public float dampingDuration = 0.1f;
    public float dampingValue = 8f;

    public float OriginalGravity { get; private set; }

    public bool IsRecoiling { get; set; }
    public bool IsGrounded { get; set; }

    public PlayerAttack playerAttack{ get; private set; }
    public PlayerMove playerMove{ get; private set; }
    public PlayerController playerController { get; private set; }
    public PlayerAimer playerAimer { get; private set; }

    public DeadeyeSkill deadeyeSkill { get; private set; }

    Rigidbody2D _rb;


    void Awake()
    {
        playerAttack= GetComponent<PlayerAttack>();
        playerMove= GetComponent<PlayerMove>();
        playerController= GetComponent<PlayerController>();
        playerAimer = GetComponent<PlayerAimer>();
        deadeyeSkill = GetComponent<DeadeyeSkill>();

        _rb = GetComponent<Rigidbody2D>();
        OriginalGravity = _rb.gravityScale;
    }

}
