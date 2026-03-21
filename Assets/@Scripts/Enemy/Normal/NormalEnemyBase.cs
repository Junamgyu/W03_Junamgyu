using System;
using System.Collections;
using UnityEngine;

public abstract class NormalEnemyBase : EnemyBase
{
    // =====================
    // 스탯
    // =====================
    [SerializeField] protected float _moveSpeed = 5f;
    [SerializeField] protected float _attackRange = 1.5f;
    [SerializeField] protected float _detectionRange = 5f;
    [SerializeField] protected float _attackCooldown = 1.5f;
    [SerializeField] protected float _hitStunDuration = 0.2f;
    [SerializeField] protected float _attackMotionDuration = 0.3f;
    [SerializeField] protected bool _isFlying = false;

    protected bool _canAttack = true;
    protected bool isAddGauge = false;

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
    // 참조
    // =====================
    protected Transform _player;
    [SerializeField] protected LayerMask _excludeLayer;

    // =====================
    // 생명주기
    // =====================
    protected override void Start()
    {
        base.Start();

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
    }

    protected virtual void Update()
    {
        Debug.Log(_currentState);
        UpdateState();
    }

    protected override void Initialize()
    {
        base.Initialize();
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
        GetComponent<Collider2D>().enabled = false;
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
            ChangeState(EnemyState.Patrol);
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
            ChangeState(EnemyState.Idle);
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
        if (_isFlying)
            _rb.linearVelocity = direction * _moveSpeed;
        else
            _rb.linearVelocity = new Vector2(direction.x * _moveSpeed, _rb.linearVelocity.y);
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

        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    protected bool IsInAttackRange()
    {
        return Vector2.Distance(transform.position, _player.position) <= _attackRange;
    }

    // =====================
    // 전투
    // =====================
    public override void TakeDamage(int damage, bool isAddGauge = false)
    {
        if (_currentState == EnemyState.Dead) return;
        base.TakeDamage(damage);
        this.isAddGauge = isAddGauge;
        ChangeState(_currentHp > 0f ? EnemyState.Hit : EnemyState.Dead);
    }

    public override void TakeDamage(int damage)
    {
        if (_currentState == EnemyState.Dead) return;
        base.TakeDamage(damage);
        this.isAddGauge = false;
        ChangeState(_currentHp > 0f ? EnemyState.Hit : EnemyState.Dead);
    }

    public override void Die() => ChangeState(EnemyState.Dead);

    protected abstract void DoAttack();

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

    protected virtual void OnCollisionEnter2D(Collision2D col) { }

    // =====================
    // 디버그
    // =====================
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}