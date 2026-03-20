using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : PersistentMonoSingleton<GameManager>
{
    [Header("Core Managers")]
    [SerializeField] private GameStateManager _gameStateManager;
    [SerializeField] private SceneFlowManager _sceneManager;
    [SerializeField] private PoolManager _poolManager;
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private CheckpointManager _checkpointManager;
    // TODO: Add EnemyManager etc.

    // 게임 매니저는 플레이어의 체력을 감시
    private PlayerHealth _playerHealth;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        RegisterManagers(); // Awake에서 매니저 등록
        InitializeManagers();

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded; 

        Debug.Log("GameManager Initialized");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindPlayerHealth();
    }

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnbindPlayerHealth();
    }

    private void BindPlayerHealth()
    {
        UnbindPlayerHealth();

        Player player = FindAnyObjectByType<Player>();
        if (player == null)
            return;

        _playerHealth = player.playerHealth;
        if (_playerHealth == null)
            return;

        _playerHealth.OnDie += HandlePlayerDie;
    }

    private void UnbindPlayerHealth()
    {
        if (_playerHealth == null)
            return;

        _playerHealth.OnDie -= HandlePlayerDie;
        _playerHealth = null;
    }

    private void HandlePlayerDie()
    {
        Debug.Log("Player Die");
        _gameStateManager.ChangeState(GameState.GameOver);

        // TODO: Show Game Over UI, Restart Button, etc.
    }

    // 매니저 등록은 Awake에서 진행
    private void RegisterManagers()
    {
        if (_gameStateManager == null)
        {
            Debug.LogError("GameStateManager is not assigned!");
            return;
        }

        if (_sceneManager == null)
        {
            Debug.LogError("SceneManager is not assigned!");
            return;
        }

        if (_poolManager == null)
        {
            Debug.LogError("PoolManager is not assigned!");
            return;
        }

        if (_inputManager == null)
        {
            Debug.LogError("InputManager is not assigned!");
            return;
        }

        if (_checkpointManager == null)
        {
            Debug.LogError("CheckpointManager is not assigned!");
            return;
        }

        ManagerRegistry.Register<GameManager>(this);
        ManagerRegistry.Register<GameStateManager>(_gameStateManager);
        ManagerRegistry.Register<PoolManager>(_poolManager);
        ManagerRegistry.Register<InputManager>(_inputManager);
        ManagerRegistry.Register<SceneFlowManager>(_sceneManager);
        ManagerRegistry.Register<CheckpointManager>(_checkpointManager);
    }

    // 매니저 초기화는 여기서 진행
    private void InitializeManagers()
    {
        Initialize(_gameStateManager);
        Initialize(_poolManager);
        Initialize(_inputManager);
        Initialize(_sceneManager);
        Initialize(_checkpointManager);
    }

    private void Initialize(IInitializable manager)
    {
        if (manager == null) return;

        if (!manager.IsInitialized)
            manager.Initialize();
    }

    public void StartGame()
    {
        if (_sceneManager == null)
        {
            Debug.LogError("SceneFlowManager is missing!");
            return;
        }

        Debug.Log("Game Start!");
        _gameStateManager.ChangeState(GameState.Playing);
        _sceneManager.LoadStage("Stage_01");
    }

    public void RestartGame()
    {
        // TODO: Restart Game Logic (e.g., Reset Score, Clear Enemies, etc.)
    }
}