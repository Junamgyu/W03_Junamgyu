using System.Collections;
using UnityEngine;

public class SuicideEnemy : EnemyBase
{
    // =====================
    // 자폭 전용 변수
    // =====================
    [SerializeField] private int _explosionDamage = 30;
    [SerializeField] private float _explosionRange = 2f;
    [SerializeField] private float _explosionWindupTime = 3f;  // 자폭 예고 시간
    [SerializeField] private ParticleSystem _explosionParticle;
    // =====================
    // 공격 (자폭 준비)
    // =====================
    protected override void DoAttack()
    {
        StartCoroutine(nameof(ExplosionRoutine));
    }

    IEnumerator ExplosionRoutine()
    {
        _rb.linearVelocity = Vector2.zero;

        // 깜빡임
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        float elapsed = 0f;
        float blinkInterval = 0.3f;

        while (elapsed < _explosionWindupTime)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(blinkInterval);
            sr.color = Color.white;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval * 2;
            blinkInterval = Mathf.Max(0.05f, blinkInterval - 0.03f); // 점점 빨라짐
        }

        Explode();
    }

    void Explode()
    {
        if (_explosionParticle != null)
        {
            ParticleSystem particle = Instantiate(_explosionParticle, transform.position, Quaternion.identity);
            particle.transform.localScale = Vector3.one * _explosionRange; // range에 맞게 스케일
        }
        // 자폭 범위 안 플레이어 감지
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            _explosionRange,
            LayerMask.GetMask("Player")
        );

        if (hit != null)
            hit.GetComponent<IDamageable>().TakeDamage(_explosionDamage);

        ChangeState(EnemyState.Dead);
    }

    // =====================
    // 충돌 시 즉시 자폭
    // =====================
    protected override void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;

        StopCoroutine(nameof(ExplosionRoutine));
        Explode();
    }

    // =====================
    // 사망
    // =====================
    protected override IEnumerator OnDieRoutine()
    {
        // TODO: 사망 이펙트
        yield return new WaitForSeconds(0.3f);
    }

    // =====================
    // 디버그
    // =====================
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _explosionRange);
    }
}