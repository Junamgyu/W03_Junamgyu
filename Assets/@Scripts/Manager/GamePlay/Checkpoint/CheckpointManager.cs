using UnityEngine;

public class CheckpointManager : MonoBehaviour, IInitializable
{
    [SerializeField] private Checkpoint[] _checkpoints;
    [SerializeField] private Transform _currentRespawnPoint;
    [SerializeField] private Transform _startRespawnPoint;

    [SerializeField] private Vector3 _currentRespawnPosition;
    [SerializeField] private Vector3 _startRespawnPosition;

    public Transform CurrentRespawnPoint => _currentRespawnPoint;
    public Vector3 CurrentRespawnPosition => _currentRespawnPosition;

    public bool IsInitialized { get; private set; }
    public Checkpoint CurrentCheckpoint { get; private set; }

    public bool HasCheckpoint => CurrentCheckpoint != null;

    public void Initialize()
    {
        if (IsInitialized)
            return;

        CacheStartRespawnPoint();
        BindCheckpoints();
        IsInitialized = true;
    }

    private void CacheStartRespawnPoint()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player == null)
            return;

        _startRespawnPosition = player.transform.position;

        if (_startRespawnPoint != null)
            _startRespawnPoint.position = _startRespawnPosition;

        if (!HasCheckpoint)
        {
            _currentRespawnPosition = _startRespawnPosition;
            _currentRespawnPoint = _startRespawnPoint;
        }
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

        CurrentCheckpoint = checkpoint;
        _currentRespawnPoint = checkpoint.RespawnPoint;
        _currentRespawnPosition = checkpoint.RespawnPosition;

        RefreshCheckpointActivation();
    }

    public void RebindCheckpoints()
    {
        UnbindCheckpoints();

        _checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        BindCheckpoints();
        RebindCurrentCheckpointByPosition();
        RefreshCheckpointActivation();
    }

    private void RebindCurrentCheckpointByPosition()
    {
        CurrentCheckpoint = null;
        _currentRespawnPoint = _startRespawnPoint;

        if (_checkpoints == null || _checkpoints.Length == 0)
            return;

        const float tolerance = 0.05f;

        for (int i = 0; i < _checkpoints.Length; i++)
        {
            Checkpoint checkpoint = _checkpoints[i];
            if (checkpoint == null)
                continue;

            float distance = Vector3.Distance(checkpoint.RespawnPosition, _currentRespawnPosition);
            if (distance <= tolerance)
            {
                CurrentCheckpoint = checkpoint;
                _currentRespawnPoint = checkpoint.RespawnPoint;
                return;
            }
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