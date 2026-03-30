using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Clear : MonoBehaviour
{
    [SerializeField] private Button _mainMenuButton;

    [Header("통계 UI")]
    [SerializeField] private TextMeshProUGUI _clearTimeText;
    [SerializeField] private TextMeshProUGUI _deathCountText;
    [SerializeField] private TextMeshProUGUI _slowTimeText;

    public event Action OnMainMenuRequested;

    private void Awake()
    {
        _mainMenuButton?.onClick.AddListener(() => OnMainMenuRequested?.Invoke());
    }

    private void OnEnable()
    {
        UpdateStats();
    }
    void UpdateStats()
    {
        var stats = RaidStartManager.Instance;
        if (stats == null) return;

        if(_clearTimeText != null)
            _clearTimeText.text = $"클리어 시간 : {stats.FormatTime(stats.ClearTime)}";
        
        if(_deathCountText != null)
            _deathCountText.text = $"사망 횟수 : {stats.DeathCount}회";
        
        if(_slowTimeText != null)
            _slowTimeText.text = $"슬로우 사용 : {stats.FormatTime(stats.TotalSlowTime)}";
    }

    private void OnDestroy()
    {
        _mainMenuButton?.onClick.RemoveAllListeners();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
