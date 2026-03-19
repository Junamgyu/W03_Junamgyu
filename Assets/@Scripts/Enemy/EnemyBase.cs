using System.Collections;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    // =====================
    // 스탯
    // =====================
    [SerializeField] protected float _maxHp = 100f;
    [SerializeField] protected float _moveSpeed = 3f;
    [SerializeField] protected float _attackDamage = 10f;
    [SerializeField] protected float _attackRange = 1.5f;
    [SerializeField] protected float _detectionRange = 5f;
    [SerializeField] protected float _attackCooldown = 1.5f;
    [SerializeField] protected float _knockBackForce = 5f;
    [SerializeField] protected float _hitStunDuration = 0.2f;
    [SerializeField] protected float _attackMotionDuration = 0.3f;

    protected float _currentHp;
    protected bool _canAttack = true;

    // =====================
    // 상태
    // =====================
    public enum EnemyState { Idle, Patrol, Chase, Attack, Hit, Dead }
    protected EnemyState _currentState;

    // =====================
    // 순찰
    // =====================
    [SerializeField] protected float _patrolDistance = 3f;
    [SerializeField] protected float _idleWaitTime = 2f;
    protected float _idleTimer;
    protected Vector2 _patrolTarget;
    protected bool _isPatrolRight = true;

    // =====================
    // 마킹
    // =====================
    [SerializeField] private GameObject _markIndicator;
    private bool _isMarked = false;

    // =====================
    // 참조
    // =====================
    protected Rigidbody2D _rb;
    protected Transform _player;

    // =====================
    // 감지
    // =====================
    [SerializeField] protected LayerMask _excludeLayer;  // Enemy 레이어 제외용

    // =====================
    // 생명주기
    // =====================
    protected virtual void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Player를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        Initialize();
    }

    protected virtual void Update()
    {
        UpdateState();
    }

    protected virtual void Initialize()
    {
        _currentHp = _maxHp;
        ShowMark(false);
        ChangeState(EnemyState.Idle);
    }

    // =====================
    // 상태 관리
    // =====================
    protected void ChangeState(EnemyState newState)
    {
        if (_currentState == EnemyState.Dead) return;

        _currentState = newState;

        switch (newState)
        {
            case EnemyState.Idle: OnEnterIdle(); break;
            case EnemyState.Patrol: OnEnterPatrol(); break;
            case EnemyState.Chase: OnEnterChase(); break;
            case EnemyState.Attack: OnEnterAttack(); break;
            case EnemyState.Hit: OnEnterHit(); break;
            case EnemyState.Dead: OnEnterDead(); break;
        }
    }

    protected void UpdateState()
    {
        switch (_currentState)
        {
            case EnemyState.Idle: OnUpdateIdle(); break;
            case EnemyState.Patrol: OnUpdatePatrol(); break;
            case EnemyState.Chase: OnUpdateChase(); break;
            case EnemyState.Attack: OnUpdateAttack(); break;
        }
    }

    // =====================
    // OnEnter
    // =====================
    protected virtual void OnEnterIdle()
    {
        _rb.linearVelocity = Vector2.zero;
        _idleTimer = _idleWaitTime;
    }

    protected virtual void OnEnterPatrol()
    {
        float dir = _isPatrolRight ? 1f : -1f;
        _patrolTarget = (Vector2)transform.position + new Vector2(_patrolDistance * dir, 0);
        _isPatrolRight = !_isPatrolRight;
    }

    protected virtual void OnEnterChase() { }

    protected virtual void OnEnterAttack()
    {
        _rb.linearVelocity = Vector2.zero;
        StopCoroutine(nameof(AttackRoutine));
        StartCoroutine(nameof(AttackRoutine));
    }

    protected virtual void OnEnterHit()
    {
        StopCoroutine(nameof(HitRoutine));
        StartCoroutine(nameof(HitRoutine));
    }

    protected virtual void OnEnterDead()
    {
        StopAllCoroutines();
        ShowMark(false);
        StartCoroutine(nameof(DieRoutine));
    }

    // =====================
    // OnUpdate
    // =====================
    protected virtual void OnUpdateIdle()
    {
        _idleTimer -= Time.deltaTime;

        if (DetectPlayer())
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        if (_idleTimer <= 0f)
        {
            ChangeState(EnemyState.Patrol);
        }
    }

    protected virtual void OnUpdatePatrol()
    {
        if (DetectPlayer())
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        Move((_patrolTarget - (Vector2)transform.position).normalized);

        if (Mathf.Abs(transform.position.x - _patrolTarget.x) < 0.2f)
        {
            ChangeState(EnemyState.Idle);
        }
    }

    protected virtual void OnUpdateChase()
    {
        if (!DetectPlayer())
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        if (IsInAttackRange() && _canAttack)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        Move(dir);
    }

    protected virtual void OnUpdateAttack() { }

    // =====================
    // 이동
    // =====================
    protected virtual void Move(Vector2 direction)
    {
        _rb.linearVelocity = new Vector2(direction.x * _moveSpeed, _rb.linearVelocity.y);
        Flip(direction.x);
    }

    protected virtual void Flip(float dirX)
    {
        if (dirX > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (dirX < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    // =====================
    // 감지
    // =====================
    protected virtual bool DetectPlayer()
    {
        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist > _detectionRange) return false;

        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, _detectionRange, ~_excludeLayer);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
            return true;

        return false;
    }

    protected bool IsInAttackRange()
    {
        return Vector2.Distance(transform.position, _player.position) <= _attackRange;
    }

    // =====================
    // 마킹
    // =====================
    public void ShowMark(bool show)
    {
        if (_markIndicator == null) return;
        _isMarked = show;
        _markIndicator.SetActive(show);
    }

    public bool IsMarked() => _isMarked;

    // =====================
    // 전투
    // =====================
    public virtual void TakeDamage(float damage, Vector2 knockBackDirection = default)
    {
        if (_currentState == EnemyState.Dead) return;

        _currentHp -= damage;

        if (knockBackDirection != default)
            KnockBack(knockBackDirection);

        if (_currentHp <= 0f)
            ChangeState(EnemyState.Dead);
        else
            ChangeState(EnemyState.Hit);
    }

    protected virtual void KnockBack(Vector2 direction)
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(direction.normalized * _knockBackForce, ForceMode2D.Impulse);
    }

    protected abstract void DoAttack();

    // 자식에서 드랍, 이펙트 등 override
    protected virtual IEnumerator OnDieRoutine()
    {
        yield break;
    }

    // =====================
    // 코루틴
    // =====================
    protected virtual IEnumerator AttackRoutine()
    {
        DoAttack();
        yield return new WaitForSeconds(_attackMotionDuration);
        StartCoroutine(nameof(AttackCooldownRoutine));
        ChangeState(EnemyState.Chase);
    }

    protected IEnumerator AttackCooldownRoutine()
    {
        _canAttack = false;
        yield return new WaitForSeconds(_attackCooldown);
        _canAttack = true;
    }

    protected virtual IEnumerator HitRoutine()
    {
        yield return new WaitForSeconds(_hitStunDuration);
        if (_currentState != EnemyState.Dead)
            ChangeState(EnemyState.Chase);
    }

    private IEnumerator DieRoutine()
    {
        yield return StartCoroutine(OnDieRoutine());
        Destroy(gameObject);
    }

    // =====================
    // 에디터 디버그
    // =====================
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}