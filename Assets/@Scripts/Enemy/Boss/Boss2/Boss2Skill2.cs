using System.Collections;
using UnityEngine;

public class Boss2Skill2 : MonoBehaviour, ISkill
{
    [Header("이동 설정")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 4f;

    [Header("발사 설정")]
    public Transform[] turrets;
    public GameObject bulletPrefab;
    public int bulletDamage = 10;
    public float bulletSpeed = 8f;
    public float fireInterval = 0.5f;
    public float skillDuration = 5f;

    private PoolManager _pool;

    void Awake()
    {
        ManagerRegistry.TryGet(out _pool);
    }

    public IEnumerator SkillRoutine()
    {
        Coroutine moveCoroutine = StartCoroutine(MoveRoutine());
        yield return StartCoroutine(FireRoutine());
        StopCoroutine(moveCoroutine);
    }

    // =====================
    // 이동 루틴 (A↔B 왕복)
    // =====================
    IEnumerator MoveRoutine()
    {
        Transform current = pointA;

        while (true)
        {
            yield return StartCoroutine(MoveToPosition(current.position));
            current = current == pointA ? pointB : pointA;
        }
    }

    IEnumerator MoveToPosition(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }

    // =====================
    // 발사 루틴
    // =====================
    IEnumerator FireRoutine()
    {
        float elapsed = 0f;
        while (elapsed < skillDuration)
        {
            FireAllTurrets();
            yield return new WaitForSeconds(fireInterval);
            elapsed += fireInterval;
        }
    }

    void FireAllTurrets()
    {
        foreach (var turret in turrets)
        {
            if (turret == null) continue;
            FireFromTurret(turret);
        }
    }

    void FireFromTurret(Transform turret)
    {
        // EnemyProjectile은 right 방향으로 이동
        // 포탑 up이 발사 방향이면 -90도 보정
        Quaternion rotation = turret.rotation * Quaternion.Euler(0f, 0f, -90f);

        GameObject bullet = _pool != null
            ? _pool.Get(bulletPrefab, turret.position, rotation)
            : Instantiate(bulletPrefab, turret.position, rotation);

        bullet.GetComponent<EnemyProjectile>()?.Initialize(bulletSpeed, bulletDamage);
    }
}