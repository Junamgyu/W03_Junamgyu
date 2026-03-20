using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Eye 연결")]
    public BossEye[] eyes;

    [Header("회전 설정")]
    public float rotationSpeed = 30f; // 초당 회전 각도 (시계방향)

    [Header("레이저 페이즈 설정")]
    public float laserDuration = 5f;  // 눈 활성화 지속 시간
    public float idleDuration = 5f;  // 이후 대기 시간

    [Header("소환 설정")]
    public GameObject[] minionPrefabs;
    public Transform spawnPoint;
    public float spawnInterval = 5f;
    public int spawnCountPerWave = 2;

    // 전체 체력 = Eye 체력 합산
    public float TotalHp => CalculateTotalHp();

    private bool _isDead = false;
    private bool _summonStarted = false;
    private int _prevDeadCount = 0;

    // =====================
    // 생명주기
    // =====================
    void Start()
    {
        StartCoroutine(AttackCycleRoutine());
        StartCoroutine(DeathCheckRoutine());
    }

    void Update()
    {
        // 시계방향 회전 (z축 음수)
        transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
    }

    // =====================
    // 체력 계산
    // =====================
    float CalculateTotalHp()
    {
        float total = 0f;
        foreach (var eye in eyes)
            if (!eye.IsDead)
                total += eye.CurrentHp; // EnemyBase의 protected라면 프로퍼티 열어줘야 함
        return total;
    }

    BossEye[] GetAliveEyes()
    {
        return System.Array.FindAll(eyes, e => !e.IsDead);
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
            BossEye[] alive = GetAliveEyes();
            if (alive.Length == 0) yield break;

            // 1~3 랜덤, 살아있는 눈 수 초과하지 않게 클램프
            int count = Mathf.Min(Random.Range(1, 4), alive.Length);

            // 살아있는 눈 중에서 count만큼 랜덤 비복원 추출
            List<BossEye> targets = PickRandom(alive, count);
            foreach (var eye in targets)
                eye.BeginLaser(laserDuration);

            // 레이저 지속 시간 대기
            yield return new WaitForSeconds(laserDuration);

            // 대기
            yield return new WaitForSeconds(idleDuration);
        }
    }

    List<BossEye> PickRandom(BossEye[] pool, int count)
    {
        // 셔플 후 앞에서 count개 뽑기
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
        // 첫 Eye 사망 시 소환 시작
        if (!_summonStarted)
        {
            _summonStarted = true;
            StartCoroutine(SummonRoutine());
        }

        // 눈 죽을수록 소환 강화
        spawnInterval = Mathf.Max(1f, spawnInterval - 0.5f);
        spawnCountPerWave += 1;
    }

    void Die()
    {
        if (_isDead) return;
        _isDead = true;

        StopAllCoroutines();
        Debug.Log("보스 사망");
        // 보스 사망 처리 (애니메이션, 드랍 등)
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

        for (int i = 0; i < spawnCountPerWave; i++)
        {
            GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];
            Vector3 pos = spawnPoint.position + (Vector3)(Random.insideUnitCircle * 2f);
            Instantiate(prefab, pos, Quaternion.identity);
        }

        Debug.Log($"소환 웨이브: {spawnCountPerWave}마리");
    }
}