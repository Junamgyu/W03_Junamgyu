using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossRespawnManager : MonoBehaviour
{
    public static BossRespawnManager Instance {get; private set;}
    public int DeathCount {get; private set;} = 0;

    [SerializeField] private String _menuSceneName = "Boss_Main";

    private GameStateManager _gameStateManager;
    private SceneFlowManager _sceneFlowManager;
    private InputManager _inputManger;
    private CheckpointManager _checkPointManger;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode made)
    {
        if(scene.name == _menuSceneName) return;
        
        StartCoroutine(OnGameSceneLoaded());
    }

    IEnumerator OnGameSceneLoaded()
    {
        yield return null;
        yield return null;

        BindManagers();
        BindPlayerDeath();

        _gameStateManager?.ChangeState(GameState.Playing);

        if(_sceneFlowManager != null)
            _sceneFlowManager.OnStageReloadCompleted -= OnStageReloadCompleted;
        if(_sceneFlowManager != null)
            _sceneFlowManager.OnStageReloadCompleted += OnStageReloadCompleted;
    }

    void BindManagers()
    {
        ManagerRegistry.TryGet(out _gameStateManager);
        ManagerRegistry.TryGet(out _sceneFlowManager);
        ManagerRegistry.TryGet(out _inputManger);
        ManagerRegistry.TryGet(out _checkPointManger);

        Debug.Log($"BindManagers -" +
            $"GSM: {_gameStateManager != null}" +
            $"SFM:{_sceneFlowManager != null} " +
            $"IM:{_inputManger != null} " +
            $"CM:{_checkPointManger != null}");
    }

    void BindPlayerDeath()
    {
        var playerHealth = FindFirstObjectByType<PlayerHealth>();
        if(playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth를 찾을 수 없음");
            return;
        }

        playerHealth.OnDie -= OnPlayerDie;
        playerHealth.OnDie += OnPlayerDie;
        Debug.Log("OnPlayerDie 구독 완료");
    }

    void OnPlayerDie()
    {
        Debug.Log($"플레이어 사망 - 죽음 횟수 :{++DeathCount}");

        Time.timeScale = 1f;   
        _inputManger?.DisablePlayerInput();     //사망 후 입력 차단
        _gameStateManager?.ChangeState(GameState.Respawning);
     
        StartCoroutine(DieAndReloadRoutine());
    }

    IEnumerator DieAndReloadRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        if(_sceneFlowManager != null)
            _sceneFlowManager.LoadStage();
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void OnStageReloadCompleted(string sceneName)
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return null; //한 프레임 대기 - 씬 오브젝트 초기화 완료 후
        yield return null;
        
        var player = FindFirstObjectByType<Player>();
        if(player == null) yield break;

        _checkPointManger?.MovePlayerToCheckpoint(player);

        var rb = player.GetComponent<Rigidbody2D>();
        if(rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        //입력 활성화
        _inputManger?.EnablePlayerInput();
        //Playing  상태
        _gameStateManager?.ChangeState(GameState.Playing);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if(_sceneFlowManager != null)
            _sceneFlowManager.OnStageReloadCompleted -= OnStageReloadCompleted;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
