using System.Collections;
using UnityEngine;

public class RangedEnemy : EnemyBase
{
    // =====================
    // 원거리 전용 변수
    // =====================
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _projectileSpeed = 8f;
    [SerializeField] private float _retreatRange = 2f;      // 이 거리보다 가까우면 후퇴
    [SerializeField] private float _preferredRange = 4f;    // 유지하려는 거리

    // =====================
    // OnEnter 오버라이드
    // =====================
    protected override void OnEnterChase()
    {
        // 원거리는 Chase 진입 시 거리 체크 후 후퇴 or 추격
    }

    // =====================
    // OnUpdate 오버라이드
    // =====================
    protected override void OnUpdateChase()
    {
        if (!DetectPlayer())
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distToPlayer <= _retreatRange)
        {
            // 너무 가까우면 뒤로 빠짐
            Vector2 retreatDir = ((Vector2)transform.position - (Vector2)_player.position).normalized;
            Move(retreatDir);
        }
        else if (distToPlayer <= _attackRange && _canAttack)
        {
            // 사거리 안이고 쿨타임 끝났으면 공격
            ChangeState(EnemyState.Attack);
        }
        else if (distToPlayer > _preferredRange)
        {
            // 너무 멀면 가까이 이동
            Vector2 chaseDir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            Move(chaseDir);
        }
        else
        {
            // 적정 거리면 멈추고 대기
            _rb.linearVelocity = Vector2.zero;
        }
    }

    // =====================
    // 공격
    // =====================
    protected override void DoAttack()
    {
        if (_projectilePrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: 투사체가 없습니다.");
            return;
        }

        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;

        GameObject projectile = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
        projectile.GetComponent<EnemyProjectile>().Initialize(dir, _projectileSpeed, _attackDamage);
    }

    // =====================
    // 사망 오버라이드 (예시)
    // =====================
    protected override IEnumerator OnDieRoutine()
    {
        // TODO: 사망 이펙트
        yield return new WaitForSeconds(0.3f);
    }
}