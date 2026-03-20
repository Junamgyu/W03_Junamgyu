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

    }

    void Update()
    {
        UpdateSlowGauge();
        UpdateMarking();
    }

    void OnDestroy()
    {
        _player.playerHealth.OnDie -= OnPlayerDie;
    }

    void OnPlayerDie()
    {
        ExitSlow();
        ExitDeadeye();
        StopAllCoroutines();
    }

    // =====================
    // 게이지
    // =====================
    #region Gauge
    [SerializeField] private float _maxGauge = 100f;
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
    private bool _isSlowActive = false;
    private float _originalFixedDeltaTime;

    public void OnSlowMotion(InputAction.CallbackContext context)
    {
        if (_isDeadeyeActive) return; // 데드아이 중엔 슬로우 입력 무시

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
        _isSlowActive = true;
        Time.timeScale = _slowTimeScale;
        Time.fixedDeltaTime = _originalFixedDeltaTime * _slowTimeScale;
    }

    private void ExitSlow()
    {
        _isSlowActive = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _originalFixedDeltaTime;
    }

    private void UpdateSlowGauge()
    {
        if (!_isSlowActive) return;
        if (_isDeadeyeActive) return; // 데드아이 중엔 슬로우 게이지 소모 안 함

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
    [SerializeField] private int _maxTargets = 3;
    [SerializeField] private float _gaugeCostDeadeye = 50f;
    [SerializeField] private float _markingDuration = 3f;

    private bool _isDeadeyeActive = false;
    private bool _isAiming = false;
    private bool _isFiring = false;
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
                ExitDeadeye(); // 이미 활성 중이면 취소 (이미 느려지는 것을 쓴거니 게이지 회복 같은 것은 없음)
                return;
            }

            if (!CanDeadeye) return;
            _isDeadeyeActive = true;
            ConsumeGauge(_gaugeCostDeadeye);
            EnterSlow();
            _markingTimer = StartCoroutine(MarkingTimerRoutine());
        }
    }
    private IEnumerator MarkingTimerRoutine()
    {
        yield return new WaitForSecondsRealtime(_markingDuration); // 슬로우 영향 안 받게
        if (_targets.Count > 0)
            StartCoroutine(FireAtTargets());
        else
            ExitDeadeye();
    }

    public void OnMarkTarget(InputAction.CallbackContext context)
    {
        if (!_isDeadeyeActive) return;

        if (context.started)
        {
            _isAiming = true;
        }
        else if (context.canceled)
        {
            _isAiming = false;
            if (_markingTimer != null)
            {
                StopCoroutine(_markingTimer);
                _markingTimer = null;
            }

            // 타겟 있으면 쏘기
            if (_targets.Count > 0)
                StartCoroutine(FireAtTargets());
            else
                ExitDeadeye(); // 타겟 없으면 그냥 취소
        }
    }

    private void UpdateMarking()
    {
        if (!_isDeadeyeActive || !_isAiming) return;
        if (_targets.Count >= _maxTargets) return;

        Vector2 mouseWorld = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);
        if (hit == null) return;

        EnemyBase enemy = hit.GetComponent<EnemyBase>();
        if (enemy != null && !_targets.Contains(enemy) && enemy.CurrentHp > 0)
        {
            _targets.Add(enemy);
            enemy.ShowMark(true);
        }
    }

    IEnumerator FireAtTargets()
    {
        // 순서대로 처치
        _isFiring = true;
        
        foreach (EnemyBase enemy in _targets)
        {
            if (enemy != null && enemy.CurrentHp > 0)
            {
                enemy.ShowMark(false);

                Vector2 dir = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;

                GameObject go = _poolManager != null
                    ? _poolManager.Get(_deadeyeBulletPrefab, transform.position, Quaternion.identity)
                    : Instantiate(_deadeyeBulletPrefab, transform.position, Quaternion.identity);

                if (go.TryGetComponent<Rigidbody2D>(out var rb))
                    rb.linearVelocity = dir * _deadeyeBulletSpeed;

            }
            yield return new WaitForSeconds(_timeBetweenShots);
        }

        ExitDeadeye(); // 시간 복구
        _isFiring = false;
    }

    private void ExitDeadeye()
    {
        _isDeadeyeActive = false;
        _isAiming = false;
        ExitSlow(); // 슬로우 자동 해제
        foreach (EnemyBase enemy in _targets)
            if (enemy != null) enemy.ShowMark(false);
        _targets.Clear();
    }

    #endregion

    


    

    

}
