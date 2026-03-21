using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SkillGaugeProgressbar : MonoBehaviour
{

    [SerializeField] private Player _player;
    [SerializeField] private CanvasGroup _canvasGroup;

    [SerializeField] private Slider _progressBar;

    [SerializeField] private RectTransform _fillRect;
    [SerializeField] private RectTransform _glowTip;
    [SerializeField] private float _tweenDuration = 0.5f;
    [SerializeField] private float _pulseSpeed = 3f;
    [SerializeField] private float _pulseAmount = 0.2f;

    [Header("색상")]
    [SerializeField] private Image _fillImage;                              // Slider의 Fill Image
    [SerializeField] private Color _colorBelow = new Color(0f, 0.78f, 0f); // 50% 미만 색상
    [SerializeField] private Color _colorAbove = new Color(0.87f, 0.2f, 0f); // 50% 이상 색상


    private void Awake()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
    private void Start()
    {
        if (_player != null)
            _player.deadeyeSkill.OnGaugeChanged += HandleGaugeChanged;
    }

    private void OnDestroy()
    {
        if (_player != null)
            _player.deadeyeSkill.OnGaugeChanged -= HandleGaugeChanged;
       
        _progressBar.DOKill();
    }

    void Update()
    {
        UpdateGlowTip();
        PulseGlowTip();
    }

    private void HandleGaugeChanged(float value)
    {
        SetProgress(value / 100f);
        UpdateFillColor(value);
    }

    public void SetProgress(float value)
    {
        _progressBar.DOKill();
        _progressBar.DOValue(Mathf.Clamp01(value), _tweenDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);

    }

    private void UpdateFillColor(float value)
    {
        if (_fillImage == null) return;

        Color targetColor = (value >= 50f) ? _colorAbove : _colorBelow;

        _fillImage.DOKill();
        _fillImage.DOColor(targetColor, _tweenDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }


    private void UpdateGlowTip()
    {
        if (_glowTip == null || _fillRect == null) return;

        // Fill의 실제 현재 너비로 계산
        float totalWidth = _fillRect.parent.GetComponent<RectTransform>().rect.width;
        float fillWidth = totalWidth * _progressBar.value;
        _glowTip.anchoredPosition = new Vector2(fillWidth, 0);

        _glowTip.gameObject.SetActive(_progressBar.value > 0.01f);
    }

    private void PulseGlowTip()
    {
        if (_glowTip == null) return;

        // 크기 펄스
        float pulse = 1f + Mathf.Sin(Time.time * _pulseSpeed) * _pulseAmount;
        _glowTip.localScale = new Vector3(pulse, pulse, 1f);

        // 알파 펄스
        Image img = _glowTip.GetComponent<Image>();
        if (img != null)
        {
            float alpha = 0.7f + Mathf.Sin(Time.time * _pulseSpeed) * 0.3f;
            img.color = new Color(1f, 1f, 1f, alpha);
        }
    }
}
