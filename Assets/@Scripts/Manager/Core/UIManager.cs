using UnityEngine;

public class UIManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _gameOverPanel;

    [Header("Panel Scripts")]
    [SerializeField] private UI_PausePanel _uiPausePanel;

    private GameStateManager _gameStateManager;
    private PauseController _pauseController;
    private SceneFlowManager _sceneFlowManager;

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

        _gameStateManager.OnStateChanged += HandleStateChanged;

        if (_uiPausePanel != null)
        {
            _uiPausePanel.OnRetryRequested += HandleRetryRequested;
        }

        HideAll();
        IsInitialized = true;
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

            default:
                HideAll();
                break;
        }
    }

    private void HandleRetryRequested()
    {
        _pauseController?.ResumeGame();
        _sceneFlowManager?.ReloadStage();
    }

    public void ShowPause()
    {
        if (_pausePanel != null)
            _pausePanel.SetActive(true);

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(true);
    }

    public void HideAll()
    {
        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_gameStateManager != null)
            _gameStateManager.OnStateChanged -= HandleStateChanged;

        if (_uiPausePanel != null)
            _uiPausePanel.OnRetryRequested -= HandleRetryRequested;
    }
}