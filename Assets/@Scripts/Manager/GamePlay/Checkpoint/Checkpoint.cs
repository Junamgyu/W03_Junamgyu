using System;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform _respawnPoint;
    [SerializeField] private Transform _genPoint;
    [SerializeField] private GameObject _checkpointWall;

    private bool _isActivated;

    public event Action<Checkpoint> OnCheckpointReached;

    private GameObject _spawnedCheckpointWall;

    public Transform RespawnPoint => _respawnPoint != null ? _respawnPoint : transform;
    public Vector3 RespawnPosition => RespawnPoint.position;
    public bool IsActivated => _isActivated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isActivated)
            return;

        if (!other.CompareTag("Player"))
            return;

        _isActivated = true;

        RefreshCheckpointWall();
        OnCheckpointReached?.Invoke(this);
    }

    public void SetActivated(bool value)
    {
        _isActivated = value;
        RefreshCheckpointWall();
    }

    private void RefreshCheckpointWall()
    {
        if (_isActivated)
        {
            TrySpawnCheckpointWall();
        }
        else
        {
            ClearCheckpointWall();
        }
    }

    private void TrySpawnCheckpointWall()
    {
        if (_checkpointWall == null || _genPoint == null)
            return;

        if (_spawnedCheckpointWall != null)
            return;

        _spawnedCheckpointWall = Instantiate(
            _checkpointWall,
            _genPoint.position,
            _genPoint.rotation,
            _genPoint
        );
    }

    private void ClearCheckpointWall()
    {
        if (_spawnedCheckpointWall == null)
            return;

        Destroy(_spawnedCheckpointWall);
        _spawnedCheckpointWall = null;
    }
}