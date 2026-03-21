using UnityEngine;

public class UIManager : MonoBehaviour, IInitializable
{
    public bool IsInitialized { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _gameOverPanel;

    private GameStateManager _gameStateManager;

    public void Initialize()
    {
        if (IsInitialized)
            return;

        if (!ManagerRegistry.TryGet<GameStateManager>(out _gameStateManager))
        {
            Debug.LogError($"{name}: GameStateManager not found.");
            return;
        }

        HideAll();
        _gameStateManager.OnStateChanged += HandleStateChanged;

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

    public void ShowPause()
    {
        if (_pausePanel != null)
            _pausePanel.SetActive(true);

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
    }

    private void OnDestroy()
    {
        if (_gameStateManager != null)
            _gameStateManager.OnStateChanged -= HandleStateChanged;
    }
}