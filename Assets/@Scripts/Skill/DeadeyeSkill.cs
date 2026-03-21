using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeadeyeSkill : MonoBehaviour
{
    // Temp
    [SerializeField] private GameObject _deadeyeBulletPrefab;
    [SerializeField] private float _deadeyeBulletSpeed = 20f;

    Player _player;
    Camera _cam;
    private PoolManager _poolManager;

    // =====================
    // 생명주기
    // =====================

    void Start()
    {
        _player = GetComponent<Player>();
        _cam = Camera.main;
        _originalFixedDeltaTime = Time.fixedDeltaTime;
        _player.playerHealth.OnDie += OnPlayerDie;

        // 풀매니저 세팅
        if (!ManagerRegistry.TryGet<PoolManager>(out _poolManager))
        {
            _poolManager = null;
        }

        _rangeTransform.localScale = Vector3.zero;
        _rangeRenderer.enabled = false;

    }

    void Update()
    {
        UpdateSlowGauge();
        UpdateDeadeyeRange();
    }

    void OnDestroy()
    {
        _player.playerHealth.OnDie -= OnPlayerDie;
    }

    void OnPlayerDie()
    {
        _isSlowActive = false;
        ExitSlow();
        ExitDeadeye();
        StopAllCoroutines();
    }

    // =====================
    // 게이지
    // =====================
    #region Gauge
    private float _maxGauge = 100f;
    [Tooltip("적을 죽일 때마다 차는 게이지량")][SerializeField] private float _gaugePerKill = 15f;
    private float _currentGauge = 0f;

    private bool CanSlowMotion => _currentGauge >= 1f;
    private bool CanDeadeye => _currentGauge >= 50f;

    public event Action<float> OnGaugeChanged; // UI 연동용

    public void AddGauge()
    {
        _currentGauge = Mathf.Min(_maxGauge, _currentGauge + _gaugePerKill);
        Debug.Log("현재 게이지: " + _currentGauge);
        OnGaugeChanged?.Invoke(_currentGauge);
    }

    private void ConsumeGauge(float amount)
    {
        _currentGauge = Mathf.Max(0f, _currentGauge - amount);
        Debug.Log("현재 게이지: " + _currentGauge);
        OnGaugeChanged?.Invoke(_currentGauge);
    }

    #endregion


    // =====================
    // 슬로우모션
    // =====================
    #region SlowMotion

    [SerializeField] private float _slowTimeScale = 0.2f;
    [Tooltip("초당 게이지 소모량")][SerializeField] private float _gaugeConsumeRate = 10f;
    private float _originalFixedDeltaTime;
    
    private bool _isSlowActive = false; // 상태 관리 잘못해서 생긴.

    // 천천히 풀리도록
    private Coroutine _slowExitRoutine;
    [SerializeField] private float _slowExitDuration = 0.5f;
    
    public void OnSlowMotion(InputAction.CallbackContext context)
    {
        if (_player.CurrentAction == ActionState.Deadeye) return; // 데드아이 중엔 슬로우 입력 무시

        if (context.started)
        {
            if (!CanSlowMotion) return;
            EnterSlow();
        }
        else if (context.canceled)
        {
            ExitSlow();
        }
    }

    private void EnterSlow()
    {
        if (_slowExitRoutine != null)
        {
            StopCoroutine(_slowExitRoutine);
            _slowExitRoutine = null;
        }
        _isSlowActive = true;

        if (_player.CurrentAction == ActionState.Deadeye)
            _isDeadeyeActive = true; // 데드아이로 인한 슬로우
        else
            _player.SetActionState(ActionState.Slow);
        
        Time.timeScale = _slowTimeScale;
        Time.fixedDeltaTime = _originalFixedDeltaTime * _slowTimeScale;
    }

    private void ExitSlow()
    {
        _isSlowActive = false;

        // Deadeye 중엔 Slow만 단독으로 해제하지 않음
        if (_player.CurrentAction != ActionState.Deadeye)
            _player.SetActionState(ActionState.None);

        if (_slowExitRoutine != null)
            StopCoroutine(_slowExitRoutine);
        _slowExitRoutine = StartCoroutine(SlowExitRoutine());
    }

    private IEnumerator SlowExitRoutine()
    {
        float elapsed = 0f;
        float startTimeScale = Time.timeScale;
        float startFixedDeltaTime = Time.fixedDeltaTime;

        while (elapsed < _slowExitDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / _slowExitDuration;
            Time.timeScale = Mathf.Lerp(startTimeScale, 1f, t);
            Time.fixedDeltaTime = Mathf.Lerp(startFixedDeltaTime, _originalFixedDeltaTime, t);
            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = _originalFixedDeltaTime;
        _slowExitRoutine = null;
    }

    private void UpdateSlowGauge()
    {
        if (!_isSlowActive) return; // ActionState 대신 bool로 체크
        if (_isDeadeyeActive) return; // 데드아이 슬로우면 게이지 소모 안 함

        ConsumeGauge(_gaugeConsumeRate * Time.unscaledDeltaTime);

        if (!CanSlowMotion)
            ExitSlow();
    }

    #endregion


    // =====================
    // 데드아이
    // =====================
    #region Deadeye
    [SerializeField] private int _damagePerShot = 120;
    [SerializeField] private float _timeBetweenShots = 0.1f;
    [SerializeField] private int _maxTargets = 6;
    [SerializeField] private float _gaugeCostDeadeye = 50f;

    [Header("Deadeye Range")]
    [SerializeField] private Transform _rangeTransform;    // DeadeyeRange 오브젝트
    [SerializeField] private SpriteRenderer _rangeRenderer;
    [SerializeField] private float _maxRadius = 5f;        // 최대 반지름
    [SerializeField] private float _expandDuration = 1f;   // 최대까지 커지는 시간
    [SerializeField] private LayerMask _enemyLayer;

    private bool _isDeadeyeActive = false; // 상태 관리 실수
    private bool _isFiring = false;
    private float _currentRadius = 0f;
    private List<EnemyBase> _targets = new List<EnemyBase>();
    
    public bool IsDeadeyeActive => _isDeadeyeActive;


    private Coroutine _markingTimer;

    // 토글 방식 동작 (Q) : 마우스(홀드) 드래그로 적 마킹하고 떼면 다다다 
    public void OnDeadeye(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (_isFiring) return; // 데드아이로 다라라 죽이고 있는 중이라면 리턴

            if (_isDeadeyeActive)
            {
                FireDeadeye();
                return;
            }

            if (!CanDeadeye) return;
            _isDeadeyeActive = true;
            _player.SetActionState(ActionState.Deadeye);
            ConsumeGauge(_gaugeCostDeadeye);
            EnterSlow();

            // 범위 초기화 후 표시
            _currentRadius = 0f;
            _rangeTransform.localScale = Vector3.zero;
            _rangeRenderer.enabled = true;
        }
        else if (context.canceled)
        {
            if (!_isDeadeyeActive) return;
            if (_isFiring) return; // 이미 발사 중이면 무시
            FireDeadeye();
        }
    }

    private void UpdateDeadeyeRange()
    {
        if (!_isDeadeyeActive || _isFiring) return;

        // 원 키우기
        _currentRadius = Mathf.MoveTowards(_currentRadius, _maxRadius, (_maxRadius / _expandDuration) * Time.unscaledDeltaTime);

        // 스프라이트 scale 업데이트 (지름 = 반지름 * 2)
        float diameter = _currentRadius * 2f;
        _rangeTransform.localScale = new Vector3(diameter, diameter, 1f);

        // 범위 안 적 타겟팅
        UpdateTargets();

        // 최대 크기 도달 시 자동 발사
        if (Mathf.Approximately(_currentRadius, _maxRadius))
            FireDeadeye();
    }

    private void UpdateTargets()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _currentRadius, _enemyLayer);

        foreach (Collider2D hit in hits)
        {
            if (_targets.Count >= _maxTargets) break;

            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null && !_targets.Contains(enemy) && enemy.CurrentHp > 0)
            {
                _targets.Add(enemy);
                enemy.ShowMark(true);
            }
        }
    }
    private void FireDeadeye()
    {
        if (_targets.Count == 0)
        {
            ExitDeadeye();
            return;
        }
        StartCoroutine(FireAtTargets());
    }


    IEnumerator FireAtTargets()
    {
        _isFiring = true;
        List<EnemyBase> targets = new List<EnemyBase>(_targets);

        foreach (EnemyBase enemy in targets)
        {
            if (enemy != null && enemy.CurrentHp > 0)
            {
                enemy.ShowMark(false);
                Vector2 dir = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;

                // 연출용 총알
                GameObject go = _poolManager != null
                    ? _poolManager.Get(_deadeyeBulletPrefab, transform.position, Quaternion.identity)
                    : Instantiate(_deadeyeBulletPrefab, transform.position, Quaternion.identity);
                if (go.TryGetComponent<Rigidbody2D>(out var rb))
                    rb.linearVelocity = dir * _deadeyeBulletSpeed;

                // 실제 공격 동시에
                enemy.TakeDamage(_damagePerShot, false);
            }
            yield return new WaitForSecondsRealtime(_timeBetweenShots);
        }

        _isFiring = false;
        ExitDeadeye();
    }

    private void ExitDeadeye()
    {
        _isDeadeyeActive = false; // 데드아이 종료 시 리셋
        _player.SetActionState(ActionState.None);
        _isFiring = false;

        // 범위 숨기기
        _rangeRenderer.enabled = false;
        _rangeTransform.localScale = Vector3.zero;
        _currentRadius = 0f;

        // 타겟 마크 해제
        foreach (EnemyBase enemy in _targets)
            if (enemy != null) enemy.ShowMark(false);
        _targets.Clear();

        ExitSlow(); // 슬로우 자동 해제
    }

    #endregion

}
