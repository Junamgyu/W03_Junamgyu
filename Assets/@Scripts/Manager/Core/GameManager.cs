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
    [SerializeField] private PauseController _pauseController;
    [SerializeField] private HapticManager _hapticManager;
    // TODO: Add EnemyManager etc.

    [SerializeField] private bool _autoStartInEditor = true;

    #region Debugging
    [ContextMenu("Debug Die")]
    private void DebugDie()
    {
        if (_playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth not found.");
            return;
        }

        _playerHealth.TakeDamage(9999);
    }

    [ContextMenu("Debug Restart")]
    private void DebugRestart()
    {
        RestartGame();
    }
    #endregion

    private Player _player;
    private PlayerHealth _playerHealth;    // 게임 매니저는 플레이어의 체력을 감시

    protected override void OnInitialized()
    {
        base.OnInitialized();

        RegisterManagers(); // Awake에서 매니저 등록
        InitializeManagers();

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded; 

        Debug.Log("GameManager Initialized");

#if UNITY_EDITOR
        if (_autoStartInEditor)
        {
            StartGame();
        }
#endif
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

        _player = FindAnyObjectByType<Player>();
        if (_player == null)
            return;

        _playerHealth = _player.playerHealth;
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

        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            Debug.Log("Player Disabled");
            player.gameObject.SetActive(false);

            // TODO: 죽는 애니메이션 재생 후 비활성화하는 방식으로 변경 필요
        }

        _inputManager.DisablePlayerInput();
        Debug.Log("Input Disabled");

        _gameStateManager.ChangeState(GameState.GameOver);
        Debug.Log("GameState -> GameOver");

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

        if (_pauseController == null)
        {
            Debug.LogError("PauseController is not assigned!");
            return;
        }

        if (_hapticManager == null)
        {
            Debug.LogError("HapticManager is not assigned!");
            return;
        }

        ManagerRegistry.Register<GameManager>(this);
        ManagerRegistry.Register<GameStateManager>(_gameStateManager);
        ManagerRegistry.Register<PoolManager>(_poolManager);
        ManagerRegistry.Register<InputManager>(_inputManager);
        ManagerRegistry.Register<SceneFlowManager>(_sceneManager);
        ManagerRegistry.Register<CheckpointManager>(_checkpointManager);
        ManagerRegistry.Register<PauseController>(_pauseController);
        ManagerRegistry.Register<HapticManager>(_hapticManager);
    }

    // 매니저 초기화는 여기서 진행
    private void InitializeManagers()
    {
        Initialize(_gameStateManager);
        Initialize(_poolManager);
        Initialize(_inputManager);
        Initialize(_sceneManager);
        Initialize(_checkpointManager);
        Initialize(_pauseController);
        Initialize(_hapticManager);
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

        Time.timeScale = 1f;
        _inputManager.EnablePlayerInput();
        _inputManager.EnableUIInput();

        _gameStateManager.ChangeState(GameState.Playing);
        //_sceneManager.LoadStage("JaeinScene"); // Debugging
    }

    // 다시 시작 (마지막 체크포인트로)
    public void RestartGame()
    {
        if (_player == null)
        {
            Debug.LogWarning("Player not found.");
            return;
        }

        PlayerHealth playerHealth = _player.playerHealth;
        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth not found.");
            return;
        }

        Transform respawnPoint = _checkpointManager.CurrentRespawnPoint;
        if (respawnPoint == null)
        {
            Debug.LogWarning("RespawnPoint not found.");
            return;
        }

        _player.transform.position = respawnPoint.position;

        Rigidbody2D rb = _player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        _player.SetActionState(ActionState.None);
        _player.SetLocomotionState(LocomotionState.Idle);
        _player.CanJump = true;

        playerHealth.ResetHP();

        _player.gameObject.SetActive(true); // 플레이어 다시 활성화

        _inputManager.EnablePlayerInput();
        _gameStateManager.ChangeState(GameState.Playing);
    }
}