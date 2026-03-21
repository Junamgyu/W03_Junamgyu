using System.Collections;
using UnityEngine;

public class RushEnemy : NormalEnemyBase
{
    [SerializeField] private float _rushSpeed = 100f;
    [SerializeField] private float _rushDuration = 0.5f;
    [SerializeField] private float _rushWindupTime = 0.5f;
    [SerializeField] private ParticleSystem _rushParticle;

    private bool _isRushing = false;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private Coroutine _rushCoroutine;

    protected override void Start()
    {
        base.Start();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
    }

    protected override void Update()
    {
        if (_isDead) return;

        bool detecting = DetectPlayer();

        if (detecting)
        {
            _wasDetecting = true;

            // 돌진 중이 아닐 때는 항상 플레이어 추격
            if (!_isRushing)
                MoveToward(_player.position);

            // 공격 사거리 안이고 쿨타임 끝났으면 돌진
            if (IsInAttackRange() && _canAttack)
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

    // 돌진 끝날 때까지 canAttack 막음
    protected override IEnumerator AttackRoutine()
    {
        _canAttack = false;
        yield return StartCoroutine(RushRoutine());
        yield return new WaitForSeconds(_attackCooldown);
        _canAttack = true;
    }

    protected override void DoAttack() { } // AttackRoutine에서 직접 처리

    IEnumerator RushRoutine()
    {
        _rb.linearVelocity = Vector2.zero;

        if (_rushParticle != null)
            _rushParticle.Play();

        yield return StartCoroutine(WindupEffectRoutine());

        _spriteRenderer.color = _originalColor;

        _isRushing = true;
        Vector2 rushDir = ((Vector2)_player.position - (Vector2)transform.position).normalized;

        float t = 0f;
        while (t < _rushDuration)
        {
            t += Time.deltaTime;
            _rb.linearVelocity = rushDir * _rushSpeed;
            yield return null;
        }

        _isRushing = false;
        _rb.linearVelocity = Vector2.zero;
    }

    IEnumerator WindupEffectRoutine()
    {
        float t = 0f;
        while (t < _rushWindupTime)
        {
            t += Time.deltaTime;
            _spriteRenderer.color = Color.Lerp(_originalColor, Color.black, t / _rushWindupTime);
            yield return null;
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;
        if (!_isRushing) return;

        _isRushing = false;
        _rb.linearVelocity = Vector2.zero;
        _spriteRenderer.color = _originalColor;
    }

    protected override IEnumerator OnDieRoutine()
    {
        yield return new WaitForSeconds(0.2f);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
    }


}

