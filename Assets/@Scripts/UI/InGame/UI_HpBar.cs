using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_HpBar : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private RectTransform _bar;

    [SerializeField] private GameObject _heartPrefab;

    [Header("Color")]
    [SerializeField] private Color _filledColor = Color.red;
    [SerializeField] private Color _emptyColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("Animation")]
    [Tooltip("커졌다가 줄어드는 최대 크기")][SerializeField] private float _punchScale = 1.3f;  
    [Tooltip("커지는 시간")][SerializeField] private float _punchDuration = 0.1f;   
    [Tooltip("줄어드는 시간")][SerializeField] private float _shrinkDuration = 0.15f; 
    [Tooltip("회복 시 커지는 시간")][SerializeField] private float _growDuration = 0.2f;

    private Image[] _slots;
    private int _currentHp;
    private int _maxHp;

    private void Start()
    {
        _maxHp = _playerHealth.MaxHp;
        _currentHp = _maxHp;
        _slots = CreateSlots(_maxHp);

        _playerHealth.OnHit += HandleHit;
        _playerHealth.OnHeal += HandleHeal;
    }

    private void OnDestroy()
    {
        _playerHealth.OnHit -= HandleHit;
        _playerHealth.OnHeal -= HandleHeal;
    }

    private Image[] CreateSlots(int count)
    {
        foreach (Transform child in _bar)
            Destroy(child.gameObject);

        Image[] slots = new Image[count];
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(_heartPrefab, _bar);
            slots[i] = go.GetComponent<Image>();
            slots[i].color = _filledColor;
        }
        return slots;
    }

    private void HandleHit(int damage)
    {
        int prevHp = _currentHp;
        _currentHp = Mathf.Max(0, _currentHp - damage);

        // 줄어든 하트만큼 애니메이션
        for (int i = prevHp - 1; i >= _currentHp; i--)
        {
            int index = i;
            RectTransform rect = _slots[index].rectTransform;

            rect.DOKill();
            rect.localScale = Vector3.one;

            // 커졌다가 0으로 줄어들며 빈 하트로
            rect.DOScale(_punchScale, _punchDuration)
                .OnComplete(() =>
                {
                    rect.DOScale(0f, _shrinkDuration)
                        .OnComplete(() =>
                        {
                            _slots[index].color = _emptyColor;
                            rect.DOScale(1f, _growDuration);
                        });
                });
        }
    }

    private void HandleHeal(int amount)
    {
        int prevHp = _currentHp;
        _currentHp = Mathf.Min(_maxHp, _currentHp + amount);

        // 늘어난 하트만큼 애니메이션
        for (int i = prevHp; i < _currentHp; i++)
        {
            int index = i;
            RectTransform rect = _slots[index].rectTransform;

            rect.DOKill();
            rect.localScale = Vector3.zero;
            _slots[index].color = _filledColor;

            // 스케일 0에서 커지며 채워진 하트로
            rect.DOScale(1f, _growDuration).SetEase(Ease.OutBack);
        }
    }

}
