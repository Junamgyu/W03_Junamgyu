using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeadeyeSkill : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _slowTimeScale = 0.2f;
    [SerializeField] private float _damagePerShot = 50f;
    [SerializeField] private float _timeBetweenShots = 0.1f;
    [SerializeField] private int _maxTargets = 3;

    private bool _isSkillActive = false;
    private bool _isAiming = false;
    private List<EnemyBase> _targets = new List<EnemyBase>();

    Player player;
    Camera _cam;

    public bool IsSkillActive => _isSkillActive;

    void Start()
    {
        player = GetComponent<Player>();
        _cam = Camera.main;
    }

    // 시프트 키
    public void OnSkill(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _isSkillActive = true;
            Time.timeScale = _slowTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        else if (context.canceled)
        {
            ExitSkill();
        }
    }

    public void OnMarkTarget(InputAction.CallbackContext context)
    {
        if (!_isSkillActive) return;

        if (context.started)
        {
            _isAiming = true;
        }
        else if (context.canceled)
        {
            // 좌클릭 떼는 순간 발동
            if (_targets.Count > 0)
                StartCoroutine(FireAtTargets());
        }
    }

    void Update()
    {
        if (!_isSkillActive || !_isAiming) return;
        if (_targets.Count >= _maxTargets) return;

        // 마우스 위치에 적이 있으면 마킹
        Vector2 mouseWorld = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

        if (hit != null)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null && !_targets.Contains(enemy))
            {
                _targets.Add(enemy);
                enemy.ShowMark(true); // 마킹 표시
            }
        }
    }

    IEnumerator FireAtTargets()
    {
        // 시간 복구 후 순서대로 처치
        ExitSkill();

        foreach (EnemyBase enemy in _targets)
        {
            if (enemy != null)
            {
                enemy.ShowMark(false);
                enemy.TakeDamage(_damagePerShot);
            }
            yield return new WaitForSeconds(_timeBetweenShots);
        }

        _targets.Clear();
    }

    void ExitSkill()
    {
        _isSkillActive = false;
        _isAiming = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
