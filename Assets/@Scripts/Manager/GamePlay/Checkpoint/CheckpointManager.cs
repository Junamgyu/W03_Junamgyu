using UnityEngine;
using UnityEngine.UIElements;

public class CheckpointManager : MonoBehaviour, IInitializable
{
    [SerializeField] private Checkpoint[] _checkpoints;
    [SerializeField] private Transform _currentRespawnPoint;
    public Transform CurrentRespawnPoint => _currentRespawnPoint;

    public bool IsInitialized { get; private set; }
    public Checkpoint CurrentCheckpoint { get; private set; }

    public bool HasCheckpoint => CurrentCheckpoint != null;

    public void Initialize()
    {
        if (IsInitialized)
            return;

        BindCheckpoints(); // 체크포인트 이벤트 바인딩
        IsInitialized = true;
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

    private void HandleCheckpointReached(Checkpoint checkpoint)
    {
        if (checkpoint == null)
            return;

        CurrentCheckpoint = checkpoint;
        _currentRespawnPoint = checkpoint.RespawnPoint;

        Debug.Log($"Checkpoint Updated: {_currentRespawnPoint.name}");
    }

    public bool TryGetCheckpointPosition(out Vector3 position)
    {
        if (_currentRespawnPoint == null)
        {
            position = Vector3.zero;
            return false;
        }

        position = _currentRespawnPoint.position;
        return true;
    }

    public void ClearCheckpoint()
    {
        CurrentCheckpoint = null;
    }

    private void OnDestroy()
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
}