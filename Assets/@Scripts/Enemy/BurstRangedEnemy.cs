using System.Collections;
using UnityEngine;

public class BurstRangedEnemy : RangedEnemy
{
    // =====================
    // 삼점사 전용 변수
    // =====================
    [SerializeField] private int _burstCount = 3;           // 발사 횟수
    [SerializeField] private float _burstInterval = 0.15f;  // 발사 간격
    private Quaternion _rotation;

    // =====================
    // 삼점사 공격
    // =====================
    protected override void DoAttack()
    {
        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _rotation = Quaternion.Euler(0, 0, angle);
        StartCoroutine(nameof(BurstRoutine));
    }

    IEnumerator BurstRoutine()
    {
        for (int i = 0; i < _burstCount; i++)
        {
            FireProjectile();
            yield return new WaitForSeconds(_burstInterval);
        }
    }

    private void FireProjectile()
    {
        if (_projectilePrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: 투사체가 없습니다.");
            return;
        }

        GameObject projectile;

        if (_pool != null)
        {
            projectile = _pool.Get(_projectilePrefab, transform.position, _rotation);
        }
        else
        {
            projectile = Instantiate(_projectilePrefab, transform.position, _rotation);
        }

        projectile.GetComponent<EnemyProjectile>().Initialize(_projectileSpeed, _attackDamage);
    }
}