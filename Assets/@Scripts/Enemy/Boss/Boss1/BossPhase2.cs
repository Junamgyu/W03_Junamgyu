using System.Collections;
using UnityEngine;

public class BossPhase2 : EnemyBase
{
    [Header("맵 경계")]
    public Vector2 mapMin = new Vector2(-10f, -6f);
    public Vector2 mapMax = new Vector2(10f, 6f);

    [Header("회전 설정")]
    public float rotationSpeed = 30f;

    [Header("튀기 패턴")]
    public float bounceSpeed = 8f;
    public float bounceDuration = 6f;

    [Header("테두리 질주 패턴")]
    public float borderSpeed = 15f;
    public float borderLapCount = 1f;

    [Header("돌진 패턴")]
    public float dashBackDistance = 1.5f;
    public float dashBackSpeed = 2f;
    public float dashSpeed = 15f;
    public float dashDistance = 8f;
    public float dashCooldown = 1f;

    [Header("패턴 설정")]
    public float idleDuration = 2f;
    public float returnSpeed = 6f;
    public float phaseStartDelay = 1.5f;

    [Header("다음 스테이지 트리거")]
    [SerializeField] private GameObject _nextStageDoor;

    [Header("피격 연출")]
    [SerializeField] private int _hitFlashCount = 3;
    [SerializeField] private float _hitFlashInterval = 0.08f;

    private Vector2 _velocity;
    private bool _isActive = false;
    private Transform _player;

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private Coroutine _flashCoroutine;

    // =====================
    // 생명주기
    // =====================
    protected override void Start()
    {
        base.Start();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
    }

    void Update()
    {
        if (!_isActive) return;
        transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
    }

    // =====================
    // 피격
    // =====================
    public override void TakeDamage(int damage, bool isAddGauge = false)
    {
        if (!_isActive) return;
        if (!gameObject.activeInHierarchy) return;

        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        if (gameObject.activeInHierarchy)
            _flashCoroutine = StartCoroutine(HitFlashRoutine());

        base.TakeDamage(damage, isAddGauge); // 맨 마지막
    }

    private IEnumerator HitFlashRoutine()
    {
        for (int i = 0; i < _hitFlashCount; i++)
        {
            _spriteRenderer.color = Color.black;
            yield return new WaitForSeconds(_hitFlashInterval);
            _spriteRenderer.color = _originalColor;
            yield return new WaitForSeconds(_hitFlashInterval);
        }
    }

    public override void Die() => Phase2Die();

    // =====================
    // 페이즈2 시작
    // =====================
    public void SetPhase2()
    {
        _currentHp = _maxHp;
        _isActive = true;

        gameObject.tag = "Enemy";
        gameObject.layer = LayerMask.NameToLayer("Enemy");

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(phaseStartDelay);
        StartCoroutine(PatternCycleRoutine());
    }

    void Phase2Die()
    {
        if (!_isActive) return;
        _isActive = false;
        StopAllCoroutines();

        if (_spriteRenderer != null)
            _spriteRenderer.color = _originalColor;

        Debug.Log("보스 완전 사망");
        _nextStageDoor.SetActive(true);
        gameObject.SetActive(false);
    }

    // =====================
    // 패턴 사이클
    // =====================
    IEnumerator PatternCycleRoutine()
    {
        while (_isActive)
        {
            int pattern = Random.Range(0, 3);

            if (pattern == 0)
                yield return StartCoroutine(BouncePattern());
            else if (pattern == 1)
                yield return StartCoroutine(BorderRushPattern());
            else
                yield return StartCoroutine(DashPattern());

            yield return StartCoroutine(ReturnToCenter());
            yield return new WaitForSeconds(idleDuration);
        }
    }

    // =====================
    // 중앙 복귀
    // =====================
    IEnumerator ReturnToCenter()
    {
        Vector2 center = (mapMin + mapMax) * 0.5f;
        yield return StartCoroutine(MoveToPosition(center, returnSpeed));
    }

