using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : EntityBase
{

    [Header("Invincibility")]
    [SerializeField] private float _invincibleDuration = 1.5f;
    private bool _isInvincible = false;
    private HapticManager _hapticManager;

    public bool IsInvincible => _isInvincible;

    public event Action<int> OnHit; // Amount   
    public event Action<int> OnHeal;
    public event Action OnDie;

    private void Awake()
    {
        if (!ManagerRegistry.TryGet<HapticManager>(out _hapticManager))
            _hapticManager = null;
    }

    public override void TakeDamage(int damage)
    {
        if (_isInvincible) return;
        if (_currentHp <= 0f) return;

        base.TakeDamage(damage);

        Debug.Log("currentHp: " + _currentHp + ", damage: " + damage);
        _hapticManager?.PlayPlayerHit();
        OnHit?.Invoke(damage);

        if (_currentHp > 0)
            StartCoroutine(InvincibleRoutine());
    }

    public void Heal(int amount = 1)
    {
        if (_currentHp <= 0f) return;

        int actual = Mathf.Min(amount, _maxHp - _currentHp);
        if (actual <= 0) return; // 이미 풀피

        _currentHp += actual;
        OnHeal?.Invoke(actual);
    }

    // Jaein 추가: 풀피 회복 (리스폰 시 필요)
    public void ResetHP()
    {
        StopAllCoroutines();
        _isInvincible = false;

        int restored = _maxHp - _currentHp;
        _currentHp = _maxHp;

        if (restored > 0)
            OnHeal?.Invoke(restored);
    }

    public override void Die()
    {
        _isInvincible = false;
        StopAllCoroutines();
        OnDie?.Invoke();
        base.Die();
    }

    private IEnumerator InvincibleRoutine()
    {
        _isInvincible = true;
        yield return new WaitForSeconds(_invincibleDuration);
        _isInvincible = false;
    }
}
