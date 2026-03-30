using System;
using Unity.Mathematics;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform _respawnPoint;
    [SerializeField] private Transform _genPoint;
    [SerializeField] private GameObject _checkpointWall;
    [SerializeField] private GameObject _checkPointMark;

    private bool _isActivated;
    public event Action<Checkpoint> OnCheckpointReached;

    private GameObject _spawnedCheckpointWall;

    public Transform RespawnPoint => _respawnPoint != null ? _respawnPoint : transform;
    public Vector3 RespawnPosition => RespawnPoint.position;
    public bool IsActivated => _isActivated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"체크포인트 충돌 감지 — {other.gameObject.name} / 태그: {other.tag}");
        if (_isActivated)
        {
            Debug.Log("이미 활성화됨 — 무시");
            return;
        }
           

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Player 태그 아님 — 무시");
            return;
        } 
        Debug.Log("체크포인트 활성화!");
        Activate();
        OnCheckpointReached?.Invoke(this);

    }

    public void SetActivated(bool value)
    {
        if(value) Activate();
        else Deactivate();
    }

    public void ResetCheckpoint()
    {
        _isActivated = false;
        ClearCheckpointWall();
    }

    private void Activate()
    {
        _isActivated = true;
        Spawnwall();
    }

    private void Deactivate()
    {
        _isActivated = false;
        ClearCheckpointWall();
    }

    private void Spawnwall()
    {
        if (_checkpointWall == null) return;
        _checkpointWall.SetActive(true);
    }

    private void ClearCheckpointWall()
    {
        if (_spawnedCheckpointWall == null) return;
        _checkpointWall.SetActive(false);
    }
}