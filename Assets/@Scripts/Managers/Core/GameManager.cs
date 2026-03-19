using UnityEngine;

public class GameManager : PersistentMonoSingleton<GameManager>
{
    [Header("Core Managers")]
    [SerializeField] private GameStateManager _gameStateManager;
    [SerializeField] private SceneFlowManager _sceneFlowManager;
    [SerializeField] private PoolManager _poolManager;
    [SerializeField] private InputManager _inputManager;
    // TODO: Add EnemyManager etc.


    protected override void OnInitialized()
    {
        base.OnInitialized();

        RegisterManagers(); // Awake에서 매니저 등록
        InitializeManagers();

        Debug.Log("GameManager Initialized");
    }

    // 매니저 등록은 Awake에서 진행
    private void RegisterManagers()
    {
        if (_gameStateManager == null)
        {
            Debug.LogError("GameStateManager is not assigned!");
            return;
        }

        if (_sceneFlowManager == null)
        {
            Debug.LogError("SceneFlowManager is not assigned!");
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

        ManagerRegistry.Register<GameManager>(this);
        ManagerRegistry.Register<GameStateManager>(_gameStateManager);
        ManagerRegistry.Register<PoolManager>(_poolManager);
        ManagerRegistry.Register<InputManager>(_inputManager);
        ManagerRegistry.Register<SceneFlowManager>(_sceneFlowManager);
    }

    // 매니저 초기화는 여기서 진행
    private void InitializeManagers()
    {
        Initialize(_gameStateManager);
        Initialize(_poolManager);
        Initialize(_inputManager);
        Initialize(_sceneFlowManager);
    }

    private void Initialize(IInitializable manager)
    {
        if (manager == null) return;

        if (!manager.IsInitialized)
            manager.Initialize();
    }

    public void StartGame()
    {
        if (_sceneFlowManager == null)
        {
            Debug.LogError("SceneFlowManager is missing!");
            return;
        }

        Debug.Log("Game Start!");
        _gameStateManager.ChangeState(GameState.Playing);
        _sceneFlowManager.LoadStage("Stage_01");
    }
}