using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeadeyeSkill : MonoBehaviour
{
    Player _player;
    private PoolManager _poolManager;
    // Temp
    [SerializeField] private GameObject _deadeyeBulletPrefab;
    [SerializeField] private float _deadeyeBulletSpeed = 20f;
    

  
    

    // =====================
    // 생명주기
    // =====================

    void Start()
    {
        _player = GetComponent<Player>();
        _originalFixedDeltaTime = Time.fixedDeltaTime;

        // 풀매니저 세팅
        if (!ManagerRegistry.TryGet<PoolManager>(out _poolManager))
        {
            _poolManager = null;
        }

    }

    void Update()
    {
        UpdateSlowGauge();
    }

    void OnDestroy()
    {
        _player.playerHealth.OnDie -= OnPlayerDie;
    }

    void OnPlayerDie()
    {
        ResetState();
    }

    // =====================
    // 게이지
    // =====================
    #region Gauge
    private float _maxGauge = 100f;
    private float _currentGauge = 30f;

    private bool CanSlowMotion => _currentGauge >= 1f;
    public float CurrentGauge => _currentGauge;

    public event Action<float> OnGaugeChanged; // UI 연동용

    public void AddGauge(float amount)
    {
        _currentGauge = Mathf.Min(_maxGauge, _currentGauge + amount);
        OnGaugeChanged?.Invoke(_currentGauge);
    }

    private void ConsumeGauge(float amount)
    {
        _currentGauge = Mathf.Max(0f, _currentGauge - amount);
        OnGaugeChanged?.Invoke(_currentGauge);
    }

    #endregion


    // =====================
    // 슬로우모션
    // =====================
    #region SlowMotion

    [SerializeField] private float _slowTimeScale = 0.2f;
    [Tooltip("초당 게이지 소모량")][SerializeField] private float _gaugeConsumeRate = 20f;
    [SerializeField] private float _slowExitDuration = 0.5f;
    private float _originalFixedDeltaTime;

    [Tooltip("초당 게이지 회복량")]
    [SerializeField] private float _gaugeRecoverRate = 5f;

    // 천천히 풀리도록 하기 위한 코루틴 관리
    private Coroutine _slowExitRoutine;
    
    public void OnSlowMotion(InputAction.CallbackContext context)
    {
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

        _player.SetSkillState(SkillState.Slow);

        Time.timeScale = _slowTimeScale;
        Time.fixedDeltaTime = _originalFixedDeltaTime * _slowTimeScale;

        RaidStartManager.Instance?.OnSlowStart();

        if (SoundManager.instance != null)
            SoundManager.instance.SetSlowAudio(Time.timeScale);
        
    }

    private void ExitSlow()
    {
        _player.SetSkillState(SkillState.None);

        if (_slowExitRoutine != null)
            StopCoroutine(_slowExitRoutine);
        _slowExitRoutine = StartCoroutine(SlowExitRoutine());

        RaidStartManager.Instance?.OnSlowEnd();
    }

    private IEnumerator SlowExitRoutine()
    {
        float elapsed = 0f;
        float startTimeScale = Time.timeScale;
        float startFixedDeltaTime = Time.fixedDeltaTime;

        while (elapsed < _slowExitDuration)
        {
            // 슬로우가 다시 켜지면 중단 TODO:?????
            if (_player.CurrentSkill == SkillState.Slow)
                yield break;

            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / _slowExitDuration;
            Time.timeScale = Mathf.Lerp(startTimeScale, 1f, t);
            Time.fixedDeltaTime = Mathf.Lerp(startFixedDeltaTime, _originalFixedDeltaTime, t);

            if (SoundManager.instance != null)
                    SoundManager.instance.SetSlowAudio(Time.timeScale);
            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = _originalFixedDeltaTime;
        _slowExitRoutine = null;
    }

    private void UpdateSlowGauge()
    {
        // Slow 상태일 때만 게이지 소모 (Deadeye일 땐 소모 안 함)
        if (_player.CurrentSkill == SkillState.Slow)
        {
            ConsumeGauge(_gaugeConsumeRate * Time.unscaledDeltaTime);

            if(!CanSlowMotion)
                ExitSlow();
        }
        else
        {
            _currentGauge = Mathf.Min(_maxGauge, _currentGauge + _gaugeRecoverRate * Time.unscaledDeltaTime);
            OnGaugeChanged?.Invoke(_currentGauge);

        }
    }

    #endregion

    public void ResetState()
    {
        StopAllCoroutines();
        _player.SetSkillState(SkillState.None);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = _originalFixedDeltaTime;
    }

}
