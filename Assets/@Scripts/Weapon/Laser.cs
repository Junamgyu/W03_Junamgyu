using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private Transform _muzzle;     // 총구 위치
    [SerializeField] private LayerMask _blockLayer; // 레이저 막을 레이어 (Ground, Enemy 등)
    [SerializeField] private float _laserRange = 20f;

    LineRenderer _laser;
    Player _player;


    void Start()
    {
        _laser = GetComponent<LineRenderer>();
        _player = GetComponentInParent<Player>();
    }

    void Update()
    {
        Vector2 aimDir = _player.playerAimer.AimDirection;

        _laser.SetPosition(0, _muzzle.position);

        RaycastHit2D hit = Physics2D.Raycast(_muzzle.position, aimDir, _laserRange, _blockLayer);
        if (hit.collider != null)
            _laser.SetPosition(1, hit.point);
        else
            _laser.SetPosition(1, (Vector2)_muzzle.position + aimDir * _laserRange);
    }
}
