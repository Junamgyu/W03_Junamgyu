using System.Collections;
using UnityEngine;

public class RangedEnemy : NormalEnemyBase
{
    [SerializeField] protected GameObject _projectilePrefab;
    [SerializeField] protected float _projectileSpeed = 8f;
    [SerializeField] protected float _retreatRange = 2f;
    [SerializeField] protected float _preferredRange = 4f;
    [SerializeField] protected Transform _gunPivot;

    [Header("연사 설정")]
    [SerializeField] private int _burstCount = 1;
    [SerializeField] private float _burstInterval = 0.15f;

    protected PoolManager _pool;

    private void Awake()
    {
        ManagerRegistry.TryGet(out _pool);
    }

    private bool _isBursting = false;

    // Jaein 추가
    protected override void OnEnable()
    {
        base.OnEnable();
        _isBursting = false;
    }

    protected override void Update()
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

            float dist = Vector2.Distance(transform.position, _player.position);

            if (_gunPivot != null && !_isBursting)
            {
                Vector2 aimDir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
                _gunPivot.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg);
            }

            if (!_isBursting)
            {
                if (dist <= _retreatRange)
                {
                    Vector2 retreatDir = ((Vector2)transform.position - (Vector2)_player.position).normalized;
                    MoveToward((Vector2)transform.position + retreatDir);
                }
                else if (dist > _preferredRange)
                {
                    MoveToward(_player.position);
                }
                else
                {
                    _rb.linearVelocity = Vector2.zero;
                }
            }
            else
            {
                _rb.linearVelocity = Vector2.zero;
            }

            if (dist <= _attackRange && _canAttack)
                StartCoroutine(AttackRoutine());
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

    protected override void DoAttack()
    {
        // Jaein 추가
        if (!TryFindPlayer())
            return;

        if (_projectilePrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: 투사체가 없습니다.");
            return;
        }

        // Jaein 추가
        StopCoroutine(nameof(BurstRoutine));
        StartCoroutine(nameof(BurstRoutine));
    }

    IEnumerator BurstRoutine()
    {
        _isBursting = true;
        _rb.linearVelocity = Vector2.zero;

        // Jaein 추가
        if (!TryFindPlayer())
        {
            _isBursting = false;
            yield break;
        }

        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        Quaternion rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        for (int i = 0; i < _burstCount; i++)
        {
            GameObject projectile;

            if (_pool != null)
            {
                projectile = _pool.Get(_projectilePrefab, transform.position, rotation);
            }
            else
            {
                projectile = Instantiate(_projectilePrefab, transform.position, rotation);
            }

            projectile.GetComponent<EnemyProjectile>().Initialize(_projectileSpeed, _attackDamage);

            if (i < _burstCount - 1)
                yield return new WaitForSeconds(_burstInterval);
        }

        _isBursting = false;
    }

    protected override IEnumerator OnDieRoutine()
    {
        yield return new WaitForSeconds(0.3f);
    }
}