using UnityEngine;

public class UIManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _clearPanel;

    [Header("Panel Scripts")]
    [SerializeField] private UI_Pause _uiPausePanel;
    [SerializeField] private UI_GameOver _uiGameOverPanel;
    [SerializeField] private UI_Clear _uiClearPanel;

    private GameStateManager _gameStateManager;
    private PauseController _pauseController;
    private SceneFlowManager _sceneFlowManager;
    private PoolManager _poolManager;

    public void Initialize()
    {
        if (IsInitialized)
            return;

        if (!ManagerRegistry.TryGet(out _gameStateManager))
        {
            Debug.LogError($"{name}: GameStateManager not found.");
            return;
        }

        ManagerRegistry.TryGet(out _pauseController);
        ManagerRegistry.TryGet(out _sceneFlowManager);
        ManagerRegistry.TryGet(out _poolManager);

        _gameStateManager.OnStateChanged += HandleStateChanged;

        RebindUI();

        IsInitialized = true;
    }

    public void RebindUI()
    {
        UnbindPanelEvents();
        FindUIReferences();
        BindPanelEvents();
        HideAll();
    }

    private void FindUIReferences()
    {
        _uiPausePanel = FindAnyObjectByType<UI_Pause>(FindObjectsInactive.Include);
        _uiGameOverPanel = FindAnyObjectByType<UI_GameOver>(FindObjectsInactive.Include);
        _uiClearPanel = FindAnyObjectByType<UI_Clear>(FindObjectsInactive.Include);

        _pausePanel = null;
        _gameOverPanel = null;
        _clearPanel = null;

        if (_uiPausePanel != null)
            _pausePanel = _uiPausePanel.gameObject;

        if (_uiGameOverPanel != null)
            _gameOverPanel = _uiGameOverPanel.gameObject;

        if(_uiClearPanel != null)
            _clearPanel = _uiClearPanel.gameObject;
    }

    private void BindPanelEvents()
    {
        if (_uiPausePanel != null)
        {
            _uiPausePanel.OnRetryRequested += HandleRetryRequested;
            _uiPausePanel.OnMainMenuRequested += HandleMainMenuRequested;
        }

        if (_uiGameOverPanel != null)
        {
            _uiGameOverPanel.OnRetryRequested += HandleRetryRequested;
            _uiGameOverPanel.OnMainMenuRequested += HandleMainMenuRequested;
        }

        if(_uiClearPanel != null)
            _uiClearPanel.OnMainMenuRequested += HandleMainMenuRequested;
    }

    private void UnbindPanelEvents()
    {
        if (_uiPausePanel != null)
        {
            _uiPausePanel.OnRetryRequested -= HandleRetryRequested;
            _uiPausePanel.OnMainMenuRequested -= HandleMainMenuRequested;

            //_uiGameOverPanel.OnRetryRequested -= HandleRetryRequested;
            //_uiGameOverPanel.OnMainMenuRequested -= HandleMainMenuRequested;
        }

        if(_uiClearPanel != null)
            _uiClearPanel.OnMainMenuRequested -= HandleMainMenuRequested;
    }

    private void HandleStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Paused:
                ShowPause();
                break;

            case GameState.GameOver:
                ShowGameOver();
                break;

            case GameState.Clear:
                ShowClear();
                if(ManagerRegistry.TryGet<InputManager>(out var im))
                    im.DisablePlayerInput();
                break;

            case GameState.Respawning:          //추가 - 패널 안뜨게
                HideAll();
                break;

            default:
                HideAll();
                break;
        }
    }

    private void HandleRetryRequested()
    {
        _gameStateManager.ChangeState(GameState.Respawning);
        _pauseController?.ResumeGame();
        _poolManager?.ClearRuntimeObjects();
        _sceneFlowManager?.LoadStage();
    }

    private void HandleMainMenuRequested()
    {
        _poolManager?.ClearRuntimeObjects();

        //! 체크포인트 초기화 추가
        if(ManagerRegistry.TryGet<CheckpointManager>(out var cm))
            cm.ClearCheckpoint();

        _sceneFlowManager?.LoadMainMenu();
    }
    
    public void ShowClear()
    {
        HideAll();
        if(_clearPanel != null)
            _clearPanel.SetActive(true);
    }

    public void ShowPause()
    {
        if (_pausePanel != null)
        {
            _pausePanel.SetActive(true);
            _uiPausePanel.ApplyFirstSelection();
        }
            

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);
    }

    public void HidePause()
    {
        if (_pausePanel != null)
            _pausePanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(true);
    }

    public void HideGameOver()
    {
        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);
    }

    public void HideAll()
    {
        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);

        if(_clearPanel != null)
            _clearPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_gameStateManager != null)
            _gameStateManager.OnStateChanged -= HandleStateChanged;

        UnbindPanelEvents();
    }
}