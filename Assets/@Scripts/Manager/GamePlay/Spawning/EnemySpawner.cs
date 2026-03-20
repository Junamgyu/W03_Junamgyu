using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Data")]
    [SerializeField] private List<SO_WaveData> _waveDatas = new();

    [Header("Spawn Points")]
    [SerializeField] private List<Transform> _spawnPoints = new();

    [Header("Options")]
    [SerializeField] private bool _autoStart = false;
    [SerializeField] private bool _stopWhenSpawnPointIsShort = true; // 스폰할 적 수가 스폰 위치 수보다 많을 때 멈출지 여부
    [SerializeField, Range(0f, 1f)] private float _changeWaveThreshold = 0.8f;

    private int _currentWaveKilledCount = 0;
    private bool _isWaveSpawnCompleted;
    private bool _isChangingWave = false;

    private int _currentWaveIndex = -1;
    private Coroutine _spawnRoutine;
    private PoolManager _pool;

    private void Awake()
    {
        ManagerRegistry.TryGet(out _pool);
    }

    private void Start()
    {
        _waveDatas.Sort((a, b) => a.id.CompareTo(b.id));

        if (_autoStart)
        {
            StartFirstWave();
        }
    }

    public void StartFirstWave()
    {
        if (_waveDatas == null || _waveDatas.Count == 0)
        {
            Debug.LogWarning($"{name}: WaveData가 없습니다.");
            return;
        }

        StartWave(0);
    }

    public void StartWave(int waveIndex)
    {
        if (waveIndex < 0 || waveIndex >= _waveDatas.Count)
        {
            Debug.LogWarning($"{name}: 잘못된 waveIndex = {waveIndex}");
            return;
        }

        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        _currentWaveIndex = waveIndex;
        _currentWaveKilledCount = 0;
        _isWaveSpawnCompleted = false;
        _isChangingWave = false;

        _spawnRoutine = StartCoroutine(CoSpawnWave(_waveDatas[_currentWaveIndex]));
    }

    private bool ShouldNextWave() // 다음 웨이브로 넘어갈 조건
    {
        if (!_isWaveSpawnCompleted)
            return false;

        if (_currentWaveIndex < 0 || _currentWaveIndex >= _waveDatas.Count)
            return false;

        SO_WaveData waveData = _waveDatas[_currentWaveIndex];
        int totalCount = waveData.enemyPrefabs.Count;

        if (totalCount <= 0)
            return false;

        float threshold = waveData.isBossWave ? 1f : _changeWaveThreshold;
        int requiredKillCount = Mathf.CeilToInt(totalCount * threshold);

        return _currentWaveKilledCount >= requiredKillCount;
    }

    public void StartNextWave()
    {
        int nextIndex = _currentWaveIndex + 1;

        if (nextIndex >= _waveDatas.Count)
        {
            Debug.Log($"{name}: 모든 웨이브 완료");
            // 보상이나 다음 스포너 호출?은 여기에 추가하면 될듯
            return;
        }

        StartWave(nextIndex);
    }

    // 스폰 코루틴: 적을 순차적으로 또는 동시에 스폰
    private IEnumerator CoSpawnWave(SO_WaveData waveData)
    {
        if (waveData == null)
        {
            _spawnRoutine = null;
            yield break;
        }

        if (_spawnPoints == null || _spawnPoints.Count == 0)
        {
            Debug.LogWarning($"{name}: SpawnPoint가 없습니다.");
            _spawnRoutine = null;
            yield break;
        }

        int enemyCount = waveData.enemyPrefabs.Count;
        int pointCount = _spawnPoints.Count;

        if (enemyCount == 0)
        {
            Debug.LogWarning($"{name}: Wave {waveData.id} 에 배치할 적이 없습니다.");
            _spawnRoutine = null;
            yield break;
        }

        if (enemyCount > pointCount)
        {
            Debug.LogWarning($"{name}: Wave {waveData.id} 적 수({enemyCount})가 스폰 위치 수({pointCount})보다 많습니다.");

            if (_stopWhenSpawnPointIsShort)
            {
                _spawnRoutine = null;
                yield break;
            }
        }

        List<Transform> selectedPoints = GetSpawnPointsForWave(enemyCount);

        if (waveData.spawnSequentially)
        {
            for (int i = 0; i < enemyCount; i++)
            {
                SpawnEnemy(waveData.enemyPrefabs[i], selectedPoints[i]);

                if (i < enemyCount - 1)
                {
                    yield return new WaitForSeconds(waveData.spawnInterval);
                }
            }
        }
        else
        {
            for (int i = 0; i < enemyCount; i++)
            {
                SpawnEnemy(waveData.enemyPrefabs[i], selectedPoints[i]);
            }
        }

        _isWaveSpawnCompleted = true;
        _spawnRoutine = null;
    }

    private void SpawnEnemy(GameObject enemyPrefab, Transform spawnPoint)
    {
        if (enemyPrefab == null || spawnPoint == null)
        {
            return;
        }

        GameObject enemyObject;

        if (_pool != null)
        {
            enemyObject = _pool.Get(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            enemyObject = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        DespawnController despawnController = enemyObject.GetComponent<DespawnController>();
        if (despawnController != null)
        {
            despawnController.SetMode(E_DespawnMode.ReturnToPool);
        }

        EnemyBase enemyBase = enemyObject.GetComponent<EnemyBase>();
        if (enemyBase != null)
        {
            // 중복 구독 방어
            // TODO: EnemyBase에 OnDeathFinished 이벤트 추가 후 구독
            //enemyBase.OnDeathFinished -= HandleEnemyDeathFinished;
            //enemyBase.OnDeathFinished += HandleEnemyDeathFinished;
        }
    }

    private List<Transform> GetSpawnPointsForWave(int enemyCount)
    {
        List<Transform> shuffled = new List<Transform>(_spawnPoints);

        for (int i = 0; i < shuffled.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[randomIndex]) = (shuffled[randomIndex], shuffled[i]);
        }

        if (enemyCount <= shuffled.Count)
        {
            return shuffled.GetRange(0, enemyCount);
        }

        List<Transform> result = new List<Transform>(enemyCount);

        for (int i = 0; i < enemyCount; i++)
        {
            result.Add(shuffled[i % shuffled.Count]);
        }

        return result;
    }

    private void HandleEnemyDeathFinished(EnemyBase enemy)
    {
        if (enemy != null)
        {
            // TODO: EnemyBase에 OnDeathFinished 이벤트 추가 후 구독 해제
            //enemy.OnDeathFinished -= HandleEnemyDeathFinished;
        }

        if (_isChangingWave)
            return;

        _currentWaveKilledCount++;

        if (ShouldNextWave())
        {
            _isChangingWave = true;
            StartNextWave();
        }
    }

    public void ClearWaveRoutine()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }

    #region Editor Gizmos
    private void OnDrawGizmosSelected()
    {
        if (_spawnPoints == null || _spawnPoints.Count == 0)
            return;

        Vector3 from = transform.position;

        // Draw only circles at spawn points (no connecting lines)
        Gizmos.color = Color.cyan;
        foreach (var pt in _spawnPoints)
        {
            if (pt == null) continue;
            Gizmos.DrawWireSphere(pt.position, 0.5f);
        }
    }
    #endregion
}