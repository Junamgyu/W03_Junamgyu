using System;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform _respawnPoint;

    private bool _isActivated;

    public event Action<Checkpoint> OnCheckpointReached;

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
        OnCheckpointReached?.Invoke(this);
    }

    public void SetActivated(bool value)
    {
        _isActivated = value;
    }
}