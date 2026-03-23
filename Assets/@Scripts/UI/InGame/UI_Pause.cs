using UnityEngine;
using UnityEngine.UI;

public class UI_Pause : UI_Base
{
    [Header("Pause Buttons")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _mainMenuButton;

    private PauseController _pauseController;

    public event System.Action OnRetryRequested;
    public event System.Action OnMainMenuRequested;

    protected override void Awake()
    {
        ManagerRegistry.TryGet(out _pauseController);
        base.Awake(); // BindEvents 실행
    }

    protected override void BindEvents()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(() => _pauseController?.ResumeGame());

        if (_retryButton != null)
            _retryButton.onClick.AddListener(() => OnRetryRequested?.Invoke());


        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(() => OnMainMenuRequested?.Invoke());

    }

}