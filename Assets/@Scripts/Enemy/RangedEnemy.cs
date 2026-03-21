using System.Collections;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

public class RangedEnemy : NormalEnemyBase
{
    // =====================
    // 원거리 전용 변수
    // =====================
    [SerializeField] protected GameObject _projectilePrefab;
    [SerializeField] protected float _projectileSpeed = 8f;
    [SerializeField] protected float _retreatRange = 2f;      // 이 거리보다 가까우면 후퇴
    [SerializeField] protected float _preferredRange = 4f;    // 유지하려는 거리
    [SerializeField] protected Transform _gunPivot;

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
        else if (distToPlayer > _preferredRange)
        {
            // 너무 멀면 가까이 이동
            Vector2 chaseDir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            Move(chaseDir);
        }

        if (_gunPivot != null)
        {
            Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            _gunPivot.rotation = rotation;
        }

        if (distToPlayer <= _attackRange && _canAttack)
        {
            // 사거리 안이고 쿨타임 끝났으면 공격
            ChangeState(EnemyState.Attack);
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
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        
        GameObject projectile = Instantiate(_projectilePrefab, transform.position, rotation);
        projectile.GetComponent<EnemyProjectile>().Initialize(_projectileSpeed, _attackDamage);
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