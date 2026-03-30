using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShield : MonoBehaviour
{
    [Header("방패 설정")]
    [SerializeField] private float _maxGauge = 100f;
    [SerializeField] private float _drainPerSec = 1f;           //홀드 중 초당 감소
    [SerializeField] private float _blockCost = 30f;            //막을 때 감소량
    [SerializeField] private float _recoverPerSec = 20f;        //미사용 시 초당 회복
    
    [Header("컴포넌트")]
    [SerializeField] private Collider2D _shieldCollider;       //ShieldHitbox 오브젝트
    [SerializeField] private Slider _gaugeSlider;               // UI 게이지

    private float _currentGauge;
    private bool _isHolding = false;
    private bool _isDepleted = false;       //게이지 소진 시 홀드 불가

    public bool IsShieldActive => _isHolding && !_isDepleted;

    private void Awake()
    {
        _currentGauge = _maxGauge;
        if(_shieldCollider != null)
            _shieldCollider.enabled = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleGauge();
        UpdateUI();
        UpdateHitbox();
    }


    public void OnShieldOn()
    {
        if(_isDepleted) return;     //게이지 소진 시 홀드 불가
        _isHolding = true;
    }

    public void OnShieldOff()
    {
        _isHolding = false;
    }

    void HandleGauge()
    {
        if(IsShieldActive)
        {
            _currentGauge -= _drainPerSec * Time.deltaTime;
            _currentGauge = Mathf.Max(0f, _currentGauge);

            if(_currentGauge <= 0f)
            {
                _isDepleted = true;
                _isHolding = false;
            }
        }
        else
        {
            _currentGauge += _recoverPerSec * Time.deltaTime;
            _currentGauge = Mathf.Min(_maxGauge, _currentGauge);

            if(_isDepleted && _currentGauge >= _maxGauge * 0.3f)
                _isDepleted = false;
        }
    }

    void UpdateHitbox()
    {
        if(_shieldCollider != null)
            _shieldCollider.enabled = IsShieldActive;
    }

    void UpdateUI()
    {
        if(_gaugeSlider != null)
            _gaugeSlider.value = _currentGauge / _maxGauge;
    }

    public bool TryBlock()
    {
        if (!IsShieldActive) return false;
        _currentGauge -= _blockCost;
        _currentGauge = Mathf.Max(0f, _currentGauge);

        if(_currentGauge <= 0f)
        {
            _isDepleted = true;
            _isHolding = false;
        }

        return true;
    }


}
