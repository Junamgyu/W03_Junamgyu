using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Eye 연결")]
    public BossEye[] eyes;

    [Header("회전 설정")]
    public float rotationSpeed = 30f;

    [Header("레이저 페이즈 설정")]
    public float laserDuration = 5f;
    public float idleDuration = 5f;

    [Header("소환 설정")]
    public GameObject[] minionPrefabs;
    public Transform spawnPoint;
    public float spawnInterval = 5f;
    public int spawnCountPerWave = 2;

    public float TotalHp => CalculateTotalHp();

    private bool _isDead = false;
    private bool _summonStarted = false;
    private int _prevDeadCount = 0;

    void Start()
    {
        StartCoroutine(AttackCycleRoutine());
        StartCoroutine(DeathCheckRoutine());
    }

    void Update()
    {
        if (_isDead) return;
        transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
    }

    // =====================
    // 체력
    // =====================
    float CalculateTotalHp()
    {
        float total = 0f;
        foreach (var eye in eyes)
            if (!eye.IsDead)
                total += eye.CurrentHp;
        return total;
    }

    BossEye[] GetAliveEyes()
    {
        return System.Array.FindAll(eyes, e => !e.IsDead);
    }

    // CanBeginLaser인 Eye만 — 전환 중인 Eye 제외
    BossEye[] GetReadyEyes()
    {
        return System.Array.FindAll(eyes, e => e.CanBeginLaser);
    }

    int DeadCount()
    {
        return System.Array.FindAll(eyes, e => e.IsDead).Length;
    }

    // =====================
    // 공격 사이클
    // =====================
    IEnumerator AttackCycleRoutine()
    {
        while (!_isDead)
        {
            // 준비된 Eye 기준으로 뽑기
            BossEye[] ready = GetReadyEyes();

            if (ready.Length > 0)
            {
                int count = Mathf.Min(Random.Range(1, 4), ready.Length);
                List<BossEye> targets = PickRandom(ready, count);
                foreach (var eye in targets)
                    eye.BeginLaser(laserDuration);
            }

            yield return new WaitForSeconds(laserDuration + idleDuration);
        }
    }

    List<BossEye> PickRandom(BossEye[] pool, int count)
    {
        List<BossEye> list = new List<BossEye>(pool);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list.GetRange(0, count);
    }

    // =====================
    // 사망 / 소환 감지
    // =====================
    IEnumerator DeathCheckRoutine()
    {
        while (!_isDead)
        {
            int deadNow = DeadCount();

            if (deadNow > _prevDeadCount)
            {
                OnEyeDied(deadNow);
                _prevDeadCount = deadNow;
            }

            if (deadNow >= eyes.Length)
            {
                Die();
                yield break;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    void OnEyeDied(int deadCount)
    {
        if (!_summonStarted)
        {
            _summonStarted = true;
            StartCoroutine(SummonRoutine());
        }

        spawnInterval = Mathf.Max(1f, spawnInterval - 0.5f);
        spawnCountPerWave += 1;
    }

    void Die()
    {
        if (_isDead) return;
        _isDead = true;
        StopAllCoroutines();
        Debug.Log("보스 사망");
        gameObject.SetActive(false);
    }

    // =====================
    // 소환 루틴
    // =====================
    IEnumerator SummonRoutine()
    {
        while (!_isDead)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnWave();
        }
    }

    void SpawnWave()
    {
        if (minionPrefabs.Length == 0) return;
        if (spawnPoint == null)
        {
            Debug.LogWarning("BossController: spawnPoint가 할당되지 않았습니다.");
            return;
        }

        for (int i = 0; i < spawnCountPerWave; i++)
        {
            GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];
            Vector3 pos = spawnPoint.position + (Vector3)(Random.insideUnitCircle * 2f);
            Instantiate(prefab, pos, Quaternion.identity);
        }
    }
}