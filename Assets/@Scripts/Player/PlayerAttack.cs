using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerAttack : MonoBehaviour
{
    Player _player;
    Rigidbody2D _rb;

    // 샷건
    [SerializeField] private SO_WeaponBase _shotgunData;
    private WeaponInstance _shotgunInstance;
    public WeaponInstance Shotgun => _shotgunInstance;

    // 좌클릭 무기 (교체 가능한.)
    [SerializeField] private SO_WeaponBase currentWeaponData;
    private WeaponInstance _currentWeaponInstance;

    public WeaponInstance Current => _currentWeaponInstance;

    // 머리 쿵 관련
    [SerializeField] private Transform _ceilingCheck;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _ceilingCheckRadius = 0.1f;

    private PoolManager _poolManager;

    private void Start()
    {
        _player = GetComponent<Player>();
        _rb = GetComponent<Rigidbody2D>();
        _shotgunInstance = new WeaponInstance(_shotgunData);

        // 좌클릭 기본 무기 넣기
        _currentWeaponInstance = new WeaponInstance(currentWeaponData);

        // 풀매니저 세팅
        if (!ManagerRegistry.TryGet<PoolManager>(out _poolManager))
        {
            _poolManager = null;
        }

    }

    public void FireShotgun()
    {
        if (!TryFireWeapon(_shotgunInstance)) return;
        Fire(_shotgunData);

        // 공중에서 쐈으면 공중 반동 상태 진입
        if (!_player.IsGrounded)
            _player.HasAirRecoil = true;
    }

    public void FireCurrentWeapon()
    {
        if (_player.deadeyeSkill.IsDeadeyeActive) return;
        if (currentWeaponData == null) return;
        if (!TryFireWeapon(_currentWeaponInstance)) return;

        Fire(currentWeaponData);

        // 공중에서 쐈으면 공중 반동 상태 진입
        if (!_player.IsGrounded)
            _player.HasAirRecoil = true;
    }

    void Fire(SO_WeaponBase data)
    {
        Vector2 aimDir = _player.playerAimer.AimDirection;

        // 총알 스폰
        SpawnBullets(data, aimDir); // 총알은 정확한 마우스 방향으로

        // 반동
        Vector2 shootDir = SnapTo8Direction(aimDir); // 반동만 8방향 스냅

        // 디버그용 저장
        _debugAimDir = aimDir;
        _debugShootDir = shootDir;

        // X만 초기화, Y는 보존 (점프 중 샷건 쏴도 Y속도 안 날아감)
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        _rb.AddForce(-shootDir * data.recoilForce, ForceMode2D.Impulse);

        TriggerRecoilRoutines(shootDir);
    }

    // 총알이 없을 시 땅이면 재장전.
    bool TryFireWeapon(WeaponInstance instance)
    {
        if (!instance.TryConsume())
        {
            if (!_player.IsGrounded) return false;
            ReloadAll();
            if (!instance.TryConsume()) return false;
        }
        return true;
    }

    Vector2 SnapTo8Direction(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 45도 단위로 반올림
        float snapped = Mathf.Round(angle / 45f) * 45f;

        float rad = snapped * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
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

        Vector3 spawnPos = _player.transform.position;
        GameObject bullet;

        // 풀매니저 연동: 풀매니저가 있으면 풀에서, 없으면 Instantiate
        if (_poolManager != null)
        {
            bullet = _poolManager.Get(data.bulletPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            bullet = Instantiate(data.bulletPrefab, _player.transform.position, Quaternion.identity);
        }

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
            bulletRb.linearVelocity = dir * data.bulletSpeed;
    }

    void TriggerRecoilRoutines(Vector2 shootDir)
    {
        StopCoroutine(nameof(GravityRoutine));
        StopCoroutine(nameof(DampingRoutine));
        StartCoroutine(nameof(GravityRoutine));
        StartCoroutine(nameof(DampingRoutine));
        //Time.timeScale = 1f;

    }

    IEnumerator GravityRoutine()
    {
        _player.IsGravityOverridden = true;
        _rb.gravityScale = 0f;

        float elapsed = 0f;
        while (elapsed < _player.gravityOffDuration)
        {
            // 천장 감지 시 즉시 중단
            if (Physics2D.OverlapCircle(
                _ceilingCheck.position,
                _ceilingCheckRadius,
                _groundLayer))
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
                break;
            }
            elapsed += Time.deltaTime;
            yield return null; // 매 프레임 체크
        }
        _rb.gravityScale = _player.OriginalGravity;
        _player.IsGravityOverridden = false;
    }

    IEnumerator DampingRoutine()
    {
        _player.IsRecoiling = true;
        _rb.linearDamping = _player.dampingValue;
        yield return new WaitForSeconds(_player.dampingDuration);
        _rb.linearDamping = 0f;
        _player.IsRecoiling = false;
    }

    public void ReloadAll()
    {
        _shotgunInstance.Reload();
        _currentWeaponInstance?.Reload();
    }

    // 무기 추가 시 필요.
    public void SwapWeapon(SO_WeaponBase newWeapon)
    {
        currentWeaponData = newWeapon;
        _currentWeaponInstance = new WeaponInstance(newWeapon); // 교체 시 인스턴스도 새로 생성 (기존꺼는 자동으로 GC가 해결.)
    }

    // 디버그용 필드
    Vector2 _debugAimDir;
    Vector2 _debugShootDir;

    void OnDrawGizmos()
    {
        // 노란색 = 실제 마우스 방향 (총알 방향)
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, _debugAimDir * 2f);

        // 빨간색 = 스냅된 반동 방향
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -_debugShootDir * 2f);
    }
}