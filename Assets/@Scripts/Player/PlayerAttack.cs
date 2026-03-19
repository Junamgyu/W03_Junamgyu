using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    Player player;
    Rigidbody2D _rb;

    // 샷건
    [SerializeField] private SO_WeaponBase _shotgunData;
    private WeaponInstance _shotgunInstance;
    public WeaponInstance Shotgun => _shotgunInstance;

    // 좌클릭 무기 (교체 가능한.)
    [SerializeField] private SO_WeaponBase currentWeaponData;
    private WeaponInstance _currentWeaponInstance;

    public WeaponInstance Current => _currentWeaponInstance;

    private void Start()
    {
        player = GetComponent<Player>();
        _rb = GetComponent<Rigidbody2D>();
        _shotgunInstance = new WeaponInstance(_shotgunData);

        // 좌클릭 기본 무기 넣기
        _currentWeaponInstance = new WeaponInstance(currentWeaponData);

    }

    public void FireShotgun()
    {
        if (!_shotgunInstance.TryConsume()) return;
        Fire(_shotgunData);
        TriggerRecoilRoutines();
    }

    public void FireCurrentWeapon()
    {
        if (player.deadeyeSkill.IsSkillActive) return;

        if (currentWeaponData == null) return;

        if (!_currentWeaponInstance.TryConsume()) return;

        Fire(currentWeaponData);
        TriggerRecoilRoutines();
    }

    public void SwapWeapon(SO_WeaponBase newWeapon)
    {
        currentWeaponData = newWeapon;
        _currentWeaponInstance = new WeaponInstance(newWeapon); // 교체 시 인스턴스도 새로 생성 (기존꺼는 자동으로 GC가 해결.)
    }

    void Fire(SO_WeaponBase data)
    {
        Vector2 aimDir = player.playerAimer.AimDirection;

        // 총알 스폰
        SpawnBullets(data, aimDir);

        // 반동
        Vector2 shootDir = aimDir;
        shootDir.x *= data.shootXMul;
        shootDir.y = shootDir.y < 0
            ? (shootDir.y - 1) / 2
            : (shootDir.y + 1) / 2;

        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(-shootDir * data.recoilForce, ForceMode2D.Impulse);
    }

    void SpawnBullets(SO_WeaponBase data, Vector2 aimDir)
    {
        float baseAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < data.pelletCount; i++)
        {
            float spread = Random.Range(-data.spreadAngle, data.spreadAngle);
            float rad = (baseAngle + spread) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            SpawnBullet(data, dir);
        }
    }

    void SpawnBullet(SO_WeaponBase data, Vector2 dir)
    {
        if (data.bulletPrefab == null) return;
        GameObject bullet = Instantiate(data.bulletPrefab, player.transform.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
            bulletRb.linearVelocity = dir * data.bulletSpeed;
    }

    void TriggerRecoilRoutines()
    {
        StopCoroutine(nameof(GravityRoutine));
        StopCoroutine(nameof(DampingRoutine));
        StartCoroutine(nameof(GravityRoutine));
        StartCoroutine(nameof(DampingRoutine));
        Time.timeScale = 1f;

    }

    IEnumerator GravityRoutine()
    {
        _rb.gravityScale = 0f;
        yield return new WaitForSeconds(player.gravityOffDuration);
        _rb.gravityScale = player.OriginalGravity;
    }

    IEnumerator DampingRoutine()
    {
        player.IsRecoiling = true;
        _rb.linearDamping = player.dampingValue;
        yield return new WaitForSeconds(player.dampingDuration);
        _rb.linearDamping = 0f;
        player.IsRecoiling = false;
    }

    public void ReloadAll()
    {
        _shotgunInstance.Reload();
        _currentWeaponInstance?.Reload();
    }
}
