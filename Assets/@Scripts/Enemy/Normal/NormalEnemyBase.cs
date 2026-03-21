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

        _originalPos = transform.position;
        _patrolTarget = GetRandomPatrolTarget();
    }

    protected virtual void Update()
    {
        
        if (_isDead) return;

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
            // 감지 → 비감지 전환 순간 한 번만 갱신
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
        if (_player == null) return false;

        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist > _detectionRange) return false;

        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, _wallLayer);
        Debug.Log(hit.collider);
        return hit.collider == null;
    }

    protected bool IsInAttackRange()
    {
        return Vector2.Distance(transform.position, _player.position) <= _attackRange;
    }

    // =====================
    // 전투
    // =====================
    //public override void TakeDamage(int damage)
    //{
    //    if (_isDead) return;
    //    base.TakeDamage(damage);

    //    if (_currentHp <= 0)
    //        Die();
    //}

    //public override void TakeDamage(int damage, bool isAddGauge = false)
    //{
    //    _isAddGauge = isAddGauge;
    //    TakeDamage(damage);
    //}

    public override void Die()
    {
        if (_isDead) return;
        _isDead = true;
        _rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
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