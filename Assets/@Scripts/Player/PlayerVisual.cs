using DG.Tweening;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Transform _visual;

    [Header("Jump Settings")]
    [SerializeField] private float _jumpStretchX = 0.75f;
    [SerializeField] private float _jumpStretchY = 1.25f;
    [SerializeField] private float _jumpActionDuration = 0.05f; // 점프는 즉각적이어야 함
    [SerializeField] private float _jumpReturnDuration = 0.3f;
    [SerializeField] private Ease _jumpEase = Ease.OutQuad;

    [Header("Landing Settings")]
    [SerializeField] private float _landSquashX = 1.4f;
    [SerializeField] private float _landSquashY = 0.6f;
    [SerializeField] private float _landActionDuration = 0.08f; // 랜딩은 약간의 무게감이 필요
    [SerializeField] private float _landReturnDuration = 0.5f;  // 더 쫀득하게 돌아옴
    [SerializeField] private Ease _landEase = Ease.OutQuint;

    [Header("Dynamic Tilt")]
    [SerializeField] private float _maxTiltAngle = 12f;
    [SerializeField] private float _tiltSpeed = 15f;
    [SerializeField] private float _tiltReferenceSpeed = 10f;

    private Vector3 _originalScale;
    private Vector3 _originalPos;
    private Player _player;
    private Rigidbody2D _rb;
    private float _height;

    void Start()
    {
        _player = GetComponentInParent<Player>();
        _rb = GetComponentInParent<Rigidbody2D>();

        _originalScale = _visual.localScale;
        _originalPos = _visual.localPosition;

        var renderer = _visual.GetComponentInChildren<SpriteRenderer>();
        _height = renderer != null ? renderer.bounds.size.y : 1f;

        _player.OnLocomotionChanged += HandleLocomotionChanged;
        _player.OnRecoilStateChanged += HandleRecoilStateChanged;
    }

    private void HandleLocomotionChanged(LocomotionState state)
    {
        _visual.DOKill(true);
        ResetVisual();

        if (state == LocomotionState.Jumping)
        {
            ExecuteJumpVisual();
        }
        else if (state == LocomotionState.Land)
        {
            ExecuteLandVisual();
        }
    }

    private void ExecuteJumpVisual()
    {
        // 점프: 위로 길쭉하게
        _visual.DOScale(new Vector3(_jumpStretchX, _jumpStretchY, 1f), _jumpActionDuration).SetEase(_jumpEase)
            .OnComplete(() => _visual.DOScale(_originalScale, _jumpReturnDuration).SetEase(Ease.OutElastic));

        // 피봇 보정
        float yOffset = (_height * (_jumpStretchY - _originalScale.y)) / 2f;
        _visual.DOLocalMoveY(_originalPos.y + yOffset, _jumpActionDuration).SetEase(_jumpEase)
            .OnComplete(() => _visual.DOLocalMoveY(_originalPos.y, _jumpReturnDuration).SetEase(Ease.OutElastic));
    }

    private void ExecuteLandVisual()
    {
        // 랜딩: 바닥에 납작하게
        _visual.DOScale(new Vector3(_landSquashX, _landSquashY, 1f), _landActionDuration).SetEase(_landEase)
            .OnComplete(() => _visual.DOScale(_originalScale, _landReturnDuration).SetEase(Ease.OutElastic));

        // 피봇 보정
        float yOffset = (_height * (_landSquashY - _originalScale.y)) / 2f;
        _visual.DOLocalMoveY(_originalPos.y + yOffset, _landActionDuration).SetEase(_landEase)
            .OnComplete(() => _visual.DOLocalMoveY(_originalPos.y, _landReturnDuration).SetEase(Ease.OutElastic));
    }

    private void HandleRecoilStateChanged(RecoilState state)
    {
        if (state != RecoilState.Recoiling) return;

        _visual.DOKill(true);
        Vector2 aimDir = _player.playerAimer.AimDirection;

        _visual.DOPunchScale(new Vector3(Mathf.Abs(aimDir.x) * 0.2f, Mathf.Abs(aimDir.y) * 0.2f, 0), 0.12f, 8, 1f);
        float rotAmount = aimDir.x > 0 ? 12f : -12f;
        _visual.DOPunchRotation(new Vector3(0, 0, -rotAmount), 0.12f, 8, 1f);
    }

    void Update()
    {
        HandleTilt();
    }

    private void HandleTilt()
    {
        float targetZ = 0f;
        float velX = _rb.linearVelocity.x;

        if (Mathf.Abs(velX) > 0.1f)
        {
            targetZ = -(velX / _tiltReferenceSpeed) * _maxTiltAngle;
        }

        float currentZ = _visual.localEulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f;

        float newZ = Mathf.Lerp(currentZ, targetZ, Time.deltaTime * _tiltSpeed);
        newZ = Mathf.Clamp(newZ, -_maxTiltAngle, _maxTiltAngle);

        _visual.localEulerAngles = new Vector3(0, 0, newZ);
    }

    private void ResetVisual()
    {
        _visual.localScale = _originalScale;
        _visual.localPosition = _originalPos;
    }

    void OnDestroy()
    {
        if (_player != null)
        {
            _player.OnLocomotionChanged -= HandleLocomotionChanged;
            _player.OnRecoilStateChanged -= HandleRecoilStateChanged;
        }
    }
}