    // =====================
    // 튀기 패턴
    // =====================
    IEnumerator BouncePattern()
    {
        _velocity = GetSafeDirectionFromWall() * bounceSpeed;

        float elapsed = 0f;
        while (elapsed < bounceDuration)
        {
            Vector3 next = transform.position + (Vector3)_velocity * Time.deltaTime;

            if (next.x <= mapMin.x || next.x >= mapMax.x)
            {
                _velocity.x *= -1f;
                next.x = Mathf.Clamp(next.x, mapMin.x, mapMax.x);
            }

            if (next.y <= mapMin.y || next.y >= mapMax.y)
            {
                _velocity.y *= -1f;
                next.y = Mathf.Clamp(next.y, mapMin.y, mapMax.y);
            }

            transform.position = next;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // =====================
    // 테두리 질주 패턴
    // =====================
    IEnumerator BorderRushPattern()
    {
        bool clockwise = Random.value > 0.5f;

        Vector2 dir = GetSafeRandomDirection();
        Vector2 wallTarget = GetWallPoint(dir);
        yield return StartCoroutine(MoveToPosition(wallTarget, borderSpeed));

        Vector2[] corners = clockwise
            ? new Vector2[]
            {
                new Vector2(mapMin.x, mapMin.y),
                new Vector2(mapMin.x, mapMax.y),
                new Vector2(mapMax.x, mapMax.y),
                new Vector2(mapMax.x, mapMin.y),
            }
            : new Vector2[]
            {
                new Vector2(mapMin.x, mapMin.y),
                new Vector2(mapMax.x, mapMin.y),
                new Vector2(mapMax.x, mapMax.y),
                new Vector2(mapMin.x, mapMax.y),
            };

        int startIndex = 0;
        float minDist = float.MaxValue;
        for (int i = 0; i < corners.Length; i++)
        {
            float dist = Vector2.Distance(transform.position, corners[i]);
            if (dist < minDist)
            {
                minDist = dist;
                startIndex = i;
            }
        }

        int totalSteps = Mathf.RoundToInt(borderLapCount * corners.Length);
        for (int step = 0; step < totalSteps; step++)
        {
            int idx = (startIndex + step) % corners.Length;
            yield return StartCoroutine(MoveToPosition(corners[idx], borderSpeed));
        }
    }

    // =====================
    // 돌진 패턴
    // =====================
    IEnumerator DashPattern()
    {
        if (_player == null) yield break;

        float savedRotSpeed = rotationSpeed;
        rotationSpeed = 0f;

        Vector3 startPos = transform.position;
        Vector3 toPlayer = (_player.position - transform.position).normalized;

        Vector3 backTarget = startPos + (-toPlayer * dashBackDistance);
        yield return StartCoroutine(MoveToPosition(backTarget, dashBackSpeed));

        Vector3 dashTarget = startPos + (toPlayer * dashDistance);
        yield return StartCoroutine(MoveToPosition(dashTarget, dashSpeed));

        yield return new WaitForSeconds(dashCooldown);

        rotationSpeed = savedRotSpeed;
    }

    // =====================
    // 방향 유틸 - 플레이어 방향 ±30도 피하기
    // =====================
    private Vector2 GetSafeRandomDirection()
    {
        if (_player == null)
        {
            float r = Random.Range(0f, 360f);
            return new Vector2(Mathf.Cos(r * Mathf.Deg2Rad), Mathf.Sin(r * Mathf.Deg2Rad));
        }

        Vector2 toPlayer = (_player.position - transform.position).normalized;
        float playerAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
        float excludeAngle = 30f;

        float angle;
        int maxTry = 100;
        do
        {
            angle = Random.Range(0f, 360f);
            float diff = Mathf.Abs(Mathf.DeltaAngle(angle, playerAngle));
            if (diff > excludeAngle) break;
        } while (--maxTry > 0);

        return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    // =====================
    // 방향 유틸 - 벽 방향 피하기
    // =====================
    private Vector2 GetSafeDirectionFromWall()
    {
        Vector2 pos = transform.position;
        Vector2 center = (mapMin + mapMax) * 0.5f;
        Vector2 toCenter = (center - pos).normalized;
        float centerAngle = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg;

        float marginX = (mapMax.x - mapMin.x) * 0.2f;
        float marginY = (mapMax.y - mapMin.y) * 0.2f;
        bool nearWall = pos.x < mapMin.x + marginX || pos.x > mapMax.x - marginX
                     || pos.y < mapMin.y + marginY || pos.y > mapMax.y - marginY;

        if (nearWall)
        {
            float angle = centerAngle + Random.Range(-60f, 60f);
            return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        }

        return GetSafeRandomDirection();
    }

    // =====================
    // 유틸
    // =====================
    Vector2 GetWallPoint(Vector2 dir)
    {
        Vector2 pos = transform.position;
        float tMin = float.MaxValue;

        if (dir.x > 0) tMin = Mathf.Min(tMin, (mapMax.x - pos.x) / dir.x);
        else if (dir.x < 0) tMin = Mathf.Min(tMin, (mapMin.x - pos.x) / dir.x);

        if (dir.y > 0) tMin = Mathf.Min(tMin, (mapMax.y - pos.y) / dir.y);
        else if (dir.y < 0) tMin = Mathf.Min(tMin, (mapMin.y - pos.y) / dir.y);

        return pos + dir * tMin;
    }

    IEnumerator MoveToPosition(Vector2 target, float speed)
    {
        while (Vector2.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }
}