using System.Collections;
using UnityEngine;

public class RushEnemy : NormalEnemyBase
{
    // =====================
    // 돌진 전용 변수
    // =====================
    [SerializeField] private float _rushSpeed = 100f;
    [SerializeField] private float _rushDuration = 0.5f;
    [SerializeField] private float _rushWindupTime = 0.5f;

    [SerializeField] private ParticleSystem _rushParticle;  // 돌진 파티클

    private bool _isRushing = false;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    // =====================
    // 생명주기
    // =====================
    protected override void Start()
    {
        base.Start();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
    }

    // =====================
    // 공격 (돌진)
    // =====================
    protected override void DoAttack()
    {
        StartCoroutine(nameof(RushRoutine));
    }

    IEnumerator RushRoutine()
    {
        _rb.linearVelocity = Vector2.zero;
        if (_rushParticle != null)
            _rushParticle.Play();

        // 색깔 점점 검게
        yield return StartCoroutine(WindupEffectRoutine());

        
        

        // 색깔 원래대로
        _spriteRenderer.color = _originalColor;

        // 돌진 시작
        _isRushing = true;
        Vector2 rushDir = ((Vector2)_player.position - (Vector2)transform.position).normalized;

        float t = 0;
        while (t < _rushDuration)
        {
            t += Time.deltaTime;
            _rb.linearVelocity = rushDir * _rushSpeed;
            yield return null;
        }

        _isRushing = false;
    }

    IEnumerator WindupEffectRoutine()
    {
        float t = 0;
        while (t < _rushWindupTime)
        {
            t += Time.deltaTime;
            // 0 → 1로 진행되면서 원래색 → 검정으로
            float ratio = t / _rushWindupTime;
            _spriteRenderer.color = Color.Lerp(_originalColor, Color.black, ratio);
            yield return null;
        }
    }

    // =====================
    // 충돌 처리
    // =====================
    protected override void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;

        if (_isRushing)
        {
            _isRushing = false;
            _rb.linearVelocity = Vector2.zero;
            _spriteRenderer.color = _originalColor;  // 색깔 복구
            StopCoroutine(nameof(RushRoutine));
        }
    }

    // =====================
    // 사망
    // =====================
    protected override IEnumerator OnDieRoutine()
    {
        yield return new WaitForSeconds(0.2f);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
    }
}