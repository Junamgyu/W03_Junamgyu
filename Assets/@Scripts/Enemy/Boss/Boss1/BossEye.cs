using System.Collections;
using UnityEngine;

public class BossEye : EnemyBase
{
    public enum EyeState { Idle, Laser, Dead }

    [Header("Eye 설정")]
    public float colorTransitionTime = 1f;
    public float blinkInterval = 0.1f;
    public int blinkCount = 4;

    [Header("레이저 설정")]
    public float laserRange = 40f;
    public float laserWidth = 1.2f;
    public float warningWidth = 0.15f;
    public float warningDuration = 0.8f;
    public int laserDamage = 10;

    public EyeState EyeCurrentState { get; private set; } = EyeState.Idle;
    public bool IsDead => EyeCurrentState == EyeState.Dead;
    public bool CanBeginLaser => EyeCurrentState == EyeState.Idle && !_isTransitioning;
    public float CurrentHp => _currentHp;

    private static readonly Color ColorIdle = Color.white;
    private static readonly Color ColorLaser = Color.red;
    private static readonly Color ColorDead = Color.black;
    private static readonly Color ColorHit = Color.yellow;

    private Renderer _rend;
    private LineRenderer _lineRenderer;
    private bool _isTransitioning;
    private bool _isBlinking;
    private Coroutine _transitionCoroutine;

    // =====================
    // EnemyBase 차단
    // =====================
    void Awake()
    {
        _rend = GetComponentInChildren<Renderer>();
        _currentHp = _maxHp;
        _rend.material.color = ColorIdle;
        SetupLineRenderer();
    }

    protected override void Start() { }
    protected override void Update() { }
    protected override void Initialize() { }

    protected override void OnEnterIdle() { }
    protected override void OnEnterPatrol() { }
    protected override void OnEnterChase() { }
    protected override void OnEnterAttack() { }
    protected override void OnEnterHit() { }
    protected override void OnEnterDead() { }

    protected override void OnUpdateIdle() { }
    protected override void OnUpdatePatrol() { }
    protected override void OnUpdateChase() { }
    protected override void OnUpdateAttack() { }

    protected override void Move(Vector2 direction) { }
    protected override bool DetectPlayer() => false;
    protected override void DoAttack() { }

    // =====================
    // LineRenderer 초기화
    // =====================
    void SetupLineRenderer()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.enabled = false;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = new Color(1f, 0.1f, 0.1f, 1f);
        _lineRenderer.endColor = new Color(1f, 0.1f, 0.1f, 1f);
        _lineRenderer.numCapVertices = 8;
        _lineRenderer.numCornerVertices = 8;
        _lineRenderer.alignment = LineAlignment.View;

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0f, 1f, 0f, 0f));
        curve.AddKey(new Keyframe(1f, 1f, 0f, 0f));
        _lineRenderer.widthCurve = curve;
        _lineRenderer.widthMultiplier = laserWidth;
    }

    // =====================
    // TakeDamage
    // =====================
    public override void TakeDamage(int damage)
    {
        if (IsDead) return;
        if (EyeCurrentState == EyeState.Laser) return;
        if (_isTransitioning) return;
        if (EyeCurrentState != EyeState.Idle) return;

        _currentHp -= damage;

        if (_currentHp <= 0)
        {
            _currentHp = 0;
            EyeDie();
            return;
        }

        if (!_isBlinking)
            StartCoroutine(HitBlinkRoutine());
    }

    public override void Die() => EyeDie();

    // =====================
    // Manager가 호출
    // =====================
    public void BeginLaser(float duration)
    {
        if (!CanBeginLaser) return;

        StartEyeTransition(EyeState.Laser, ColorLaser, () =>
        {
            StartCoroutine(LaserRoutine(duration));
        });
    }

    // =====================
    // 레이저 루틴
    // =====================
    IEnumerator LaserRoutine(float duration)
    {
        if (IsDead) yield break;

        _lineRenderer.enabled = true;

        // 1단계 : 예고선
        _lineRenderer.widthMultiplier = warningWidth;
        _lineRenderer.startColor = new Color(1f, 0.3f, 0.3f, 0.5f);
        _lineRenderer.endColor = new Color(1f, 0.3f, 0.3f, 0.5f);

        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            if (IsDead) { _lineRenderer.enabled = false; yield break; }
            UpdateLaserLine(dealDamage: false);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2단계 : 실제 발사
        _lineRenderer.widthMultiplier = laserWidth;
        _lineRenderer.startColor = new Color(1f, 0.1f, 0.1f, 1f);
        _lineRenderer.endColor = new Color(1f, 0.1f, 0.1f, 1f);

        elapsed = 0f;
        while (elapsed < duration)
        {
            if (IsDead) { _lineRenderer.enabled = false; yield break; }
            UpdateLaserLine(dealDamage: true);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _lineRenderer.enabled = false;

        if (!IsDead)
            StartEyeTransition(EyeState.Idle, ColorIdle);
    }

    void UpdateLaserLine(bool dealDamage)
    {
        Vector2 origin = transform.position;
        Vector2 direction = transform.up;

        int layerMask = ~LayerMask.GetMask("Enemy", "Ground");
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, laserRange, layerMask);

        Vector2 end = hit.collider != null
            ? hit.point
            : origin + direction * laserRange;

        _lineRenderer.SetPosition(0, origin);
        _lineRenderer.SetPosition(1, end);

        if (dealDamage && hit.collider != null && hit.collider.CompareTag("Player"))
            hit.collider.GetComponent<IDamageable>()?.TakeDamage(laserDamage);
    }

    // =====================
    // 내부 루틴
    // =====================
    void EyeDie()
    {
        if (IsDead) return;

        StopAllCoroutines();

        _lineRenderer.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        _transitionCoroutine = StartCoroutine(TransitionRoutine(EyeState.Dead, ColorDead, null));
    }

    void StartEyeTransition(EyeState next, Color target, System.Action onComplete = null)
    {
        if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
        _transitionCoroutine = StartCoroutine(TransitionRoutine(next, target, onComplete));
    }

    IEnumerator TransitionRoutine(EyeState next, Color target, System.Action onComplete)
    {
        _isTransitioning = true;
        Color from = _rend.material.color;
        float elapsed = 0f;

        while (elapsed < colorTransitionTime)
        {
            elapsed += Time.deltaTime;
            _rend.material.color = Color.Lerp(from, target, elapsed / colorTransitionTime);
            yield return null;
        }

        _rend.material.color = target;
        EyeCurrentState = next;
        _isTransitioning = false;
        onComplete?.Invoke();
    }

    IEnumerator HitBlinkRoutine()
    {
        _isBlinking = true;
        Color original = _rend.material.color;

        for (int i = 0; i < blinkCount; i++)
        {
            if (IsDead) { _isBlinking = false; yield break; }
            _rend.material.color = ColorHit;
            yield return new WaitForSeconds(blinkInterval);
            _rend.material.color = original;
            yield return new WaitForSeconds(blinkInterval);
        }

        _isBlinking = false;
    }
}