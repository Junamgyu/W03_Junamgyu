using System.Collections;
using UnityEngine;

public class RushEnemy : EnemyBase
{
    // =====================
    // 돌진 전용 변수
    // =====================
    [SerializeField] private float _rushSpeed = 100f;
    [SerializeField] private float _rushDuration = 0.5f;
    [SerializeField] private float _rushWindupTime = 0.5f;  // 돌진 전 예고 시간

    private bool _isRushing = false;

    // =====================
    // 공격 (돌진)
    // =====================
    protected override void DoAttack()
    {
        StartCoroutine(nameof(RushRoutine));
    }

    IEnumerator RushRoutine()
    {
        // 돌진 예고 (멈추고 잠깐 대기)
        _rb.linearVelocity = Vector2.zero;
        // TODO: 예고 이펙트
        yield return new WaitForSeconds(_rushWindupTime);
        Debug.Log("Rush!");
        // 돌진 시작
        _isRushing = true;
        Vector2 rushDir = ((Vector2)_player.position - (Vector2)transform.position).normalized;

        float t = 0;
        while (t < _rushDuration)
        {
            t += Time.deltaTime;
            _rb.linearVelocity = new Vector2(rushDir.x * _rushSpeed, _rb.linearVelocity.y);
            yield return null;
        }

        _isRushing = false;
    }

    // =====================
    // 충돌 처리
    // =====================
    protected override void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;

        //col.gameObject.GetComponent<IDamageable>().TakeDamage(_attackDamage);

        // 돌진 중이면 충돌하는 순간 돌진 종료
        if (_isRushing)
        {
            _isRushing = false;
            _rb.linearVelocity = Vector2.zero;
            StopCoroutine(nameof(RushRoutine));
        }
    }

    // =====================
    // 사망
    // =====================
    protected override IEnumerator OnDieRoutine()
    {
        // TODO: 사망 이펙트
        yield return new WaitForSeconds(0.2f);
    }

    // =====================
    // 디버그
    // =====================
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
    }
}