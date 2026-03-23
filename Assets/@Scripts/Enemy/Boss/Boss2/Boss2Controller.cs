using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2Controller : EnemyBase
{
    [Header("스킬")]
    public MonoBehaviour[] skills;

    [Header("패턴 설정")]
    public float idleDuration = 2f;
    public float phaseStartDelay = 1.5f;
    public float returnSpeed = 8f;

    [Header("피격 설정")]
    public float blinkInterval = 0.1f;
    public int blinkCount = 3;

    [Header("다음 스테이지 트리거")]
    [SerializeField] private GameObject _nextStageDoor;

    private bool _isActive = false;
    private List<ISkill> _skills = new List<ISkill>();
    private Vector3 _originalPos;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor; // 추가
    private Coroutine _blinkCoroutine;

    private void OnEnable()
    {
        CameraManager.OnBossOutro -= StartBoss2; // 중복 방지
        CameraManager.OnBossOutro += StartBoss2;
    }

    private void OnDisable()
    {
        CameraManager.OnBossOutro -= StartBoss2;
    }

    protected override void Initialize()
    {
        _currentHp = _maxHp;
    }

    void Start()
    {
        Initialize();
        _originalPos = transform.position;
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color; // 원본 색 저장

        foreach (var skill in skills)
        {
            if (skill is ISkill s)
                _skills.Add(s);
            else
                Debug.LogWarning($"{skill.name}은 ISkill을 구현하지 않았습니다.");
        }
    }

    public override void Die() => Boss2Die();

    public override void TakeDamage(int damage)
    {
        if (!_isActive) return;
        if (!gameObject.activeInHierarchy) return;
        _currentHp -= damage;

        if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = StartCoroutine(HitBlinkRoutine());

        if (_currentHp <= 0)
        {
            _currentHp = 0;
            Boss2Die();
        }
    }

    public override void TakeDamage(int damage, bool isAddGauge = false)
    {
        if (!_isActive) return;
        if (!gameObject.activeInHierarchy) return;
        _currentHp -= damage;

        if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = StartCoroutine(HitBlinkRoutine());

        if (_currentHp <= 0)
        {
            _currentHp = 0;
            Boss2Die();
        }
    }

    void Boss2Die()
    {
        if (!_isActive) return;
        _isActive = false;
        StopAllCoroutines();

        if (_spriteRenderer != null)
            _spriteRenderer.color = _originalColor; // 색상 원복

        _nextStageDoor.SetActive(true);
        Debug.Log("보스2 사망");
        gameObject.SetActive(false);
    }

    // =====================
    // 피격 깜빡임
    // =====================
    IEnumerator HitBlinkRoutine()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            _spriteRenderer.color = Color.black;
            yield return new WaitForSeconds(blinkInterval);
            _spriteRenderer.color = _originalColor; // 원본 색으로
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    // =====================
    // 외부에서 호출
    // =====================
    public void StartBoss2()
    {
        _isActive = true;
        StartCoroutine(DelayedStart());
    }

    public void StopBoss2()
    {
        _isActive = false;
        StopAllCoroutines();
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(phaseStartDelay);
        StartCoroutine(PatternCycleRoutine());
    }

    // =====================
    // 패턴 사이클
    // =====================
    IEnumerator PatternCycleRoutine()
    {
        while (_isActive)
        {
            ISkill skill = PickRandomSkill();
            if (skill != null)
                yield return StartCoroutine(skill.SkillRoutine());

            yield return StartCoroutine(ReturnToOrigin());
            yield return new WaitForSeconds(idleDuration);
        }
    }

    IEnumerator ReturnToOrigin()
    {
        Vector3 targetPos = new Vector3(transform.position.x, _originalPos.y, transform.position.z);

        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, returnSpeed * Time.deltaTime);
            yield return null;
            targetPos = new Vector3(transform.position.x, _originalPos.y, transform.position.z);
        }

        transform.position = new Vector3(transform.position.x, _originalPos.y, transform.position.z);
    }

    ISkill PickRandomSkill()
    {
        if (_skills.Count == 0) return null;
        return _skills[Random.Range(0, _skills.Count)];
    }
}