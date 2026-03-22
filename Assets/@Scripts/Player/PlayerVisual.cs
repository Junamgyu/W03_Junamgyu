using DG.Tweening;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Transform _visual; // Visual 오브젝트

    [Header("Squash & Stretch")]
    [SerializeField] private float _jumpStretchX = 0.7f;
    [SerializeField] private float _jumpStretchY = 1.3f;
    [SerializeField] private float _jumpDuration = 0.1f;

    [SerializeField] private float _landSquashX = 1.3f;
    [SerializeField] private float _landSquashY = 0.7f;
    [SerializeField] private float _landDuration = 0.15f;

    [SerializeField] private float _recoilSquashX = 1.2f;
    [SerializeField] private float _recoilSquashY = 0.8f;
    [SerializeField] private float _recoilDuration = 0.1f;

    [Header("Tilt")]
    [SerializeField] private float _maxTilt = 15f;
    [SerializeField] private float _tiltSpeed = 10f;

    [Header("Return")]
    [SerializeField] private float _returnDuration = 0.2f;

    private Vector3 _originalScale;
    private Player _player;
    private Rigidbody2D _rb;

    void Start()
    {
        _player = GetComponentInParent<Player>();
        _rb = GetComponentInParent<Rigidbody2D>();
        _originalScale = _visual.localScale;

        _player.OnLocomotionChanged += HandleLocomotionChanged;
        _player.OnRecoilStateChanged += HandleRecoilStateChanged;
    }

    void OnDestroy()
    {
        _player.OnLocomotionChanged -= HandleLocomotionChanged;
        _player.OnRecoilStateChanged -= HandleRecoilStateChanged;
    }

    void Update()
    {
        HandleTilt();
    }

    private void HandleLocomotionChanged(LocomotionState state)
    {
        switch (state)
        {
            case LocomotionState.Jumping:
                // 길쭉하게
                _visual.DOKill();
                _visual.DOScaleX(_jumpStretchX, _jumpDuration)
                    .OnComplete(() => _visual.DOScaleX(_originalScale.x, _returnDuration));
                _visual.DOScaleY(_jumpStretchY, _jumpDuration)
                    .OnComplete(() => _visual.DOScaleY(_originalScale.y, _returnDuration));
                break;

            case LocomotionState.Land:
                // 납작하게
                _visual.DOKill();
                _visual.DOScaleX(_landSquashX, _landDuration)
                    .OnComplete(() => _visual.DOScaleX(_originalScale.x, _returnDuration));
                _visual.DOScaleY(_landSquashY, _landDuration)
                    .OnComplete(() => _visual.DOScaleY(_originalScale.y, _returnDuration));
                break;
        }
    }

    private void HandleRecoilStateChanged(RecoilState state)
    {
        if (state != RecoilState.Recoiling) return;

        // 반동 방향 반대로 찌그러짐
        Vector2 recoilDir = _player.playerAimer.AimDirection;

        // 반동이 수평에 가까우면 X 찌그러짐, 수직에 가까우면 Y 찌그러짐
        float absX = Mathf.Abs(recoilDir.x);
        float absY = Mathf.Abs(recoilDir.y);

        _visual.DOKill();
        if (absX > absY)
        {
            // 수평 반동
            _visual.DOScaleX(_recoilSquashX, _recoilDuration)
                .OnComplete(() => _visual.DOScaleX(_originalScale.x, _returnDuration));
            _visual.DOScaleY(_recoilSquashY, _recoilDuration)
                .OnComplete(() => _visual.DOScaleY(_originalScale.y, _returnDuration));
        }
        else
        {
            // 수직 반동
            _visual.DOScaleX(_recoilSquashY, _recoilDuration)
                .OnComplete(() => _visual.DOScaleX(_originalScale.x, _returnDuration));
            _visual.DOScaleY(_recoilSquashX, _recoilDuration)
                .OnComplete(() => _visual.DOScaleY(_originalScale.y, _returnDuration));
        }
    }

    private void HandleTilt()
    {
        if (_player.CurrentRecoil == RecoilState.Recoiling) return;

        float targetTilt = 0f;
        if (_rb.linearVelocity.x != 0)
            targetTilt = Mathf.Sign(_rb.linearVelocity.x) * _maxTilt;

        float currentZ = _visual.localEulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f; // 각도 정규화

        float newZ = Mathf.Lerp(currentZ, -targetTilt, _tiltSpeed * Time.deltaTime);
        _visual.localEulerAngles = new Vector3(0f, 0f, newZ);
    }
}
