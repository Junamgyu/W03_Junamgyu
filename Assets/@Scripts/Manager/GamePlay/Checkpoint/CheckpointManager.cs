using UnityEngine;

public class CheckpointManager : MonoBehaviour, IInitializable
{
    [SerializeField] private Checkpoint[] _checkpoints;
    [SerializeField] private Transform _currentRespawnPoint;
    [SerializeField] private Transform _startRespawnPoint;

    [SerializeField] private Vector3 _currentRespawnPosition;
    [SerializeField] private Vector3 _startRespawnPosition;

    [SerializeField] private bool _hasReachedCheckpoint;

    private Player _player;

    public Transform CurrentRespawnPoint => _currentRespawnPoint;
    public Vector3 CurrentRespawnPosition => _currentRespawnPosition;

    public bool IsInitialized { get; private set; }
    public Checkpoint CurrentCheckpoint { get; private set; }

    public bool HasCheckpoint => _hasReachedCheckpoint;

    public void Initialize()
    {
        if (IsInitialized)
            return;

        _player = FindFirstObjectByType<Player>();

        BindRespawnPoint();

        if (!_hasReachedCheckpoint)
        {
            _currentRespawnPoint = _startRespawnPoint;
            _currentRespawnPosition = _startRespawnPosition;
        }

        BindCheckpoints();
        IsInitialized = true;
    }

    private void BindRespawnPoint()
    {
        GameObject startPointObject = GameObject.Find("StartPoint");
        if (startPointObject == null)
        {
            Debug.LogWarning($"{name}: StartPoint 오브젝트를 찾지 못했습니다.");
            return;
        }

        _startRespawnPoint = startPointObject.transform;
        _startRespawnPosition = _startRespawnPoint.position;
    }

    private void BindCheckpoints()
    {
        if (_checkpoints == null || _checkpoints.Length == 0)
            _checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);

        foreach (Checkpoint checkpoint in _checkpoints)
        {
            if (checkpoint == null)
                continue;

            checkpoint.OnCheckpointReached -= HandleCheckpointReached;
            checkpoint.OnCheckpointReached += HandleCheckpointReached;
        }
    }

    private void UnbindCheckpoints()
    {
        if (_checkpoints == null)
            return;

        foreach (Checkpoint checkpoint in _checkpoints)
        {
            if (checkpoint == null)
                continue;

            checkpoint.OnCheckpointReached -= HandleCheckpointReached;
        }
    }

    private void HandleCheckpointReached(Checkpoint checkpoint)
    {
        if (checkpoint == null)
            return;

        _hasReachedCheckpoint = true;
        CurrentCheckpoint = checkpoint;
        _currentRespawnPoint = checkpoint.RespawnPoint;
        _currentRespawnPosition = checkpoint.RespawnPosition;

        // TODO: 체력 풀피로 만들기 리스타트 로직 보면 있을거임
        _player = FindFirstObjectByType<Player>();
        PlayerHealth playerHealth = _player.playerHealth;
        playerHealth.ResetHP();


        RefreshCheckpointActivation();
    }

    public void RebindCheckpoints()
    {
        UnbindCheckpoints();

        BindRespawnPoint();

        _checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        BindCheckpoints();

        RebindCurrentCheckpointByPosition();
        RefreshCheckpointActivation();
    }

    private void RebindCurrentCheckpointByPosition()
    {
        CurrentCheckpoint = null;

        if (!_hasReachedCheckpoint)
        {
            _currentRespawnPoint = _startRespawnPoint;
            _currentRespawnPosition = _startRespawnPosition;
            return;
        }

        Checkpoint matchedCheckpoint = null;
        const float tolerance = 0.05f;

        for (int i = 0; i < _checkpoints.Length; i++)
        {
            Checkpoint checkpoint = _checkpoints[i];
            if (checkpoint == null)
                continue;

            float distance = Vector3.Distance(checkpoint.RespawnPosition, _currentRespawnPosition);
            if (distance <= tolerance)
            {
                matchedCheckpoint = checkpoint;
                break;
            }
        }

        if (matchedCheckpoint != null)
        {
            CurrentCheckpoint = matchedCheckpoint;
            _currentRespawnPoint = matchedCheckpoint.RespawnPoint;
            _currentRespawnPosition = matchedCheckpoint.RespawnPosition;
        }
        else
        {
            _currentRespawnPoint = null;
        }
    }

    private void RefreshCheckpointActivation()
    {
        if (_checkpoints == null)
            return;

        for (int i = 0; i < _checkpoints.Length; i++)
        {
            Checkpoint checkpoint = _checkpoints[i];
            if (checkpoint == null)
                continue;

            bool shouldActivate = CurrentCheckpoint != null && checkpoint == CurrentCheckpoint;
            checkpoint.SetActivated(shouldActivate);
        }
    }

    public bool TryGetCheckpointPosition(out Vector3 position)
    {
        position = _currentRespawnPosition;
        return true;
    }

    public void MovePlayerToCheckpoint(Player player)
    {
        if (player == null)
            return;

        player.transform.position = _currentRespawnPosition;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void ClearCheckpoint()
    {
        _hasReachedCheckpoint = false;
        CurrentCheckpoint = null;
        _currentRespawnPoint = _startRespawnPoint;
        _currentRespawnPosition = _startRespawnPosition;
        RefreshCheckpointActivation();
    }

    private void OnDestroy()
    {
        UnbindCheckpoints();
    }
}