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
    [SerializeField] protected bool _isFlying = false;

    // =====================
    // 순찰
    // =====================
    [SerializeField] protected float _patrolRadius = 3f;
    [SerializeField] protected LayerMask _wallLayer;

    protected Vector2 _originalPos;
    protected Vector2 _patrolTarget;
    protected bool _isDead = false;
    protected bool _canAttack = true;
    protected bool _isAddGauge = false;
    protected bool _wasDetecting = false;

    protected Transform _player;

    // =====================
    // 생명주기
    // =====================
    protected override void Start()
    {
        base.Start();

        // Jaein 추가
        TryFindPlayer();

        _originalPos = transform.position;
        _patrolTarget = GetRandomPatrolTarget();
    }

    // Jaein 추가
    protected virtual void OnEnable()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = true;

        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;

        _isDead = false;
        _canAttack = true;
        _isAddGauge = false;
        _wasDetecting = false;
        _currentHp = _maxHp;

        _player = null;
        TryFindPlayer();

        _originalPos = transform.position;
        _patrolTarget = GetRandomPatrolTarget();

        ShowMark(false);
    }

    // Jaein 추가
    protected virtual void OnDisable()
    {
        _player = null;
        StopAllCoroutines();

        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;
    }

    // Jaein 추가
    protected bool TryFindPlayer()
    {
        if (_player != null)
            return true;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
            return false;

        _player = playerObj.transform;
        return _player != null;
    }

    protected virtual void Update()
    {
        if (_isDead) return;

        // Jaein 추가
        if (!TryFindPlayer())
        {
            _rb.linearVelocity = Vector2.zero;
            Patrol();
            return;
        }

        bool detecting = DetectPlayer();
        if (detecting)
        {
            _wasDetecting = true;

            if (IsInAttackRange())
            {
                _rb.linearVelocity = Vector2.zero;
                if (_canAttack)
                    StartCoroutine(AttackRoutine());
            }
            else
            {
                MoveToward(_player.position);
            }
        }
        else
        {
            if (_wasDetecting)
            {
                _wasDetecting = false;
                _originalPos = transform.position;
                _patrolTarget = GetRandomPatrolTarget();
            }

            Patrol();
        }
    }

    protected override void Initialize()
    {
        base.Initialize();
        ShowMark(false);
    }

    // =====================
    // 순찰
    // =====================
    protected void Patrol()
    {
        MoveToward(_patrolTarget);

        if (Vector2.Distance(transform.position, _patrolTarget) < 0.2f)
            _patrolTarget = GetRandomPatrolTarget();
    }

    protected Vector2 GetRandomPatrolTarget()
    {
        float randomX = Random.Range(-_patrolRadius, _patrolRadius);
        return new Vector2(_originalPos.x + randomX, _originalPos.y);
    }

    // =====================
    // 이동
    // =====================
    protected virtual void MoveToward(Vector2 target)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;

        if (_isFlying)
            _rb.linearVelocity = dir * _moveSpeed;
        else
            _rb.linearVelocity = new Vector2(dir.x * _moveSpeed, _rb.linearVelocity.y);
    }

    // =====================
    // 감지
    // =====================
    protected virtual bool DetectPlayer()
    {
        // Jaein 추가
        if (!TryFindPlayer())
            return false;

        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist > _detectionRange) return false;

        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, _wallLayer);
        return hit.collider == null;
    }

    protected bool IsInAttackRange()
    {
        // Jaein 추가
        if (!TryFindPlayer())
            return false;

        return Vector2.Distance(transform.position, _player.position) <= _attackRange;
    }

    // =====================
    // 전투
    // =====================
    public override void Die()
    {
        if (_isDead) return;
        _isDead = true;
        _rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        ShowMark(false);
        StartCoroutine(DieRoutine());
    }

    protected abstract void DoAttack();

    protected virtual IEnumerator AttackRoutine()
    {
        _canAttack = false;
        DoAttack();
        yield return new WaitForSeconds(_attackCooldown);
        _canAttack = true;
    }

    // =====================
    // 디버그
    // =====================
    protected virtual void OnDrawGizmosSelected()
    {
        Vector2 origin = Application.isPlaying ? _originalPos : (Vector2)transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(origin, new Vector3(_patrolRadius * 2f, 0.2f, 0f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        if (_player != null)
        {
            Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * _detectionRange);
        }
    }
}